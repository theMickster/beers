# Data Loader

This console app seeds Cosmos DB demo data for the Beers project.

## What It Does

- Ensures a Cosmos database exists.
- Ensures the configured `Metadata` and `Beers` containers exist.
- Seeds lookup metadata from the JSON files under `Data/`.

The loader is idempotent for existing items: it checks for each document first and skips records that already exist.

## Authentication Model

The loader uses `DefaultAzureCredential`.

That means:

- Run `az login --use-device-code` outside the loader.
- Make sure the signed-in identity has permission to create databases and containers and to write items in the target Cosmos account.
- The loader does not call `az` for you.
- The loader does not use Key Vault.

## Required Environment Variables

Set these before running the loader:

- `COSMOS_ENDPOINT_BEERS_LOADER`
- `COSMOS_DATABASE_BEERS_LOADER`
- `COSMOS_CONTAINER_BEERS_LOADER`
- `COSMOS_CONTAINER_METADATA_LOADER`

Example:

```bash
export COSMOS_ENDPOINT_BEERS_LOADER="https://<account-name>.documents.azure.com:443/"
export COSMOS_DATABASE_BEERS_LOADER="Beers-Dev"
export COSMOS_CONTAINER_BEERS_LOADER="Beers"
export COSMOS_CONTAINER_METADATA_LOADER="Metadata"
```

## Step-By-Step Setup

1. Sign in with Azure CLI.

```bash
az login
```

2. Confirm you are on the intended subscription.

```bash
az account show --query "{name:name,id:id}" -o table
```

3. Verify the Cosmos account endpoint from Azure CLI.

```bash
az cosmosdb show \
  --resource-group <resource-group-name> \
  --name <cosmos-account-name> \
  --query documentEndpoint -o tsv
```

4. Export the four loader environment variables.

```bash
export COSMOS_ENDPOINT_BEERS_LOADER="https://<account-name>.documents.azure.com:443/"
export COSMOS_DATABASE_BEERS_LOADER="<non-prod-database-name>"
export COSMOS_CONTAINER_BEERS_LOADER="<beer-container-name>"
export COSMOS_CONTAINER_METADATA_LOADER="<metadata-container-name>"
```

5. Run the loader.

```bash
cd tools/dataLoader
dotnet restore
dotnet run
```

6. When prompted, press `Y` to continue.

## Safety Notes

- Double-check the endpoint before running.
- Use a non-production database and container names for local testing.
- The loader creates missing databases and containers if they do not already exist.
- The loader performs real writes. There is no dry-run mode.

## Troubleshooting

- If the loader exits immediately, verify every required environment variable is set.
- If authorization fails, confirm `az login` is current and the signed-in identity has Cosmos data-plane access to the target account.
- If the database or container names are wrong, the loader will create or use those exact names in the target account.
