namespace BeersDataLoader.Models;

internal sealed record CosmosSettings(
    string AccountEndpoint,
    string DatabaseName,
    string BeersContainerName,
    string MetadataContainerName
);
