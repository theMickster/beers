namespace BeersDataLoader.Models;

internal sealed record CosmosSettings(
    string AccountEndpoint,
    string SecurityKey,
    string DatabaseName
);
