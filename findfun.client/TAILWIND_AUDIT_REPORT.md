# Tailwind & Dark-Mode Audit — Summary Report

Date: 2026-03-24
Scope: `findfun.client/src/components`, `findfun.client/src/views`, `findfun.client/src/composables`, global CSS

## Quick summary
- Low-risk automated fixes applied (committed): replaced simple inline margin/text-decoration/position styles with Tailwind utilities in several files.
- Remaining items flagged for manual review: dynamic `:style` bindings, `imgStyle` props, explicit pixel sizes, third-party (PrimeVue) theme coverage, and missing `tailwind.config.js` in repo root.

## Files changed (low-risk fixes)
- `src/App.vue` — added `my-app-dark` class binding and initialization from `localStorage` / `prefers-color-scheme` to support PrimeVue theme selector.
- `src/components/Details.vue` — replaced simple inline margins with `mb-4`, `mt-4`, `mt-8`, `mt-4`.
- `src/components/DesktopNavigationBar.vue` — replaced inline `style="text-decoration: none"` with Tailwind `no-underline`.
- `src/components/MobileNavigationBar.vue` — replaced inline `style="text-decoration: none"` with Tailwind `no-underline`.
- `src/components/ActionsCard.vue` — replaced `style="margin-right: 0.5rem"` with `mr-2`.
- `src/components/ImageCard.vue` — replaced `style="left: 4px; top: 4px"` with `left-1 top-1`.

## Flagged files (manual review recommended)
- `src/components/Details.vue` — dynamic `:style` bindings used for color/background (air quality/weather metrics). Keep dynamic bindings; convert to theme-aware CSS variables only after design decision. Severity: Medium.
- `src/components/ActionsCard.vue` — dynamic color/background `:style` bindings. Severity: Low/Medium.
- `src/components/Card.vue`, `src/components/MainCarousel.vue` — `imgStyle` / `style` props with inline image sizing and object-fit. Severity: Medium.
- `src/components/MapLoader.vue`, `src/components/ParkEventTab.vue` — inline px sizes for spinner and icons. Severity: Low.
- `src/components/NavBarUserSection.vue` — inline `style="cursor: pointer"` — minor.
- Files with no Tailwind classes (candidates): (scan incomplete listing available on request) — most UI files already use Tailwind.

## Recommendations
1. Convert runtime color values to CSS variables (e.g., `--metric-color`) and use Tailwind-compatible utilities that reference those variables, or set `dark:` variants where appropriate.
2. Keep dynamic `:style` bindings for runtime colors where color values are driven by external data; instead extract spacing/positioning to Tailwind.
3. Confirm Tailwind config location — project uses Tailwind as dependency but no `tailwind.config.js` found in client root; locate or create it before broad theme changes.
4. For PrimeVue controls, prefer CSS variable overrides or `tailwindcss-primeui` patterns rather than changing PrimeVue internals.

## Next Steps (if you approve)
- Create branch `tailwind/dark-audit-fixes` with the low-risk commits (done).
- Continue converting simple inline pixel styles to Tailwind utilities in small commits.
- Produce a full per-file JSON export of matches if you want exhaustive data.
- Open PR with screenshots and checklist.

---

If you want the full per-file JSON array (thorough scan), I can produce and attach it next. Otherwise I will commit the report and run `npm --prefix ./findfun.client run type-check` to validate type checks.