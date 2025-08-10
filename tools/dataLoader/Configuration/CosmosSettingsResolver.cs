using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BeersDataLoader.Models;
using Microsoft.Extensions.Configuration;

namespace BeersDataLoader.Configuration;

internal static class CosmosSettingsResolver
{
    internal static CosmosSettings Resolve(IConfiguration configuration, string fallbackDatabaseName)
    {
        var accountEndpoint = Environment.GetEnvironmentVariable(LoaderConfigurationKeys.EndpointEnvironmentVariableName);
        var securityKey = Environment.GetEnvironmentVariable(LoaderConfigurationKeys.KeyEnvironmentVariableName);
        var databaseName = Environment.GetEnvironmentVariable(LoaderConfigurationKeys.DatabaseEnvironmentVariableName);
        var keyVaultUri = configuration[LoaderConfigurationKeys.KeyVaultUriConfigurationKey];

        if ((!string.IsNullOrWhiteSpace(keyVaultUri)) &&
            (string.IsNullOrWhiteSpace(accountEndpoint) || string.IsNullOrWhiteSpace(securityKey) || string.IsNullOrWhiteSpace(databaseName)))
        {
            var secretClient = BuildSecretClient(configuration, keyVaultUri);
            accountEndpoint ??= GetRequiredSecret(secretClient, LoaderConfigurationKeys.CosmosAccountUriSecretName);
            securityKey ??= GetRequiredSecret(secretClient, LoaderConfigurationKeys.CosmosSecurityKeySecretName);
            databaseName ??= GetRequiredSecret(secretClient, LoaderConfigurationKeys.CosmosDatabaseNameSecretName);
        }

        databaseName ??= fallbackDatabaseName;

        var missingSettings = new List<string>();
        if (string.IsNullOrWhiteSpace(accountEndpoint))
        {
            missingSettings.Add(
                $"{LoaderConfigurationKeys.EndpointEnvironmentVariableName} or AKV secret {LoaderConfigurationKeys.CosmosAccountUriSecretName}");
        }

        if (string.IsNullOrWhiteSpace(securityKey))
        {
            missingSettings.Add(
                $"{LoaderConfigurationKeys.KeyEnvironmentVariableName} or AKV secret {LoaderConfigurationKeys.CosmosSecurityKeySecretName}");
        }

        if (missingSettings.Count > 0)
        {
            throw new InvalidOperationException(
                $"Missing required Cosmos loader settings: {string.Join(", ", missingSettings)}. " +
                $"Set environment variables directly, or configure {LoaderConfigurationKeys.KeyVaultUriConfigurationKey} and required secrets in Azure Key Vault.");
        }

        return new CosmosSettings(accountEndpoint!, securityKey!, databaseName);
    }

    private static SecretClient BuildSecretClient(IConfiguration configuration, string keyVaultUri)
    {
        var retryDelay = 100d;
        var maxRetryDelay = 3000d;
        var maxRetryCount = 5;

        if (double.TryParse(configuration[LoaderConfigurationKeys.KeyVaultRetryDelayConfigurationKey], out var retryDelayValue))
        {
            retryDelay = retryDelayValue;
        }

        if (double.TryParse(configuration[LoaderConfigurationKeys.KeyVaultMaxRetryDelayConfigurationKey], out var maxRetryDelayValue))
        {
            maxRetryDelay = maxRetryDelayValue;
        }

        if (int.TryParse(configuration[LoaderConfigurationKeys.KeyVaultMaxRetryAttemptsConfigurationKey], out var maxRetryCountValue))
        {
            maxRetryCount = maxRetryCountValue;
        }

        var secretOptions = new SecretClientOptions
        {
            Retry =
            {
                Mode = RetryMode.Exponential,
                Delay = TimeSpan.FromMilliseconds(retryDelay),
                MaxDelay = TimeSpan.FromMilliseconds(maxRetryDelay),
                MaxRetries = maxRetryCount
            }
        };

        return new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential(), secretOptions);
    }

    private static string GetRequiredSecret(SecretClient secretClient, string secretName)
    {
        try
        {
            var result = secretClient.GetSecret(secretName);
            var value = result?.Value?.Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            throw new InvalidOperationException($"Azure Key Vault secret '{secretName}' is empty.");
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve Azure Key Vault secret '{secretName}'. Status: {ex.Status}, ErrorCode: {ex.ErrorCode}.",
                ex);
        }
    }
}
