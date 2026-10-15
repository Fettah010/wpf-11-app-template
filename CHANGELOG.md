# Changelog

## 1.2.0 (2026-10-14)

Visual-system upgrade. No new controls, no new packages, no breaking changes.

- Token-driven gap rhythm: seven `AppGap*` Thickness tokens in both
  densities (Comfortable preserves the 1.1.0 look; Compact visibly
  tightens it — 9% pixels on Controls). All 7 Views converted; retired
  literals enforced by `XamlTokenTests`.
- Gallery: category index with anchor scroll + keyboard focus move,
  focusable section headings, keyboard note; new
  `GalleryIndex_JumpsToSection_AndMovesFocus` UI test.
- Dashboard: standard title/description header; page scrolls on short
  windows (table keeps fixed-height virtualization).
- Rail/sidebar + dashboard audit: rail collapses below 800 px, selection
  and persistence verified; no product changes needed beyond the above.
- Visual-regression spike: `eng/compare-screenshots.ps1` (tolerant pixel
  diff, masks, red-overlay artifacts, never auto-approves) + committed
  baselines; advisory-only in 1.2.0 (same-UI noise 0.03%).
- Contrast contract in `docs/design-system/tokens.md`; theme chain
  allowlisted by `ThemeTokenChainTests`. Light/Dark round-trip and
  640–1600 px resize covered by new UI tests.
- Known toolkit gaps (not fixed): `ui:Card` automation names do not
  surface; chart unthemed in dark (D17, → 1.3.0).

## 1.1.0 (2026-10-14)

Stabilization and test truth. No new features; no breaking changes.

- Honest UI gate: skips use `Assert.Skip` (counted, never passes) and
  `eng/check-ui-tests.ps1` fails the required job on any skip or zero
  executed tests. Nav click flake fixed via foreground focus + waited lookup.
- Template matrix: full Clean/Recommended/Full + `IncludeTests=false` + item
  templates run from the source folder and the packed `.nupkg` (CI enforces
  both on every PR/push). Per-run `DOTNET_CLI_HOME` isolation, per-run logs,
  `finally` restore, long-path-safe cleanup.
- `eng/measure-startup.ps1` resolves `*App.exe` dynamically (renames and
  multi-project layouts keep working).
- Docs: real screenshots (`docs/images/`, provenance noted), duplicate 1.0.0
  roadmap row removed, canonical 1.0→2.0 roadmap links, `DEFERRED.md`
  D21–D24, `docs/DEPENDENCIES.md` re-audit (0 vuln, 0 deprecated; Hosting
  10.0.13 patch accepted-not-upgraded).
- Fix: cancelled update check/download reports `Idle`, not `Failed`
  (+ regression tests).
- Template package `DevTemWPF.Templates 1.1.0` (attached to the release).
- Perf (proxy hardware, not low-end evidence): cold-start median 0.75 s
  (max 0.86 s), working set median 144.2 MB, idle 30 s CPU 0%.

## 1.0.0 (2026-10-14)

Stable release. No product changes since 1.0.0-rc.1 (freeze held).

- Template package `DevTemWPF.Templates 1.0.0` (attached to the release).
- Security policy (`SECURITY.md`): supported versions, private reporting,
  7-day triage commitment.
- Support commitments: semver, changelog discipline, migration guide
  (`docs/migration.md`), troubleshooting (`docs/template/troubleshooting.md`).
- Definition of done (roadmap): install → generate → customize → add pages
  → test → ship, verified by the rc.1 + 1.0.0 gates.

## 1.0.0-rc.1 (2026-10-14)

- Frozen surface (`docs/frozen-surface.md`) + machine guard test.
- Template NuGet package (`DevTemWPF.Templates 1.0.0-rc.1`) verified by
  install-from-package generation.
- Visit-all-pages UI automation; theme/mode launch matrix.
- Migration policy + extension points (`docs/migration.md`).
- Tests: 83 passing. No blocking defects.

## 0.9.0 (2026-10-14)

- Beta hardening: FlaUI UI-automation suite (launch, navigate + keyboard,
  dialog), WPF binding-error test guard, startup write-avoidance, idle
  numbers (0% CPU), fresh-clone quickstart proof, full audits, frozen scope.
- Tests: 81 passing. Startup 0.80s / working set 142.5 MB (budgets hold).

## 0.8.0 (2026-10-14)

- `dotnet new` scaffolding from a single template tree: Clean/Recommended/
  Full presets, tests option, generated README + feature manifest.
- Page + service item templates with convention checklists.
- `eng/test-templates.ps1` machine-verifies every combo (generate, residue,
  modules, restore/build/test/launch); CI runs the Clean preset.
- Shell/theme scaffold flags cut after empirical disproof (runtime switching
  documented). Tests: 77 passing (repo suite unaffected by directives).

## 0.7.0 (2026-10-14)

- Production services (all optional/inert by default): Serilog file logging,
  SQLite favorites, typed HTTP client, zip backup/restore, Velopack updates
  (channels, explicit flow), tray integration, EN+de localization.
- Diagnostics page (log viewer/filter/export, support bundle, backup,
  updates, probe, paths/versions). CI workflow + distribution docs.
- Tests: 77 passing. Startup 0.77s+ (re-measured below).

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
