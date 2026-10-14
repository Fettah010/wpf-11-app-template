# ADR-0001: WPF-UI as the primary Fluent UI foundation

- Status: Accepted
- Date: 2026-10-14
- Milestone: 0.1.0

## Context

The roadmap mandates a primary Fluent/WPF component foundation and forbids
layering overlapping theme/control suites by default.

## Options considered

1. **WPF-UI (`lepoco/wpfui`)** — Fluent styles, navigation, dialogs,
   `NumberBox`, title-bar/window features, tray support, gallery.
2. **ModernWPF** — 1.x line still in RC/preview at last check; overlaps WPF-UI.
3. **MaterialDesignInXamlToolkit / MahApps.Metro** — different visual language
   (Material/Metro) or overlapping window theming; reference-only per roadmap.

## Decision

Adopt **WPF-UI 4.3.0** as the primary UI dependency.

## Evidence (checked 2026-10-14)

- Latest stable release `4.3.0` (2026-05-04,
  `github.com/lepoco/wpfui/releases/tag/4.3.0`); older 4.x lines are
  deprecated on NuGet for critical bugs — 4.3.0 is the fixed line.
- NuGet `WPF-UI 4.3.0` ships explicit TFMs `net8.0/net9.0/net10.0-windows7.0`
  (+ net462/472/481); sole runtime dependency is `WPF-UI.Abstractions 4.3.0`.
- License: MIT (`github.com/lepoco/wpfui/blob/main/LICENSE`).
- Proven in this milestone: `net10.0-windows10.0.26100` consumer app restores,
  builds with 0 warnings, launches, and shows a window using
  `ThemesDictionary` + `ControlsDictionary` + `ui:Button` + `ui:SymbolIcon`
  (`eng/smoke.ps1` launch check).

## Consequences

- Extend via resource dictionaries, adapters, and the `ThemeBootstrap` seam;
  no private fork without a documented blocker.
- Any additional theme/control suite needs a PoC + conflict review + license
  check + new ADR.

## Revisit triggers

WPF-UI archived/unmaintained, license change, or a proven gap that cannot be
closed cleanly in our UI layer.
