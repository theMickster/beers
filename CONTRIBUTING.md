# Contributing

Thanks for contributing to Beers.

## Local development setup

1. Install .NET SDK 10.x.
2. Configure `Beers.API` user-secrets with Key Vault URI and AutoMapper license.
3. Ensure required secrets exist and are current in AKV.
4. Run the API and verify `/swagger` loads.

## Required local user-secrets keys

Set secrets on the API project:

```bash
cd api/src/Beers.API
dotnet user-secrets set "KeyVault:VaultUri" "https://<your-vault>.vault.azure.net/"
dotnet user-secrets set "AutoMapperLicenseKey" "<automapper-license-key>"
```

## Required Azure Key Vault secrets

- `AzureCosmosDbAccountUri`
- `AzureCosmosDbDatabaseName`
- `AzureCosmosDbSecurityKey`
- `beers-aplication-insights-connection-string`

## Cosmos key rotation checklist

When rotating Cosmos keys:

1. Update AKV secret `AzureCosmosDbSecurityKey`.
2. Restart the API process.
3. Validate with a metadata endpoint (for example `/api/v1/beerTypes`).

## Remove stale local keys

To avoid confusion, remove obsolete local keys from user-secrets:

```bash
cd api/src/Beers.API
dotnet user-secrets remove "AzureCosmosDb:Account"
dotnet user-secrets remove "AzureCosmosDb:DatabaseName"
dotnet user-secrets remove "AzureCosmosDb:SecurityKey"
dotnet user-secrets remove "KeyVault:TenantId"
dotnet user-secrets remove "KeyVault:ClientId"
dotnet user-secrets remove "KeyVault:ClientSecret"
dotnet user-secrets remove "ApplicationInsights:InstrumentationKey"
dotnet user-secrets remove "ApplicationInsights:ConnectionString"
```

## Troubleshooting 401 Unauthorized from Cosmos

If you see Cosmos `401 Unauthorized` with token/signature errors:

- Confirm `AzureCosmosDbSecurityKey` in AKV matches the active Cosmos key.
- Confirm `AzureCosmosDbAccountUri` and `AzureCosmosDbDatabaseName` in AKV are correct.
- Confirm the API process was restarted after AKV secret updates.
