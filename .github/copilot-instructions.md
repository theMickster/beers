# Beers Copilot Instructions (Epic 553 / 2025 Beers)

## Project Intent

This project is a capstone learning platform that demonstrates modern Azure Cosmos DB patterns and Angular 21 features through beer discovery scenarios.

## Scope

- Applies to `api/`, `BeersDataLoader/`, and planned frontend code.
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

## Feature-Specific Guidance (Epic 553)

- Feature `#516` Brewer Reviews:
  - Store reviews with existing discriminator and partition conventions.
  - Recalculate aggregate ratings via change feed-triggered workflows.
  - Expose rating summary fields in read models without breaking current contracts.
- Feature `#517` News/Blog Posts:
  - Support multiple post shapes (text, gallery, event announcement) with clear schema/version markers.
  - Add filtering by brewer, tag, and published date.
  - Keep contracts forward-compatible for future post variants.
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

## Angular 21 Frontend Guidance

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

## Documentation Sync

When endpoint surface, setup requirements, Cosmos data patterns, or frontend capabilities change, update `README.md` in the same PR.
