using System.Net;
using BeersDataLoader.Infrastructure;
using BeersDataLoader.Models;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace BeersDataLoader.Seeding;

internal static class NewsBlogPostSeeder
{
    /// <summary>Seeds news/blog post documents into <paramref name="container"/>, skipping items that already exist.</summary>
    /// <param name="container">Target Cosmos container (hierarchical partition key: BrewerId / EntityType).</param>
    /// <param name="dataFilePath">Absolute path to the JSON seed file; returns <see cref="SeedResult.Invalid"/> if the file is missing.</param>
    /// <returns>A <see cref="SeedResult"/> with total, created, and skipped counts.</returns>
    internal static async Task<SeedResult> SeedAsync(Container container, string dataFilePath)
    {
        if (!File.Exists(dataFilePath))
        {
            Console.WriteLine($"Invalid path to news blog post entities data: {dataFilePath}");
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
            item["EntityType"] = PartitionKeyConstants.NewsBlogPost;

            var author = item["Author"] as JObject;
            if (author != null)
            {
                var authorId = ((string?)author["id"])?.Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(authorId))
                    author["id"] = authorId;

                var authorName = ((string?)author["name"])?.Trim();
                if (!string.IsNullOrWhiteSpace(authorName))
                    author["name"] = authorName;

                var authorWebsite = ((string?)author["website"])?.Trim();
                if (!string.IsNullOrWhiteSpace(authorWebsite))
                    author["website"] = authorWebsite;
            }

            var partitionKey = new PartitionKeyBuilder()
                .Add(brewerId)
                .Add(PartitionKeyConstants.NewsBlogPost)
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
                    Console.WriteLine($"Failed to create news blog post with id: {id}");
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

    /// <summary>Extracts and normalizes a string property to lowercase so Cosmos IDs and partition keys remain consistent across seed runs.</summary>
    /// <param name="item">The JSON object containing the property.</param>
    /// <param name="propertyName">Name of the property to extract.</param>
    /// <returns>Trimmed, lowercase value, or <see cref="string.Empty"/> if the property is null or whitespace.</returns>
    private static string NormalizeId(JObject item, string propertyName)
    {
        return ((string?)item[propertyName])?.Trim().ToLowerInvariant() ?? string.Empty;
    }
}
