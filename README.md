# Beers

Beers is a side-project API focused on cataloging beer and brewery data while exploring Azure-hosted .NET patterns.

## Repository Structure

- `api/`: ASP.NET Core API solution and unit tests
- `api/src/Beers.API`: HTTP host, controllers, middleware, registration
- `api/src/Beers.Application`: services, validators, data access abstractions
- `api/src/Beers.Domain`: entities, models, AutoMapper profiles
- `api/src/Beers.Common`: shared constants/settings/helpers
- `api/tests/Beers.UnitTests`: xUnit test project
- `tools/dataLoader/`: console app for Cosmos metadata seeding

## Technology

- API: ASP.NET Core on .NET 10
- Data loader: .NET 10 console app
- Database: Azure Cosmos DB
- Secrets/config: Azure Key Vault (authoritative source)

## Local Setup Prerequisites

- .NET SDK 10.x
- Access to Azure Key Vault via `KeyVault:VaultUri`
- Valid Key Vault secrets for Cosmos and telemetry
- Local user-secrets key: `AutoMapperLicenseKey`

## Required Azure Key Vault Secrets

- `AzureCosmosDbAccountUri`
- `AzureCosmosDbDatabaseName`
- `AzureCosmosDbSecurityKey`
- `beers-aplication-insights-connection-string`

`Beers.API` reads Cosmos settings from AKV only. If keys are rotated, update AKV and restart the app.

## Run the API

```bash
cd api/src/Beers.API
dotnet restore
dotnet run
```

Swagger is available at `/swagger` when running locally.

## Seed Cosmos Data

The data loader expects these environment variables:

- `BeersCosmosEndpoint`
- `BeersCosmosKey`

Run:

```bash
cd tools/dataLoader
dotnet restore
dotnet run
```

The loader now also seeds rich Brewer Review demo data into `Beers` using
`tools/dataLoader/Data/BrewerReviews.json` (generated from `Brewers.json` with
deterministic shape and random review IDs).

## Cosmos Shared-Container Document Types

`Beers.Application` keeps beer-scoped operational documents in the shared
Cosmos `MainContainer` using the hierarchical partition key
`/BrewerId` + `/EntityType`.

Current discriminator-backed document types in that container include:

- `Beer`
- `Brewer`
- `BrewerReview`
- `NewsBlogPost`

`NewsBlogPost` documents store an embedded brewer author snapshot and continue
to use the existing shared-container partitioning pattern for Feature #517.

## Cosmos Bicep Deployments (Container-Only Safety)

Container IaC operations now live under `infra/bicep/cosmos/` and are **safety-constrained**.

- Deployment mode is **Incremental only**
- **No Cosmos account mutation**
- **No Cosmos database mutation**
- Scope is **container-only** for:
  - `Beers-Dev`
  - `Beers-QA`

Quick commands:

```bash
# Validate
infra/bicep/cosmos/scripts/validate.sh dev
infra/bicep/cosmos/scripts/validate.sh qa

# What-if preview
infra/bicep/cosmos/scripts/what-if.sh dev
infra/bicep/cosmos/scripts/what-if.sh qa

# Deploy (incremental)
infra/bicep/cosmos/scripts/deploy.sh dev
infra/bicep/cosmos/scripts/deploy.sh qa
```

See `infra/bicep/cosmos/README.md` for operator details and raw `az` examples.

## Read-Only Crawl Testing (Containerized)

This repository now includes a read-only crawl test stack that runs the API in Docker and validates read/search endpoints with:

- Playwright API tests for functional API read behavior
- k6 for smoke/load/stress read-only performance runs

The API read scope is intentionally read endpoints only in this phase (no create/update/delete workflow assertions).

### Test Assets

- Docker compose files: `.docker/compose/quality-gates.yml`
- Docker env template: `.docker/env/.env.api-read.example`
- Optional docker wrappers: `.docker/scripts/`
- Local test scripts: `tests/api-read/scripts/`
- Playwright tests: `tests/api-read/playwright/tests/api-read.spec.ts`
- k6 TypeScript source: `tests/api-read/k6/src/api-read.ts`
- k6 compiled runtime artifact: `tests/api-read/k6/dist/api-read.js`
- GitHub Actions workflow: `.github/workflows/quality-gates.yml`
- Azure DevOps pipeline: `.pipelines/quality-gates.yml`

### Required Environment Variables

These are required to start the containerized API (AKV-backed runtime):

- `KeyVault__VaultUri`
- `AutoMapperLicenseKey`
- `AZURE_TENANT_ID`
- `AZURE_CLIENT_ID`
- `AZURE_CLIENT_SECRET`

Optional:

- `API_PORT` (default `8080`)
- `BASE_URL` (default `http://localhost:8080`)
- `TEST_MODE` (`smoke`, `load`, `stress`; default `smoke`)
- `VUS`, `DURATION`, `THRESHOLD_P95`, `THRESHOLD_ERRORS`

See `.docker/env/.env.api-read.example` for a template.

### Local Run

```bash
cp .docker/env/.env.api-read.example .docker/env/.env.api-read
# fill in real secret values
chmod +x tests/api-read/scripts/*.sh
tests/api-read/scripts/run-api-read-suite.sh
```

Run only Playwright:

```bash
tests/api-read/scripts/start-api-container.sh
tests/api-read/scripts/run-playwright-api-read.sh
tests/api-read/scripts/stop-api-container.sh
```

Run only k6:

```bash
# script auto-installs test dependencies when missing
TEST_MODE=smoke tests/api-read/scripts/run-k6-api-read.sh
```

Optional local k6 checks:

```bash
cd tests/api-read
npm run api-read:k6:typecheck
npm run api-read:k6:build
```

## CI/CD API Read Test Entry Points

- GitHub Actions: `.github/workflows/quality-gates.yml`
  - Uses repository secrets:
    - `KEYVAULT_VAULTURI`
    - `AUTOMAPPER_LICENSE_KEY`
    - `AZURE_TENANT_ID`
    - `AZURE_CLIENT_ID`
    - `AZURE_CLIENT_SECRET`
  - Supports `workflow_dispatch` input `test_mode`.
  - Workflow writes a temporary compose env file and runs `.docker/compose/quality-gates.yml`.

- Azure DevOps: `.pipelines/quality-gates.yml`
  - Expects secure variables:
    - `KeyVault__VaultUri`
    - `AutoMapperLicenseKey`
    - `AZURE_TENANT_ID`
    - `AZURE_CLIENT_ID`
    - `AZURE_CLIENT_SECRET`
  - Uses `TEST_MODE` variable to switch k6 mode without code changes.
  - Pipeline writes a temporary compose env file and runs `.docker/compose/quality-gates.yml`.

## API Surface (v1)

- `GET /api/v1/beers`
- `GET /api/v1/beers/{beerId}`
- `POST /api/v1/beers`
- `PUT /api/v1/beers/{beerId}`
- `DELETE /api/v1/beers/{beerId}`
- `POST /api/v1/beers/search`
- `GET /api/v1/brewers`
- `GET /api/v1/brewers/{brewerId}`
- `POST /api/v1/brewers`
- `PUT /api/v1/brewers/{brewerId}`
- `PATCH /api/v1/brewers/{brewerId}`
- `DELETE /api/v1/brewers/{brewerId}`
- `POST /api/v1/brewers/{brewerId}/reviews`
- `GET /api/v1/brewers/{brewerId}/reviews`
- `GET /api/v1/brewers/{brewerId}/reviews/{reviewId}`
- `DELETE /api/v1/brewers/{brewerId}/reviews/{reviewId}`
- `POST /api/v1/brewers/{brewerId}/reviews/search`
- `GET /api/v1/beerTypes`
- `GET /api/v1/beerStyles`
- `GET /api/v1/beerCategories`
- `GET /api/v1/breweryTypes`

## Contributing

See `CONTRIBUTING.md` for setup conventions, AKV secret hygiene, and troubleshooting guidelines.

## Questions / Comments

Please open a GitHub Issue.

## macOS Local Debugging Checklist (Cosmos + API)

Use this checklist when local API requests fail with port bind errors, Cosmos `401`, or Cosmos `503/20001`.

### 1) Check for port conflicts first

`Beers.API` defaults to `44379` (HTTPS) and `44378` (HTTP) locally.

```bash
lsof -nP -iTCP:44379 -sTCP:LISTEN
lsof -nP -iTCP:44378 -sTCP:LISTEN
```

If another process is listening, stop it:

```bash
kill <PID>
```

Or run on temporary ports:

```bash
ASPNETCORE_URLS="https://localhost:54479;http://localhost:54478" dotnet run
```

### 2) Verify AKV + local runtime context

- Ensure `az login` is active for your local identity.
- Ensure your identity has access to Key Vault secrets.
- Remember: `Beers.API` reads Cosmos values from AKV only.

### 3) Check macOS firewall state

```bash
sudo /usr/libexec/ApplicationFirewall/socketfilterfw --getglobalstate
sudo /usr/libexec/ApplicationFirewall/socketfilterfw --getblockall
```

Temporary toggle (for quick testing only):

```bash
sudo /usr/libexec/ApplicationFirewall/socketfilterfw --setglobalstate off
# test
sudo /usr/libexec/ApplicationFirewall/socketfilterfw --setglobalstate on
```

### 4) Check packet filter (`pf`)

```bash
sudo pfctl -s info
sudo pfctl -sr | head -100
```

Temporary test:

```bash
sudo pfctl -d
# retry API request
sudo pfctl -e
```

### 5) Check proxy/PAC settings

```bash
scutil --proxy
```

If PAC/proxy is enabled, temporarily disable auto-proxy for your active interface (example: Wi-Fi):

```bash
networksetup -listallnetworkservices
networksetup -setautoproxystate "Wi-Fi" off
# retry API request
networksetup -setautoproxystate "Wi-Fi" on
```

### 6) Interpret common Cosmos errors quickly

- `401 Unauthorized`: wrong/old Cosmos key in AKV, wrong account URI, or stale process after secret rotation.
- `503 ServiceUnavailable` with substatus `20001`: client cannot establish network path to Cosmos endpoints (often direct-mode networking/proxy/filter issue).

### 7) Fast local reset loop

```bash
cd api/src/Beers.API
dotnet clean
dotnet restore
dotnet user-secrets list
dotnet run
```
