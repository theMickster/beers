# Beers Project

A capstone learning project: beer discovery platform built with .NET 10 API + Azure Cosmos DB + Angular 22.

## Repository Layout

```
beers/
├── api/                          # Main API solution (Beers.API.sln)
│   ├── src/
│   │   ├── Beers.API/            # ASP.NET Core Web API host (.NET 10)
│   │   ├── Beers.Application/    # Business logic, services, validators, data access
│   │   ├── Beers.Common/         # Shared constants, attributes, helpers, settings
│   │   └── Beers.Domain/         # Entities, view models, AutoMapper profiles
│   └── tests/
│       └── Beers.UnitTests/      # xUnit tests
├── tools/dataLoader/             # .NET 10 console app for Cosmos DB seeding
└── angular-app/                  # Angular 22 frontend (Nx workspace)
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
- **Secrets:** AKV-only for Cosmos and telemetry. Required AKV secrets: `AzureCosmosDbAccountUri`, `AzureCosmosDbDatabaseName`, `AzureCosmosDbSecurityKey`, `beers-aplication-insights-connection-string`
- **Local user-secrets:** `KeyVault:VaultUri`, `AutoMapperLicenseKey`
- **Docker profile** available (ports 8080/8081)

## Azure DevOps

- **Organization:** https://dev.azure.com/mletofsky
- **Project:** JustForFun
- **Epic:** #553 "2025 Beers"
- **Commit format:** Reference work items with `#<id>` in commit messages

## Dependency Security (Supply Chain)

- **7-day cool-off:** never add, upgrade or pin a package version (npm, NuGet, dotnet tools, GitHub Actions, container images) published less than 7 days ago. Look up the publish date in the registry first (`npm view <pkg> time --json`; the nuget.org registration `published` field). If the latest release is too new, propose the newest release that is at least 7 days old and say why.
- **Audit before changing dependencies:** run `npm audit` in `angular-app/` and `tests/api-read/`, and `dotnet list package --vulnerable --include-transitive`. Report the results.
- **Never run `npm audit fix --force`.** It proposes major downgrades. Upgrade deliberately.
- **No silent exceptions:** if a security fix exists only in a release younger than 7 days, stop and ask the human; do not bypass the cool-off.
- **Scope:** applies to new changes. Pins already in the repo on 2026-11-15 are grandfathered.
- Keep the Angular packages and the Nx packages in lockstep, commit lockfiles, install with `npm ci`.
- Enforced by `.npmrc` (`min-release-age=7`) and `.github/dependabot.yml` (`cooldown`). NuGet has no native cool-off, so for .NET this rule is manual.

## Do NOT

- Do not create new Cosmos DB containers without discussion -- extend existing containers with new `EntityType` discriminators
- Do not change the partition key strategy without discussion
- Do not add RxJS Subjects for state management in the Angular frontend -- use signals
- Do not add packages without verifying they exist and are necessary
- Do not use git branches nor worktrees unless the human CLEARLY states that is requested.
