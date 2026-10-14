# Changelog

## 0.6.0 (2026-10-14)

- Premium dashboard: stat cards, LiveCharts2 line chart, sortable/filterable
  virtualized activity table (1500 rows), master/detail, empty states, setup
  stepper — all on deterministic sample data, isolated for template exclusion.
- LiveCharts2 2.0.5 (MIT, app-only). Master/detail + settings-section page
  patterns; top-nav/tabbed shells deferred to 0.8.0 with reasons.
- Tests: 56 passing (data, filter/sort/select/stepper, virtualization assert).
- Zero other packages. Startup 0.77s / working set 132.2 MB (budgets hold).

## 0.5.0 (2026-10-14)

- Motion system: tokenized durations/easings, Auto/Full/Reduced/Off profiles
  (OS animation setting + render tier), page entrances, reduced-motion and
  failure fallbacks that snap final state.
- Effects showcase page (presets, click ripple, opt-in animated gradient)
  behind the effects switch; warp shader explicitly deferred (see motion.md).
- Tests: 45 passing (policy matrix, instant path, ripple lifecycle).
- Zero new packages. Startup 0.77s / working set 131.8 MB (budgets hold).

## 0.4.0 (2026-10-14)

- Controls gallery page (buttons, inputs, cards/expanders, InfoBar, progress,
  validated sample form) with usage notes; `docs/design-system/components.md`.
- Feedback services wired: snackbar + content-dialog singletons with shell
  overlays; VM commands fake-tested (both dialog paths, busy single-flight).
- Zero custom controls, zero new packages. Budgets ratified (start ≤1.0s,
  working set ≤150 MB, modern HW). Tests: 34 passing.
- Startup 0.68s / working set 131.7 MB (gallery resolves on navigation).

## 0.3.0 (2026-10-14)

- Token layer (`DesignTokens/`): semantic color aliases, type ramp, comfortable/
  compact spacing, radii; add-only over WPF-UI, documented override rules.
- Density selector + custom accent override (`#RRGGBB/#AARRGGBB`, system restore),
  live-applied and persisted; HC-compatible by alias design.
- Tokens preview page (colors/type/spacing/shapes/states); all app XAML off
  hard-coded style values; `docs/design-system/` standards.
- Tests: 25 passing (accent ordering, hex validation, page-XAML guards).
- Zero new packages. Startup 0.73s / working set 131.4 MB (vs 0.71s / 130.4 MB).

## 0.2.0 (2026-10-14)

- Application shell: `FluentWindow` + `TitleBar` + adaptive `NavigationView`
  with rail (`LeftMinimal`) and sidebar (`Left`) modes, responsive collapse,
  back navigation, `BreadcrumbBar`, Home + Settings pages.
- Services: JSON settings persistence (`%AppData%`), system/light/dark theme
  service, Mica backdrop service with opaque fallback (ctor-time DP rule).
- Tests: 16 passing (settings/theme/backdrop/shell units incl. `WpfFact` STA
  coverage, extended host integration test).
- New packages: `WPF-UI.DependencyInjection 4.3.0` (MIT, same vendor —
  v4 page provider), `Xunit.StaFact 4.0.24` (MS-PL, test-only).
- ADRs 0005–0006; startup 0.71s / working set 130.4 MB (vs 0.61s / 116.3 MB).

## 0.1.0 (2026-10-14)

- Repository foundation: `global.json` SDK pin, central package management,
  analyzers/formatting conventions, solution skeleton.
- Minimal runnable WPF app on `net10.0-windows10.0.26100` with Generic Host,
  typed options, MVVM ViewModel, and WPF-UI 4.3.0 theme prototype.
- xUnit.net v3 (MTP mode) test projects: ViewModel unit tests + host
  lifecycle integration test (3 passing).
- `eng/smoke.ps1` gate and `eng/measure-startup.ps1` baseline tooling.
- ADRs 0001–0004, dependency/license inventory, scaffold contract,
  first performance baseline.
