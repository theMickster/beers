# Beers Copilot Instructions (Epic 553 / 2025 Beers)

## Project Intent

This project is a capstone learning platform that demonstrates modern Azure Cosmos DB patterns and Angular 22 features through beer discovery scenarios.

## Scope

- Applies to `api/`, `tools/dataLoader/`, and `angular-app/`.
- Prefer focused diffs that preserve existing layering and naming conventions.
- Implement requested behavior end-to-end (API/contracts/tests/docs), not partial stubs.

## Layering and Architecture Rules

- Controllers call application services only; avoid direct context access in controllers.
- Application services own orchestration, validation, hydration, and data access coordination.
- Domain stays focused on entities/models/profiles and shared business semantics.
- Keep new feature areas isolated by folder and naming (for example, `Reviews`, `NewsBlogPosts`, `Flights`, `Discovery`).

## Dependency Injection Conventions

- Use existing lifetime attributes:
  - `ServiceLifetimeScoped`
  - `ServiceLifetimeTransient`
  - `ServiceLifetimeSingleton`
- Keep `I{ClassName}` interface naming so reflection-based registration continues to work.
- Register any new cross-cutting services (query helpers, patch services, ETag helpers) through existing patterns.

## Cosmos DB Implementation Guardrails

- Preserve and extend hierarchical partition key strategy already in use (`BrewerId/EntityType`, `ApplicationName/TypeId`).
- Support flexible schemas in shared containers using explicit type discriminators.
- Prefer point reads where possible; use cross-partition queries only for discovery/search features.
- Use patch operations for partial updates when full-document replacement is unnecessary.
- Implement optimistic concurrency with ETags on mutable user-generated content.
- For aggregate updates (such as rolling ratings), use change feed-driven processing patterns.
- Keep query projections lean and paginated; avoid loading full documents when not required.
- Cosmos configuration is **AKV-only** for `Beers.API`: use Key Vault secrets `AzureCosmosDbAccountUri`, `AzureCosmosDbDatabaseName`, and `AzureCosmosDbSecurityKey`.
- Do not add or reintroduce local Cosmos fallback config keys (for example `AzureCosmosDb:*`) in runtime code paths.
- If Cosmos keys rotate, update AKV and restart app instances; do not solve rotation issues by adding alternate config paths.

## Feature-Specific Guidance (Epic 553)

- Feature `#516` Brewer Reviews:
  - Store reviews with existing discriminator and partition conventions.
  - Recalculate aggregate ratings via change feed-triggered workflows.
  - Expose rating summary fields in read models without breaking current contracts.
- Feature `#517` News/Blog Posts:
  - Support multiple post shapes (text, gallery, event announcement) with clear schema/version markers.
  - Add filtering by brewer, tag, and published date.
  - Keep contracts forward-compatible for future post variants.
  - Seeding infrastructure is implemented: `NewsBlogPostSeeder` + `Data/NewsBlogPosts.json` (14 entries). Founders and Sierra Nevada have all three post types seeded (TextPost, ImageGallery, EventAnnouncement); remaining 8 brewers have one TextPost each. Run `dotnet run` from `tools/dataLoader/` to populate.
- Beer Flight Builder:
  - Model as lightweight user state with efficient read/write paths.
  - Optimize for cross-partition read patterns used in curation flows.
- Faceted Search and Discovery:
  - Support filters for style, category, brewer, and rating range.
  - Build query composition for optional facets without duplicate code.
- "What Should I Drink?" Recommender:
  - Keep logic deterministic and query-driven (no ML dependencies).
  - Base recommendations on highly rated styles/categories.
- Seasonal and Limited Releases:
  - Prefer TTL or date-range-based filtering conventions.
  - Ensure availability windows are easy to query and test.

## API and Contract Conventions

- Keep routes under `/api/v1/...` with version attributes.
- Preserve response style consistency with existing controllers and models.
- Include pagination/query contracts for list/discovery endpoints.
- Document discriminator, partition, and concurrency fields where relevant.

## Validation and Error Handling

- Use FluentValidation under `api/src/Beers.Application/Validators`.
- Reuse shared constants/messages where possible.
- Return explicit validation/concurrency errors; do not silently swallow failures.
- For ETag conflicts, return clear conflict responses with actionable messages.

## Angular 22 Frontend Guidance

- Prefer standalone components and modern control flow (`@if`, `@for`, `@defer`).
- Use signals and signal-based forms for local UI state.
- Support SSR with hydration-friendly data-loading patterns.
- Keep UX responsive/mobile-first for discovery, compare, and dashboard views.
- Frontend features should include:
  - interactive faceted explorer,
  - side-by-side beer comparison,
  - dashboard visualizations (top rated, trending, style breakdown).

## Testing Expectations

- For behavior changes, add/update tests in `api/tests/Beers.UnitTests`.
- Use xUnit + FluentAssertions + Moq for API/application changes.
- Add focused tests for:
  - partition key correctness,
  - cross-partition query behavior,
  - ETag concurrency handling,
  - patch update behavior,
  - change feed aggregation logic.

## Dependency Security (Supply Chain)

- **7-day cool-off:** never add, upgrade or pin a package version (npm, NuGet, dotnet tools, GitHub Actions, container images) published less than 7 days ago. Look up the publish date in the registry first (`npm view <pkg> time --json`; the nuget.org registration `published` field). If the latest release is too new, propose the newest release that is at least 7 days old and say why.
- **Audit before changing dependencies:** run `npm audit` in `angular-app/` and `tests/api-read/`, and `dotnet list package --vulnerable --include-transitive`. Report the results.
- **Never run `npm audit fix --force`.** It proposes major downgrades. Upgrade deliberately.
- **No silent exceptions:** if a security fix exists only in a release younger than 7 days, stop and ask the human; do not bypass the cool-off.
- **Scope:** applies to new changes. Pins already in the repo on 2026-11-15 are grandfathered.
- Keep the Angular packages and the Nx packages in lockstep, commit lockfiles, install with `npm ci`.
- Enforced by `.npmrc` (`min-release-age=7`) and `.github/dependabot.yml` (`cooldown`). NuGet has no native cool-off, so for .NET this rule is manual.

## Documentation Sync

When endpoint surface, setup requirements, Cosmos data patterns, or frontend capabilities change, update `README.md` in the same PR.
