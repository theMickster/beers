# Spike #834 — Color Scheme & App Icons Decision

## Context

Feature #833 ("Beers Angular 22 Frontend") kicks off with Spike #834: establish the
visual identity (color palette, logo/favicon, icon set, design tokens) before any
frontend component work starts. The `beers` repo has no `web/` app yet — this is a
greenfield decision, not a refactor.

This follows the same theming patterns already proven in the AdventureWorks Angular
app (`apps/angular-web`): Tailwind v4 CSS-first config + DaisyUI v5, a light/dark
**named theme pair** defined via `@plugin "daisyui/theme"` blocks in `styles.css`, a
signal-based `ThemeService` that toggles `data-theme` and persists to
`localStorage`, and Font Awesome for iconography. The palette itself is original —
fun, on-brand for beer/brewing, and specifically nods to Colorado (the microbrewery
capital of the world) without being "too wild" for a capstone project.

This document is the spike's deliverable — the decision record, not a plan to
scaffold the frontend app (that's separate future work under Feature #833's other
stories).

## What DaisyUI Is, and How It's Used Here

DaisyUI is a Tailwind CSS plugin that provides semantic component classes
(`btn`, `card`, `alert`, `badge`, etc.) instead of hand-composed utility classes.
Those classes resolve to a small, named set of **theme tokens** —
`primary`/`secondary`/`accent`/`neutral`/`base-100`/`base-200`/`base-300`/
`info`/`success`/`warning`/`error` (each with a paired `-content` text color).
Define those tokens once per named theme, and every DaisyUI component across the
app automatically picks up the right colors — swapping `data-theme` on `<html>`
swaps the entire palette instantly. This is exactly the mechanism AW's
`alpine-circuit` / `alpine-circuit-dark` themes use, and exactly what the two
tables in Decision 1 below define for Beers.

## Decision 1: Theme Pair — "Fourteener"

Named for Colorado's 14,000-ft peaks. `fourteener-pale` (light, default) evokes an
alpine day — granite/snow base with a pale-ale amber primary. `fourteener-stout`
(dark, `prefersdark`) evokes a stout at night — near-black base with the same amber
glowing as the pop color, matching AW's pattern of one named light/dark pair with
full DaisyUI semantic tokens per theme.

### `fourteener-pale` (light, `default: true`)

| Token | Hex | Role |
|---|---|---|
| `--color-primary` | `#D97706` | pale-ale amber |
| `--color-primary-content` | `#1C1917` | charcoal (fixed — cream failed AA contrast, see below) |
| `--color-secondary` | `#166534` | pine forest green |
| `--color-secondary-content` | `#F0FDF4` | |
| `--color-accent` | `#C2410C` | copper kettle |
| `--color-accent-content` | `#FFF7ED` | |
| `--color-neutral` | `#44403C` | granite |
| `--color-neutral-content` | `#FAFAF9` | |
| `--color-base-100` | `#FFFFFF` | snow |
| `--color-base-200` | `#F5F5F4` | alpine mist |
| `--color-base-300` | `#E7E5E4` | deeper granite |
| `--color-base-content` | `#1C1917` | charcoal |
| `--color-info` | `#0284C7` | Colorado sky blue |
| `--color-info-content` | `#FFFFFF` | |
| `--color-success` | `#16A34A` | pine green |
| `--color-success-content` | `#1C1917` | charcoal (fixed — white failed AA contrast, see below) |
| `--color-warning` | `#D97706` | amber |
| `--color-warning-content` | `#1C1917` | |
| `--color-error` | `#DC2626` | mountain-berry red |
| `--color-error-content` | `#FFFFFF` | |

### `fourteener-stout` (dark, `prefersdark: true`)

| Token | Hex | Role |
|---|---|---|
| `--color-primary` | `#F59E0B` | amber glowing against dark |
| `--color-primary-content` | `#1C1917` | |
| `--color-secondary` | `#22C55E` | brighter hop green |
| `--color-secondary-content` | `#0B1710` | |
| `--color-accent` | `#FB923C` | copper glow |
| `--color-accent-content` | `#1C1917` | |
| `--color-neutral` | `#57534E` | mid granite |
| `--color-neutral-content` | `#FAFAF9` | |
| `--color-base-100` | `#1C1917` | stout |
| `--color-base-200` | `#292524` | |
| `--color-base-300` | `#3F3F46` | |
| `--color-base-content` | `#FAFAF9` | snow text |
| `--color-info` | `#38BDF8` | |
| `--color-info-content` | `#1C1917` | |
| `--color-success` | `#4ADE80` | |
| `--color-success-content` | `#0B1710` | |
| `--color-warning` | `#FBBF24` | |
| `--color-warning-content` | `#1C1917` | |
| `--color-error` | `#F87171` | |
| `--color-error-content` | `#1C1917` | |

**Contrast — measured (WCAG relative-luminance formula), not just eyeballed:**

| Theme | Pair | Ratio | Verdict |
|---|---|---|---|
| pale | `secondary`/`secondary-content` | 6.81:1 | AA body text |
| pale | `accent`/`accent-content` | 4.88:1 | AA body text |
| pale | `neutral`/`neutral-content` | 9.84:1 | AA body text |
| pale | `base-100`/`base-content` | 17.49:1 | AAA |
| pale | `warning`/`warning-content` | 5.49:1 | AA body text |
| pale | `error`/`error-content` | 4.83:1 | AA body text |
| pale | `primary`/`primary-content` | 3.07:1 → **5.49:1 after fix** | AA body text (fixed) |
| pale | `success`/`success-content` | 3.30:1 → **5.31:1 after fix** | AA body text (fixed) |
| pale | `info`/`info-content` | 4.10:1 → 4.27:1 (best achievable) | **Known limitation — large text/UI only, not body text** |
| stout | all 9 pairs | 6.32:1 – 16.74:1 | AA body text or better |

Two pairs originally failed AA (4.5:1) for body text: `primary-content` and
`success-content` in `fourteener-pale` were specified as light/cream text, which
only hit 3.07:1 and 3.30:1 against their backgrounds. Fix applied: both now use
dark charcoal (`#1C1917`) as content color instead, which clears AA (5.49:1 and
5.31:1). Tables above reflect the fix.

`info`/`info-content` in `fourteener-pale` does **not** clear 4.5:1 with either
light or dark text at this hue (best achievable is 4.27:1 with dark text) — this
is a genuine limitation of `#0284C7` at this lightness, not a fixable typo. Treat
it as fine for large text, icons, and UI components (WCAG's 3:1 tier), but avoid
using it for small body copy.

When implemented, these become two `@plugin "daisyui/theme" { ... }` blocks inside
the future `web/src/styles.css`, following the exact structure AW uses.

## Decision 2: Icon Set — Font Awesome (free, solid)

Matches AW's icon library choice for consistency across the user's projects.

| Concept | Proposed icon | Status |
|---|---|---|
| Beer | `fa-beer-mug-empty` | Verified present in FA 7.3.1 |
| Brewer / Brewery | `fa-industry` | Verified present in FA 7.3.1 |
| Flight (tasting flight) | `fa-layer-group` | Verified present in FA 7.3.1 — best available stand-in, no literal "flight of glasses" icon exists in the free set |
| Review | `fa-star` | Verified present in FA 7.3.1 |
| Search | `fa-magnifying-glass` | Verified present in FA 7.3.1 |
| Seasonal | `fa-calendar-days` | Verified present in FA 7.3.1 |

**Verified** by grepping the actual installed `@fortawesome/fontawesome-free`
package in AW's `node_modules` (resolved version `7.3.1`, from the `^7.2.0` pin):
all six classes exist as real selectors in `all.css`. No further check needed
before wiring these into components.

## Decision 3: Logo / Favicon — AI-generated circular badge

**Superseded the original hand-coded placeholder.** After drafting the palette,
we generated several candidates via Gemini/Nano Banana image generation (prompts
built from the palette above) and picked the best of what came back rather than
settling for the placeholder sketch.

**Final: `834-assets/beers-logo-badge.png`** (512×512, real alpha transparency) —
a circular craft-brewery badge: copper ring, dark granite mountain, pine-green
tree line, amber pint glass, blank cream banner (left empty for a wordmark later).
Source candidates are kept for provenance in `834-assets/source/`.

Two real problems were found and fixed while finalizing this, not just cosmetic
nitpicks:

1. **The as-generated JPEG had a checkerboard pattern baked into its actual
   pixels**, not real transparency — confirmed by sampling raw pixel values
   (alternating `#C5C5C5`/`#E9E9E9` blocks), not by eyeballing it. It happened
   because the original prompt asked for a "transparent background," which JPEG
   can't represent, so the model drew a literal checkerboard as a stand-in.
   **Fix:** masked the image to its circular ring geometrically (ring bounds
   found by scanning for the copper color, not guessed) to produce genuine alpha
   transparency, verified programmatically (corner alpha = 0, center alpha = 255).
2. **A follow-up compression pass introduced a second, different problem:**
   aggressive palette quantization with dithering corrupted the mountain's shaded
   area into visual noise that looked like more checkerboard but was actually a
   compression artifact, not transparency. Caught by actually looking at the
   output instead of trusting the file size improvement. Fixed by rebuilding from
   the untouched source with plain LANCZOS resizing and no palette dithering.

**A second candidate, `834-assets/source/icon_01.jpeg`** (mountain/mug scene in a
wood frame with a mountain-bike silhouette) was tested empirically as a favicon
candidate — downscaled to real favicon sizes (32×32, 16×16) with `sips` and
visually inspected. It turns into an indistinct color blob at 16px; too much fine
detail and mid-tone contrast to survive downscaling. **Verdict: keep it as
supplementary decorative art (e.g. an about-page image), not a favicon.**

**Caveat on color fidelity:** measured actual pixel colors in the generated art
against the documented hex tokens rather than assuming a match. The beer-liquid
amber landed almost exactly on `primary` (`#D1760D` measured vs. `#D97706`
documented, distance ~11 on a 0–441 scale). But dominant background tones (sky
blues, neutral grays) measured 60–130 units off from `info`/`neutral` — lighter
and more pastel than documented, because the generation prompts described colors
in English and never pasted literal hex codes. **These images are supplementary
brand art, not a literal rendering of the token palette — the hex tables above
remain the authoritative source for the actual DaisyUI theme CSS.**

## Decision 4: Website Banner — AI-generated hero image

Not one of spike #834's five acceptance-criteria bullets, but produced during
this same work and worth recording. **Final: `834-assets/beers-banner-hero.jpeg`**
(2400×595, resized down from a 4128×1024 source for reasonable file size) — three
beer glasses of different shapes lined up on a bar rail against snow-capped
mountains, blue sky with sun rays. Source kept at `834-assets/source/wide01.jpeg`.
Same color-fidelity caveat as Decision 3 applies — treat as brand art, not a
literal token rendering. This is scoped as supplementary asset work for Feature
#833's broader frontend build-out, not a requirement of this spike.

When `web/` is scaffolded, copy the final logo PNG to `web/public/logos/`, export
a rasterized `favicon.ico` from it, and wire it up the same minimal way AW does —
single `<link rel="icon">` in `index.html`, `theme-color` meta tag set to the
primary amber (`#D97706`), no PWA manifest.

## Rationale

- Mirrors AW's proven structure (named light/dark DaisyUI theme pair, signal-based
  theme toggle, Font Awesome, minimal favicon) so the two Angular apps feel like
  siblings and any of the user's existing muscle memory transfers directly.
- Palette is thematically coherent (pale ale / stout as the light/dark metaphor,
  Colorado sky blue for info, pine green for secondary/success) rather than an
  arbitrary Tailwind default swap.
- Kept deliberately simple: one theme pair (not multiple selectable brand themes),
  one icon library, one logo mark — appropriate for a capstone spike.

## Acceptance Criteria Mapping (Spike #834)

- [x] Color palette selected (primary/secondary/accent/background/surface/error) — Decision 1
- [x] App logo / favicon created or sourced — Decision 3 (`834-assets/beers-logo-badge.png`)
- [x] Icon set chosen for key concepts (beer, brewer, flight, review, search, seasonal) — Decision 2, verified against AW's installed Font Awesome 7.3.1
- [x] Design tokens documented for the team — the two tables in Decision 1, contrast-corrected
- [x] Spike findings documented with rationale — this document

## Verification

Everything below was actually run, not just asserted:

- **Contrast:** computed real WCAG relative-luminance ratios for all 18 token
  pairs (both themes). Two failing pairs fixed (see Decision 1's contrast table);
  one (`info`/`info-content` in `fourteener-pale`) documented as a known
  limitation — usable for large text/UI, not small body copy.
- **Font Awesome icons:** grepped AW's actual installed
  `node_modules/@fortawesome/fontawesome-free/css/all.css` (v7.3.1) for all six
  proposed classes — all present.
- **Favicon legibility:** downscaled both icon candidates to 32×32 and 16×16 with
  `sips` and visually inspected the real output — confirmed the circular badge
  survives, the framed mountain/mug scene doesn't.
- **Transparency:** confirmed real alpha channels (not baked-in solid color) via
  `sips -g hasAlpha` and direct pixel sampling with Pillow (corner alpha=0,
  center alpha=255) on the final logo PNG.
- **Color fidelity to tokens:** sampled actual pixel values in the generated art
  and compared distances to the documented hex tokens (see Decision 3's caveat)
  instead of assuming the AI output matched.

Still open, for whoever picks up the actual frontend build (not blockers for
closing this spike):

- Render both themes side by side in a real DaisyUI component test page before
  locking in the CSS — the numbers above are correct, but nobody has looked at
  real buttons/cards/alerts rendered in these themes yet.
- `info`'s sub-4.5:1 contrast in the light theme may be worth revisiting with a
  slightly different hue if it ends up carrying body text anywhere.
