namespace BeersDataLoader.Configuration;

internal static class LoaderConfigurationKeys
{
    public const string DefaultDatabaseName = "PlatformDatabases";

    public const string EndpointEnvironmentVariableName = "BeersCosmosEndpoint";

    public const string KeyEnvironmentVariableName = "BeersCosmosKey";

    public const string DatabaseEnvironmentVariableName = "BeersCosmosDatabaseName";

    public const string KeyVaultUriConfigurationKey = "KeyVault:VaultUri";

    public const string KeyVaultRetryDelayConfigurationKey = "KeyVault:RetryDelayMilliseconds";

    public const string KeyVaultMaxRetryDelayConfigurationKey = "KeyVault:MaxRetryDelayMilliseconds";

    public const string KeyVaultMaxRetryAttemptsConfigurationKey = "KeyVault:MaxRetryAttempts";

    public const string CosmosAccountUriSecretName = "AzureCosmosDbAccountUri";

    public const string CosmosSecurityKeySecretName = "AzureCosmosDbSecurityKey";

    public const string CosmosDatabaseNameSecretName = "AzureCosmosDbDatabaseName";
}
