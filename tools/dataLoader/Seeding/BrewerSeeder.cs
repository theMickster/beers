using System.Globalization;
using System.Net;
using BeersDataLoader.Infrastructure;
using BeersDataLoader.Models;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace BeersDataLoader.Seeding;

internal static class BrewerSeeder
{
    internal static async Task<SeedResult> SeedAsync(Container container, string dataFilePath)
    {
        if (!File.Exists(dataFilePath))
        {
            Console.WriteLine($"Invalid path to brewer entities data: {dataFilePath}");
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
                skippedItems++;
                continue;
            }

            item["id"] = id;
            item["BrewerId"] = brewerId;
            item["EntityType"] = PartitionKeyConstants.Brewer;
            item["isDeletable"] = CoerceBoolean(item["isDeletable"]);

            NormalizeDate(item, "CreatedDate");
            NormalizeDate(item, "ModifiedDate");
            NormalizeBreweryType(item["BreweryType"] as JObject);
            EnsureRating(item);

            var partitionKey = new PartitionKeyBuilder()
                .Add(brewerId)
                .Add(PartitionKeyConstants.Brewer)
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
                    Console.WriteLine($"Failed to create brewer entity with id: {id}");
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

    private static void NormalizeBreweryType(JObject? breweryType)
    {
        if (breweryType == null)
        {
            return;
        }

        var metadataId = ((string?)breweryType["MetadataId"])?.Trim().ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(metadataId))
        {
            breweryType["MetadataId"] = metadataId;
        }

        var name = ((string?)breweryType["Name"])?.Trim();
        if (!string.IsNullOrWhiteSpace(name))
        {
            breweryType["Name"] = name;
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