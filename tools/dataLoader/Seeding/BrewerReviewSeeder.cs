using System.Net;
using BeersDataLoader.Infrastructure;
using BeersDataLoader.Models;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace BeersDataLoader.Seeding;

internal static class BrewerReviewSeeder
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
                skippedItems++;
                continue;
            }

            item["id"] = id;
            item["BrewerId"] = brewerId;
            item["EntityType"] = PartitionKeyConstants.BrewerReview;

            var partitionKey = new PartitionKeyBuilder()
                .Add(brewerId)
                .Add(PartitionKeyConstants.BrewerReview)
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
}
