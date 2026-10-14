# ADR-0007: Token layer, density, and accent override

- Status: Accepted
- Date: 2026-10-14
- Milestone: 0.3.0

## Context

Roadmap 0.3.0 needs centrally defined tokens, runtime themes, compact
density, accent override, and HC compatibility — composed over WPF-UI 4.3.0
without fragile upstream patching.

## Decision

- `DesignTokens/` add-only layer (semantic aliases via DynamicResource;
  static metrics; full layering/rules in `docs/design-system/tokens.md`).
- Density by whole-dictionary swap (`DesignTokenBootstrap`), persisted,
  Comfortable fallback. WPF-UI has no density concept — verified absent.
- Accent override (`#RRGGBB/#AARRGGBB`, null = system) applied after every
  theme apply + on `Changed` (accent-only, no recursion — source-verified).
  Validation in the VM; service falls back to system accent defensively.
- `TokensPage` as the living preview; XAML refactor onto tokens (no
  unexplained literals); `docs/design-system/` standards.
- Zero new packages.

## Evidence

- 4.3.0 `Dark.xaml` token system + `ApplicationAccentColorManager` /
  `WindowBackgroundManager` sources read (2026-10-14).
- Launches: defaults, Light+Compact+accent, Dark+None+Rail — all window-up.
- 25/25 tests (settings/accent/page-XAML guards); vuln scan clean.

## Consequences

- Token consumers must use StaticResource for spacing (swap rule).
- Custom accent under HC is author responsibility (documented).

## Revisit triggers

Second token consumer (UI library split), motion tokens (0.5.0), full accent
palette UI (deferred).
