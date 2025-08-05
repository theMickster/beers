---
description: Review all markdown files for stale or missing content and suggest updates.
allowed-tools: Read, Edit, Glob, Grep, Bash(az boards:*)
---

Scan every markdown file in this repository and cross-reference its content against the current codebase state. Report what is stale, incorrect, or missing.

## Steps

1. **Discover markdown files.** Glob for `**/*.md` in the repo root (exclude `node_modules/`, `.notes/`).

2. **Gather current state.** Read these sources of truth in parallel:
   - All `*.csproj` files — extract `TargetFramework` and package versions.
   - All `*.sln` files — extract Visual Studio version.
   - All controller files (`*Controller.cs`) — extract route attributes for the API surface.
   - Directory listing of top-level folders — confirm project paths.

3. **Read each markdown file** and check for:
   - **.NET version references** that don't match csproj `TargetFramework` values.
   - **Project paths** that don't match the actual directory structure (e.g., old `tools/dataLoader/` path references should be current).
   - **API endpoint lists** that are missing routes found in controllers, or list routes that no longer exist.
   - **SDK version requirements** that are outdated.
   - **Visual Studio version references** that don't match sln files.
   - **Feature/epic references** that are stale or incomplete.

4. **Report findings** as a markdown table:

| File | Line | Issue | Current Value | Suggested Fix |
|------|------|-------|---------------|---------------|
| ... | ... | ... | ... | ... |

If no issues are found, say so.

5. **Offer to apply fixes.** If issues were found, ask the user whether to apply the suggested corrections.
