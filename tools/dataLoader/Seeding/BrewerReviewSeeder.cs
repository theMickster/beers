using System.Net;
using BeersDataLoader.Infrastructure;
using BeersDataLoader.Models;
using Microsoft.Azure.Cosmos;

namespace BeersDataLoader.Seeding;

internal sealed class BrewerReviewSeeder
{
    internal async Task<SeedResult> SeedAsync(Container container, string dataFilePath)
    {
        if (!File.Exists(dataFilePath))
        {
            Console.WriteLine($"Invalid path to beer entities data: {dataFilePath}");
            return SeedResult.Invalid;
        }

        var items = await JsonFileStore.ReadListAsync<dynamic>(dataFilePath);
        var createdItems = 0;
        var skippedItems = 0;

        foreach (var item in items)
        {
            var createItem = false;
            var id = ((string)item.id).ToLowerInvariant();
            var brewerId = ((string)item.BrewerId).ToLowerInvariant();
            var entityType = ((string)item.EntityType);
            var partitionKey = new PartitionKeyBuilder()
                .Add(brewerId)
                .Add(entityType)
                .Build();

            try
            {
                await container.ReadItemAsync<dynamic>(id, partitionKey);
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
}
