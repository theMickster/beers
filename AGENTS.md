# Agent Instructions

Project conventions, architecture and commands live in [`.claude/CLAUDE.md`](.claude/CLAUDE.md) and [`.github/copilot-instructions.md`](.github/copilot-instructions.md). Read both before changing code.

## Dependency Security (Supply Chain)

- **7-day cool-off:** never add, upgrade or pin a package version (npm, NuGet, dotnet tools, GitHub Actions, container images) published less than 7 days ago. Look up the publish date in the registry first (`npm view <pkg> time --json`; the nuget.org registration `published` field). If the latest release is too new, propose the newest release that is at least 7 days old and say why.
- **Audit before changing dependencies:** run `npm audit` in `angular-app/` and `tests/api-read/`, and `dotnet list package --vulnerable --include-transitive`. Report the results.
- **Never run `npm audit fix --force`.** It proposes major downgrades. Upgrade deliberately.
- **No silent exceptions:** if a security fix exists only in a release younger than 7 days, stop and ask the human; do not bypass the cool-off.
- **Scope:** applies to new changes. Pins already in the repo on 2026-11-15 are grandfathered.
- Keep the Angular packages and the Nx packages in lockstep, commit lockfiles, install with `npm ci`.
- Enforced by `.npmrc` (`min-release-age=7`) and `.github/dependabot.yml` (`cooldown`). NuGet has no native cool-off, so for .NET this rule is manual.
