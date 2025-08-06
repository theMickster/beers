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
