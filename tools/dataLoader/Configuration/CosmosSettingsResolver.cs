using BeersDataLoader.Models;

namespace BeersDataLoader.Configuration;

internal static class CosmosSettingsResolver
{
    internal static CosmosSettings Resolve()
    {
        var accountEndpoint = Environment.GetEnvironmentVariable(LoaderConfigurationKeys.EndpointEnvironmentVariableName);
        var databaseName = Environment.GetEnvironmentVariable(LoaderConfigurationKeys.DatabaseEnvironmentVariableName);
        var beersContainerName = Environment.GetEnvironmentVariable(LoaderConfigurationKeys.BeersContainerEnvironmentVariableName);
        var metadataContainerName = Environment.GetEnvironmentVariable(LoaderConfigurationKeys.MetadataContainerEnvironmentVariableName);

        var missingSettings = new List<string>();
        if (string.IsNullOrWhiteSpace(accountEndpoint))
        {
            missingSettings.Add(LoaderConfigurationKeys.EndpointEnvironmentVariableName);
        }

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            missingSettings.Add(LoaderConfigurationKeys.DatabaseEnvironmentVariableName);
        }

        if (string.IsNullOrWhiteSpace(beersContainerName))
        {
            missingSettings.Add(LoaderConfigurationKeys.BeersContainerEnvironmentVariableName);
        }

        if (string.IsNullOrWhiteSpace(metadataContainerName))
        {
            missingSettings.Add(LoaderConfigurationKeys.MetadataContainerEnvironmentVariableName);
        }

        if (missingSettings.Count > 0)
        {
            throw new InvalidOperationException(
                $"Missing required Cosmos loader environment variables: {string.Join(", ", missingSettings)}.");
        }

        return new CosmosSettings(accountEndpoint!, databaseName!, beersContainerName!, metadataContainerName!);
    }
}
