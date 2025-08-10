using System.Net;
using BeersDataLoader.Entities;
using BeersDataLoader.Infrastructure;
using BeersDataLoader.Models;
using Microsoft.Azure.Cosmos;

namespace BeersDataLoader.Seeding.Metadata;

internal sealed class MetadataSeeder
{
    internal async Task<SeedResult> SeedAsync<T>(Container container, string metadataFilePath) where T : BaseMetadataEntity
    {
        if (!File.Exists(metadataFilePath))
        {
            Console.WriteLine($"Invalid path to {typeof(T)} metadata: {metadataFilePath}");
            return SeedResult.Invalid;
        }

        var items = await JsonFileStore.ReadListAsync<T>(metadataFilePath);
        var createdItems = 0;
        var skippedItems = 0;

        foreach (var item in items)
        {
            var createItem = false;
            var partitionKey = new PartitionKeyBuilder()
                .Add(item.ApplicationName)
                .Add(item.TypeId.ToString().ToLowerInvariant())
                .Build();

            try
            {
                await container.ReadItemAsync<T>(item.Id.ToString().ToLowerInvariant(), partitionKey);

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

            var createResponse = await container.CreateItemAsync(item, partitionKey);
            if (createResponse.StatusCode != HttpStatusCode.Created)
            {
                Console.WriteLine($"Failed to create {typeof(T)} : {item.Name}");
            }
            else
            {
                createdItems++;
            }
        }

        return new SeedResult(items.Count, createdItems, skippedItems);
    }
}
