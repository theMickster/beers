using System.Net;
using Microsoft.Azure.Cosmos;

namespace BeersDataLoader.Infrastructure;

internal static class CosmosProvisioner
{
    internal static async Task<Database> EnsureDatabaseAsync(CosmosClient client, string databaseName)
    {
        var databaseResponse = await client.CreateDatabaseIfNotExistsAsync(databaseName, 1000);
        switch (databaseResponse.StatusCode)
        {
            case HttpStatusCode.Created:
                Console.WriteLine($"A new Cosmos Database named '{databaseName}' was successfully created.");
                break;
            case HttpStatusCode.OK:
                Console.WriteLine(
                    $"A Cosmos Database named '{databaseName}' already exists for the configured Cosmos Db instance.");
                break;
            default:
                throw new InvalidOperationException(
                    $"Unable to create or locate a Cosmos Database named '{databaseName}'. Status Code: {databaseResponse.StatusCode}");
        }

        return databaseResponse.Database;
    }

    internal static async Task<Container> EnsureMetadataContainerAsync(Database db, string databaseName, string containerName)
    {
        List<string> metadataPartitionKeys = ["/ApplicationName", "/TypeId"];
        var metadataContainerProperties = new ContainerProperties(containerName, partitionKeyPaths: metadataPartitionKeys);

        var container = await db.CreateContainerIfNotExistsAsync(metadataContainerProperties, 400);
        switch (container.StatusCode)
        {
            case HttpStatusCode.Created:
                Console.WriteLine($"A new Cosmos Container named '{containerName}' was successfully created for the Cosmos Db: '{databaseName}'.");
                break;
            case HttpStatusCode.OK:
                Console.WriteLine($"A Cosmos Container named '{containerName}' already exists for the Cosmos Db: '{databaseName}'.");
                break;
            default:
                throw new InvalidOperationException($"Unable to create or locate a Cosmos Container named '{containerName}'. Status Code: {container.StatusCode}");
        }

        return container;
    }

    internal static async Task<Container> EnsureBeersContainerAsync(Database db, string databaseName, string containerName)
    {
        List<string> beersPartitionKeys = ["/BrewerId", "/EntityType"];
        var beerContainerProperties = new ContainerProperties(containerName, partitionKeyPaths: beersPartitionKeys);

        var beerContainer = await db.CreateContainerIfNotExistsAsync(beerContainerProperties, 400);
        switch (beerContainer.StatusCode)
        {
            case HttpStatusCode.Created:
                Console.WriteLine($"A new Cosmos Container named '{containerName}' was successfully created for the Cosmos Db: '{databaseName}'.");
                break;
            case HttpStatusCode.OK:
                Console.WriteLine($"A Cosmos Container named '{containerName}' already exists for the Cosmos Db: '{databaseName}'.");
                break;
            default:
                throw new InvalidOperationException($"Unable to create or locate a Cosmos Container named '{containerName}'. Status Code: {beerContainer.StatusCode}");
        }

        return beerContainer;
    }
}
