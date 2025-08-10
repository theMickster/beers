using System.Net;
using Microsoft.Azure.Cosmos;

namespace BeersDataLoader.Infrastructure;

internal sealed class CosmosProvisioner
{
    private const string MetadataContainerName = "Metadata";
    private const string BeersContainerName = "Beers";

    internal async Task<Database> EnsureDatabaseAsync(CosmosClient client, string databaseName)
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

    internal async Task<Container> EnsureMetadataContainerAsync(Database db, string databaseName)
    {
        List<string> metadataPartitionKeys = ["/ApplicationName", "/TypeId"];
        var metadataContainerProperties = new ContainerProperties(MetadataContainerName, partitionKeyPaths: metadataPartitionKeys);

        var container = await db.CreateContainerIfNotExistsAsync(metadataContainerProperties, 400);
        switch (container.StatusCode)
        {
            case HttpStatusCode.Created:
                Console.WriteLine($"A new Cosmos Container named '{MetadataContainerName}' was successfully created for the Cosmos Db: '{databaseName}'.");
                break;
            case HttpStatusCode.OK:
                Console.WriteLine($"A Cosmos Container named '{MetadataContainerName}' already exists for the Cosmos Db: '{databaseName}'.");
                break;
            default:
                throw new InvalidOperationException($"Unable to create or locate a Cosmos Container named '{MetadataContainerName}'. Status Code: {container.StatusCode}");
        }

        return container;
    }

    internal async Task<Container> EnsureBeersContainerAsync(Database db, string databaseName)
    {
        List<string> beersPartitionKeys = ["/BrewerId", "/EntityType"];
        var beerContainerProperties = new ContainerProperties(BeersContainerName, partitionKeyPaths: beersPartitionKeys);

        var beerContainer = await db.CreateContainerIfNotExistsAsync(beerContainerProperties, 400);
        switch (beerContainer.StatusCode)
        {
            case HttpStatusCode.Created:
                Console.WriteLine($"A new Cosmos Container named '{BeersContainerName}' was successfully created for the Cosmos Db: '{databaseName}'.");
                break;
            case HttpStatusCode.OK:
                Console.WriteLine($"A Cosmos Container named '{BeersContainerName}' already exists for the Cosmos Db: '{databaseName}'.");
                break;
            default:
                throw new InvalidOperationException($"Unable to create or locate a Cosmos Container named '{BeersContainerName}'. Status Code: {beerContainer.StatusCode}");
        }

        return beerContainer;
    }
}
