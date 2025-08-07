# Cosmos Container-Only Bicep Operations

This folder is for **container-level Cosmos IaC only**.

## Safety guardrails

- ✅ Deployment mode: **Incremental only**
- ✅ Scope: **container-only**
- ✅ Allowed environments: **Beers-Dev** and **Beers-QA**
- ❌ No Cosmos **account** mutation
- ❌ No Cosmos **database** mutation

The scripts in `infra/bicep/cosmos/scripts/` enforce these rules before running `az deployment group`.

## Prerequisites

- Azure CLI logged in (`az login`)
- Correct subscription selected
- Existing resource group/account/database already provisioned
- Template file present at `infra/bicep/cosmos/main.bicep` (or override via env var)

## Scripted commands (recommended)

```bash
# Validate template + guardrails
infra/bicep/cosmos/scripts/validate.sh dev
infra/bicep/cosmos/scripts/validate.sh qa

# Preview changes (what-if)
infra/bicep/cosmos/scripts/what-if.sh dev
infra/bicep/cosmos/scripts/what-if.sh qa

# Deploy changes (incremental mode only)
infra/bicep/cosmos/scripts/deploy.sh dev
infra/bicep/cosmos/scripts/deploy.sh qa
```

## Environment variables and resolution rules

Always required:

```bash
export AZURE_SUBSCRIPTION_ID="<subscription-id>"
export COSMOS_RESOURCE_GROUP_DEV="Mick-West-US-3-CosmosDb"
export COSMOS_RESOURCE_GROUP_QA="Mick-West-US-3-CosmosDb"
```

For the current Beers environment, both `dev` and `qa` map to `Mick-West-US-3-CosmosDb` because the `Beers-Dev` and `Beers-QA` containers live in the same resource group.

Parameter file selection precedence (per environment):

1. `COSMOS_PARAMS_DEV` / `COSMOS_PARAMS_QA`
2. `COSMOS_PARAMS_FILE`
3. Default file if present: `infra/bicep/cosmos/parameters/current-env.json`
4. If no parameter file resolves, scripts fall back to inline parameters from env vars

Parameter file options:

```bash
export COSMOS_BICEP_TEMPLATE="infra/bicep/cosmos/main.bicep"
export COSMOS_PARAMS_DEV="infra/bicep/cosmos/parameters/current-env.json"      # or .bicepparam
export COSMOS_PARAMS_QA="infra/bicep/cosmos/parameters/current-env.json"       # or .bicepparam
export COSMOS_PARAMS_FILE="infra/bicep/cosmos/parameters/current-env.json"     # shared fallback
```

Parameter-file guardrails:
- `.json` and `.bicepparam` are supported.
- `.json` files must include `cosmosAccountName`, `cosmosDatabaseName`, and `containers` (and no unknown template params).
- `.bicepparam` files must `using` the same template resolved by `COSMOS_BICEP_TEMPLATE`.

If no parameter file is used, these are required:

```bash
export COSMOS_ACCOUNT_NAME_DEV="<existing-account-name-dev>" # or COSMOS_ACCOUNT_NAME
export COSMOS_ACCOUNT_NAME_QA="<existing-account-name-qa>"   # or COSMOS_ACCOUNT_NAME
export COSMOS_DATABASE_NAME="<existing-sql-database-name>"
export COSMOS_CONTAINERS_JSON='[{"id":"Beers-Dev"},{"id":"Beers-QA"}]'
```

## Subscription safety check

Scripts require `az login` and fail if the active Azure CLI subscription does not match `AZURE_SUBSCRIPTION_ID`.

## Raw Azure CLI examples (no wrappers)

Use these if you need direct `az` commands without wrappers.

### Validate

```bash
az deployment group validate \
  --subscription "$AZURE_SUBSCRIPTION_ID" \
  --resource-group "Mick-West-US-3-CosmosDb" \
  --template-file infra/bicep/cosmos/main.bicep \
  --parameters @infra/bicep/cosmos/parameters/current-env.json
```

### What-if

```bash
az deployment group what-if \
  --subscription "$AZURE_SUBSCRIPTION_ID" \
  --resource-group "Mick-West-US-3-CosmosDb" \
  --template-file infra/bicep/cosmos/main.bicep \
  --parameters @infra/bicep/cosmos/parameters/current-env.json \
  --mode Incremental
```

### Deploy

```bash
az deployment group create \
  --subscription "$AZURE_SUBSCRIPTION_ID" \
  --resource-group "Mick-West-US-3-CosmosDb" \
  --template-file infra/bicep/cosmos/main.bicep \
  --parameters @infra/bicep/cosmos/parameters/current-env.json \
  --mode Incremental
```
