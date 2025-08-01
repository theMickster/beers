# Beers Project

A capstone learning project: beer discovery platform built with .NET 9 API + Azure Cosmos DB + Angular 21.

## Repository Layout

```
beers/
├── api/                          # Main API solution (Beers.API.sln)
│   ├── src/
│   │   ├── Beers.API/            # ASP.NET Core Web API host (.NET 9)
│   │   ├── Beers.Application/    # Business logic, services, validators, data access
│   │   ├── Beers.Common/         # Shared constants, attributes, helpers, settings
│   │   └── Beers.Domain/         # Entities, view models, AutoMapper profiles
│   └── tests/
│       └── Beers.UnitTests/      # xUnit tests
├── BeersDataLoader/              # Standalone .NET 8 console app for Cosmos DB seeding
└── web/                          # (Planned) Angular 21 frontend
```

## Architecture

**Layered:** API → Application → Domain/Common. No shortcuts -- controllers call services, services use EF Core contexts.

**Cosmos DB containers:**
- `Beers` container: Beer, Brewer entities. Hierarchical partition key: `/BrewerId` + `/EntityType`.
- `Metadata` container: BeerType, BeerStyle, BeerCategory, BreweryType. Hierarchical partition key: `/ApplicationName` + `/TypeId`. Discriminator-based TPH.

**Key patterns:**
- Owned/embedded types via EF Core `OwnsOne`/`OwnsMany` for pricing, ratings, slim references
- Slim entities for denormalized references (BrewerSlimEntity, BeerTypeSlimEntity, etc.)
- Reflection-based DI registration via `[ServiceLifetimeScoped]`, `[ServiceLifetimeTransient]`, `[ServiceLifetimeSingleton]` attributes
- Metadata cached via `IMemoryCache` (300s TTL)
- FluentValidation for request validation
- AutoMapper profiles in `Beers.Domain.Profiles`

## Conventions

- **Naming:** PascalCase everywhere. Entity classes suffixed with `Entity`. Service interfaces prefixed with `I`.
- **API versioning:** All endpoints under `api/v1/`. Uses `Asp.Versioning.Mvc`.
- **Controllers** return typed results; use `[ProducesResponseType]` attributes.
- **Services** are split by operation: `ReadBeerService`, `CreateBeerService`, `UpdateBeerService`, `DeleteBeerService`.
- **New entity types** in the Beers container use the `EntityType` discriminator (e.g., "Beer", "Brewer"). Extend `BaseBeerEntity`.
- **New metadata types** extend `BaseMetaDataEntity` with a unique `TypeId` constant.
- **JSON:** Newtonsoft.Json (`JsonProperty` attributes on entities).
- **Testing:** xUnit. Test classes organized by layer (Controllers/, Services/, Validators/).
- **Nullable reference types** enabled. Implicit usings enabled.

## Local Development

- **API URL:** https://localhost:44379 (HTTP: 44378)
- **Swagger:** Available at `/swagger` in Development environment
- **Secrets:** Azure Key Vault integration. Required secrets: `AzureCosmosDbAccountUri`, `AzureCosmosDbDatabaseName`, `AzureCosmosDbSecurityKey`
- **Docker profile** available (ports 8080/8081)

## Azure DevOps

- **Organization:** https://dev.azure.com/mletofsky
- **Project:** JustForFun
- **Epic:** #553 "2025 Beers"
- **Commit format:** Reference work items with `#<id>` in commit messages

## Do NOT

- Create new Cosmos DB containers without discussion -- extend existing containers with new `EntityType` discriminators
- Change the partition key strategy without discussion
- Add RxJS Subjects for state management in the Angular frontend -- use signals
- Add packages without verifying they exist and are necessary
