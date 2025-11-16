using System.Globalization;
using System.Net;
using BeersDataLoader.Infrastructure;
using BeersDataLoader.Models;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace BeersDataLoader.Seeding;

internal static class BeerSeeder
{
    internal static async Task<SeedResult> SeedAsync(Container container, string dataFilePath)
    {
        if (!File.Exists(dataFilePath))
        {
            Console.WriteLine($"Invalid path to beer entities data: {dataFilePath}");
            return SeedResult.Invalid;
        }

        var items = await JsonFileStore.ReadListAsync<JObject>(dataFilePath);
        var createdItems = 0;
        var skippedItems = 0;

        foreach (var item in items)
        {
            var id = NormalizeId(item, "id");
            var brewerId = NormalizeId(item, "BrewerId");

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(brewerId))
            {
                Console.WriteLine($"Skipping beer — id: '{id}', BrewerId: '{brewerId}'");
                skippedItems++;
                continue;
            }

            item["id"] = id;
            item["BrewerId"] = brewerId;
            item["EntityType"] = PartitionKeyConstants.Beer;
            item["isDeletable"] = CoerceBoolean(item["isDeletable"]);

            NormalizeDate(item, "CreatedDate");
            NormalizeDate(item, "ModifiedDate");
            NormalizeSlimMetadata(item["BeerType"] as JObject);
            NormalizeSlimMetadataArray(item["BeerStyles"] as JArray);
            NormalizeSlimMetadataArray(item["BeerCategories"] as JArray);
            NormalizeBrewerSlim(item["Brewer"] as JObject);
            EnsureRating(item);

            var partitionKey = new PartitionKeyBuilder()
                .Add(brewerId)
                .Add(PartitionKeyConstants.Beer)
                .Build();

            var createItem = false;
            try
            {
                await container.ReadItemAsync<JObject>(id, partitionKey);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                createItem = true;
            }

            if (!createItem)
            {
                skippedItems++;
                continue;
            }

            try
            {
                var createResponse = await container.CreateItemAsync(item, partitionKey);
                if (createResponse.StatusCode != HttpStatusCode.Created)
                {
                    Console.WriteLine($"Failed to create beer entity with id: {id}");
                }
                else
                {
                    createdItems++;
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                skippedItems++;
            }
        }

        return new SeedResult(items.Count, createdItems, skippedItems);
    }

    private static string NormalizeId(JObject item, string propertyName)
    {
        return ((string?)item[propertyName])?.Trim().ToLowerInvariant() ?? string.Empty;
    }

    private static bool CoerceBoolean(JToken? token)
    {
        if (token == null)
        {
            return true;
        }

        return token.Type switch
        {
            JTokenType.Boolean => token.Value<bool>(),
            JTokenType.String when bool.TryParse(token.Value<string>(), out var parsed) => parsed,
            _ => true
        };
    }

    private static void NormalizeDate(JObject item, string propertyName)
    {
        var value = ((string?)item[propertyName])?.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var parsed))
        {
            item[propertyName] = parsed.ToUniversalTime().ToString("O");
        }
    }

    private static void NormalizeSlimMetadata(JObject? slim)
    {
        if (slim == null)
        {
            return;
        }

        var metadataId = ((string?)slim["metadataId"])?.Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(metadataId))
        {
            slim["metadataId"] = metadataId;
        }

        var name = ((string?)slim["name"])?.Trim();
        if (!string.IsNullOrWhiteSpace(name))
        {
            slim["name"] = name;
        }
    }

    private static void NormalizeSlimMetadataArray(JArray? array)
    {
        if (array == null)
        {
            return;
        }

        foreach (var element in array.OfType<JObject>())
        {
            NormalizeSlimMetadata(element);
        }
    }

    private static void NormalizeBrewerSlim(JObject? brewer)
    {
        if (brewer == null)
        {
            return;
        }

        var id = ((string?)brewer["id"])?.Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(id))
        {
            brewer["id"] = id;
        }

        var name = ((string?)brewer["name"])?.Trim();
        if (!string.IsNullOrWhiteSpace(name))
        {
            brewer["name"] = name;
        }

        var website = ((string?)brewer["website"])?.Trim();
        if (!string.IsNullOrWhiteSpace(website))
        {
            brewer["website"] = website;
        }
    }

    private static void EnsureRating(JObject item)
    {
        if (item["Rating"] is JObject existing)
        {
            existing["average"] ??= 0m;
            existing["reviews"] ??= 0;
            return;
        }

        item["Rating"] = new JObject
        {
            ["average"] = 0m,
            ["reviews"] = 0
        };
    }
}
