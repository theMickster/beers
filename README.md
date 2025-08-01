# Beers

Beers is a side-project API focused on cataloging beer and brewery data while exploring Azure-hosted .NET patterns.

## Repository Structure

- `api/`: ASP.NET Core API solution and unit tests
- `api/src/Beers.API`: HTTP host, controllers, middleware, registration
- `api/src/Beers.Application`: services, validators, data access abstractions
- `api/src/Beers.Domain`: entities, models, AutoMapper profiles
- `api/src/Beers.Common`: shared constants/settings/helpers
- `api/tests/Beers.UnitTests`: xUnit test project
- `BeersDataLoader/`: console app for Cosmos metadata seeding

## Technology

- API: ASP.NET Core on .NET 9
- Data loader: .NET 8 console app
- Database: Azure Cosmos DB
- Secrets/config: Azure Key Vault (for API runtime secrets)

## Local Setup Prerequisites

- .NET SDK 9.x (API)
- .NET SDK 8.x (BeersDataLoader)
- Access to Key Vault configured by `KeyVault:VaultUri`
- Cosmos DB secrets available in Key Vault:
  - `AzureCosmosDbAccountUri`
  - `AzureCosmosDbSecurityKey`
  - `AzureCosmosDbDatabaseName`
- Optional telemetry secret:
  - `beers-aplication-insights-connection-string`

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
cd BeersDataLoader
dotnet restore
dotnet run
```

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
- `GET /api/v1/beerTypes`
- `GET /api/v1/beerStyles`
- `GET /api/v1/beerCategories`
- `GET /api/v1/breweryTypes`

## Questions / Comments

Please open a GitHub Issue.
