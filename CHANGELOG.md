# Changelog

## 1.8.0 - Architecture preparation

Architecture proof for the 2.0 package-boundary direction. The app and
generated-app contract remain in place; only compiled design-token resources
move behind a tested library boundary.

- **UI resource package proof**: `DevTemWpf.UI` owns the compiled token
  dictionaries; app-level merge order and density selection remain unchanged.
  An independent package-only WPF consumer resolves Comfortable and Compact
  resources and runtime density replacement without a project reference to
  the gallery app. The prototype package is not a stable public control API.
- **No speculative splits**: Windows integration, motion, shell, and optional
  services remain app-owned. ADR-0020 records the narrow accepted boundary and
  the evidence required before any Windows/service package is considered.
- **Migration contract**: a compatibility draft records the preserved 1.x
  template flags, item-template IDs, resource keys, DI root, and settings
  behavior, plus what still must be decided before the 2.0 preview.
- **More reliable UI automation**: each test uses a temporary app-data profile
  that survives relaunches but cannot leak state to other tests; resize targets
  respect the runner's desktop bounds, and narrow-width navigation uses the
  keyboard command palette when the responsive pane is collapsed.
- **Verification**: warning-free Release build; full suite passed 234/234
  locally (including all 18 UI automation tests) with no skips; complete
  source and packed generator/template gates passed; package-only UI canary
  passed; vulnerability/deprecation audits were clean. All five PR checks
  passed and the stack merged. The first default-branch release preflight
  passed build, test, UI, generator, and package-canary gates, then caught
  package-build artifacts under the UI library's `obj` folder in the source
  template residue check. The exclusion fix is in a follow-up PR; a green
  hosted rerun and default-branch preflight are required before the candidate
  can be pinned or the scheduled release published. The fix and a redundant
  resize-test click correction have since merged in PR #11; the release was
  rescheduled for 23:50 UTC after the original preflight missed its window.
  The following preflight showed that restore creates a generated project's
  `.nuget/packages` cache outside `bin`/`obj`. The residue check now excludes
  that build-output cache while still scanning generated source and project
  files; a green hosted rerun is required before release.
- **More reliable template gates**: each native command now has isolated
  diagnostic logs, and smoke scripts return an explicit success code after
  passing. CI runs source and packed preset checks concurrently while retaining
  aggregate required checks. Local `-NoLaunch` runs keep unit/integration
  coverage without opening WPF windows or running interactive UI automation.
- **Documentation**: scaffold workflow diagram, concise project direction,
  UI-package instructions, and updated testing commands.

## 1.7.0 (2026-10-27)

Advanced scaffolding and developer productivity (roadmap §8.7). Item
templates become a complete catalog, drive their namespaces from one root
namespace flag, derive companion type names from the item name, and are
verified by a blocking generator gate.

- **Nine new generators** alongside `devtemwpf-page` / `devtemwpf-service`:
  `devtemwpf-settings`, `devtemwpf-listdetail`, `devtemwpf-datagrid`,
  `devtemwpf-treeview`, `devtemwpf-tabpage`, `devtemwpf-control`,
  `devtemwpf-viewmodel`, plus behavioral test scaffolds `devtemwpf-test-vm`
  and `devtemwpf-test-service`. Every scaffold emits token-based XAML,
  CommunityToolkit.Mvvm code, `INavigableView<T>` pages, and entrance motion
  where a page is involved.
- **Naming/namespace contract fixed**: item templates now take
  `--RootNamespace <Root>` (replaces the whole `DevTemWpf` root, preserving
  `.App`/`.Unit`), so test scaffolds resolve the app namespace too. Companion
  types derive from `-n` (`OrdersPage` → `OrdersRow`/`OrdersItem`/`OrdersNode`,
  `AuditServiceTests` → `AuditService`) via template `forms`, so repeated
  generation cannot collide.
- **Blocking generator gate** `eng/test-generators.ps1`: isolated CLI home,
  source-folder or `.nupkg` install, all eleven templates plus repeat and
  edge-case generations, residual-token detection (`DevTemWpf`, `Sample*`,
  undefined `App*` styles), `--warnaserror` build, and generated-test run.
  Fixed generator defects it caught: invalid `Themes/Generic.xaml` setter
  form, missing `AutomationPeer` using/override, a read-only collection
  assigned in an initializer, and an undefined `AppBodyStrongText` style.
- **Visual Studio decision recorded** (ADR-0019, closes D08r as CLI-first):
  project template appears in **New Project** after
  `dotnet new install`; a VSIX for **Add → New Item** is rejected for 1.x for
  cost/duplication reasons - documented rather than implied.
- **Generated apps stay clean**: the project template now excludes the
  repository's `.kilo/`, `.idea/`, and `*.user` files, and the generator gate
  asserts that no tool config, build output, or absolute developer path leaks
  into generated output.
- **Docs**: new [`docs/template/item-templates.md`](docs/template/item-templates.md)
  catalog (flags, companion naming, output folders, post-generation
  checklists), refreshed `generators.md`, README and scaffold `FEATURES.md`.

## 1.6.0 (2026-10-18)

Production modules, Windows integration assessment, and optionality (roadmap
§8.6). Named presets stay byte-identical to 1.5.0; the single `IncludeServices`
switch becomes per-module flags, and every module — including the two new
samples — can be turned off completely (files, packages, DI, config, tests).

- **Per-service modules**: `IncludeServices` is replaced by
  `IncludeLogging` / `IncludeLocalData` / `IncludeHttp` / `IncludeBackup` /
  `IncludeUpdates` / `IncludeTray` (+ existing `IncludeDashboard`,
  `IncludeDiagnostics`), each reachable in `Custom` via a `--Custom<Module>`
  flag. Dependency edges: diagnostics → {logging, http, backup, updates};
  backup → localdata; **crash → logging** (shared redaction). Generated output
  carries no template residue and no `#if` directives (ADR-0017).
- **Logging privacy & bounds** (new): `LogPrivacy` + `RedactingTextFormatter`
  mask secret-shaped keys and `Bearer` credentials on every line, never throw,
  and the file sink is capped at 5 MB with 14 retained files in addition to
  daily rolling (ADR-0018).
- **Richer update surface**: Diagnostics now shows installed vs available
  version, delta-vs-full, an explicit **Restart now** prompt (no auto-restart),
  `Retry`/`Cancel`, and an opt-in downgrade switch that confirms first.
  Failures become status, never a crash (ADR-0014).
- **Optional auth sample** (`--CustomAuth`): MSAL (`Microsoft.Identity.Client`
  4.91.0, MIT) public client + `IAuthService`; no client secret, no UI, silent
  cache-only restore at startup. Unconfigured ⇒ no provider call, no network
  (ADR-0017).
- **Optional crash sample** (`--CustomCrash`): local-first, redacted capture
  (`ICrashReporter`, `ICrashSink` adapter seam) under `%LOCALAPPDATA%`; no
  vendor, no telemetry. Reuses `LogPrivacy.Redact` (ADR-0017/0018).
- **Windows app notifications: NO-GO in 1.6.0** (deferred, D07t). The
  `AppNotificationManager`/WindowsAppSDK spike built and registered unpackaged,
  but interactive cold-start activation could not be exercised headlessly and
  the dependency adds unmeasured startup/working-set cost to every measured
  gate; no package or flag ships — the in-app Snackbar is the fallback.
- **Distribution docs & tooling**: `docs/release/packaging.md` separates the
  portable / Velopack / MSIX profiles and separates signing (owner-owned); new
  `eng/pack-portable.ps1` publishes, zips, hashes, and explicitly reports
  signed vs UNSIGNED. `docs/release/` is now tracked.
- **Guides**: `module-selection`, `updates-and-channels`, `backup-restore`,
  `auth-and-crash`.
- **Tests**: 234 total (215 unit + 1 integration + 18 UI), 0 skipped (1.5.0
  baseline 196). New: log-privacy, update-service, auth, and crash suites;
  `eng/test-templates.ps1` gains a 5-combo Custom presence/absence module
  matrix (built and asserted) and named-preset sample-absence checks.
- **Template packaging**: `DevTemWPF.Templates 1.6.0`. `eng/test-templates.ps1`
  now spawns the generated smoke with `-NoProfile` so a machine's PowerShell
  profile cannot fail the gate.
- No telemetry and no new default dependency; `Microsoft.ApplicationInsights`
  remains test-only.

## 1.5.0 (2026-10-17)

Motion system and effects prototype (roadmap §8.5). Additive only — no
breaking changes, no new packages.

- Motion profiles: new `Balanced` level between Reduced and Rich, appended as
  `MotionMode.Balanced = 4` (existing values unchanged; `Motion=1` still
  resolves to Rich). `Auto` now settles on Balanced; Rich stays an explicit
  opt-in, so decorative effects never run by default (ADR-0016).
- Motion tokens: added Stagger, exit distance, and opacity/scale values;
  reduced-motion still snaps.
- Motion primitives: cancellable enter/exit, selection, expand/collapse,
  feedback, and reduced-motion-aware page transitions (`IMotionService`); all
  snap to their final state on cancellation or failure.
- Motion lifecycle: shared `MotionScope` cancels in-flight motion and runs
  cleanup on navigation-away, unload, theme change, deactivation, and
  shutdown; no `CompositionTarget.Rendering` hooks (repo-only guard test).
- Effects showcase: the Effects page now controls the master switch and each
  effect (ripple, gradient, warp) live, reports the warp fallback reason, and
  (development builds only) prints the resolved tokens and render tier. Each
  switch is gated through `IMotionService.AllowsEffect`.
- Accessibility guards: repo-only tests fail if motion adds a
  `CompositionTarget.Rendering` hook or any render-target `Effect`
  (Blur/DropShadow/Shader), keeping ClearType/hit-testing intact; reduced
  motion and Off are covered per primitive.
- Backdrop warp prototype: a simulated, backdrop-only radial-highlight drift —
  no HLSL shader, no text or controls inside; off by default with a static
  fallback (ADR-0016 records the shader no-go).
- Docs: README status/badge/roadmap, recaptured `gallery-home.png` + new
  `gallery-effects.png`; `motion.md`, `frozen-surface.md`, ADR-0016; HLSL
  shader deferred in `docs/DEFERRED.md` (D13).
- Template package `DevTemWPF.Templates 1.5.0` (attached to the release).

## 1.4.1 (2026-10-17)

Tabbed shell themed-surface fix. No new packages, no breaking changes, no
template-surface change.

- Fixed: the Tabbed shell's document area rendered a pure-white panel (67.9%
  of the window in Dark) because WPF-UI's implicit `TabControl` style is not
  applied to the derived `ui:TabView`; the built-in WPF (white) template was
  used instead. `ui:TabView` now applies the WPF-UI `TabControl` style
  explicitly, so the document surface matches the themed host (`#272727`) with
  no seam (dark and light verified).
- Fixed: the detail `GridSplitter` used the default bright `#F0F0F0` paint; it
  now uses the `AppDividerBrush` token (`#3A3A3A`).
- Regression guard: repo-only `TabbedShellSurfaceTests` fails on the pre-fix
  revision and passes after. New `eng/verify-shell-surfaces.ps1` captures the
  maximized shell deterministically (PrintWindow) and asserts the
  theme-surface invariants (white-fraction threshold, splitter color); fault
  injection on the pre-fix build fails as expected.
- Docs: README status/badge + roadmap rows, recaptured `shell-tabbed.png` and
  `shell-top.png`; `docs/releases/1.4.1-report.md` with root cause and A/B
  evidence.
- Template package `DevTemWPF.Templates 1.4.1` (attached to the release).

## 1.4.0 (2026-10-16)

Shells and workspaces. No new packages, no breaking changes (one additive
template parameter; `ShellMode` gains additive members).

- Four shell presets sharing one route registry (`Services/NavigationRoutes`):
  Sidebar, Rail, Top-navigation bar, and Tabbed document workspace.
  `dotnet new --Shell Sidebar|Rail|Top|Tabbed` seeds the first-launch
  default (all shells ship in every scaffold); template surface amended
  (`frozen-surface.md` + `TemplateSurfaceTests`).
- Shell changes apply at next launch (ADR-0015): swapping `PaneDisplayMode`
  on a live WPF-UI 4.3.0 `NavigationView` crashes intermittently (NaN
  `DoubleAnimation` during re-template; upstream fixed only the load path).
  Settings documents the restart step and reuses the Restart button.
- Tabbed workspace: route-keyed documents (same pages as every shell) in a
  `TabView`, open/focus/close, selection-synced rail, Back/Forward history,
  keyboard-operable reorder (detail Move buttons) + resizable detail split
  with persisted width, state persistence with malformed/stale recovery.
  Documents host pages in per-tab chromeless `Frame`s (WPF parents `Page`
  only to `Window`/`Frame`). Pointer drag prototyped and deferred (see
  report); Move buttons are the reorder contract.
- Command palette (Ctrl+K + header button): stable `nav.*` IDs, substring
  filter, enabled states, Enter/Esc/mouse contract, focus return to invoker.
- Stale-guard rework: per-route-key independent opens (rapid opens all
  complete; same-key doubles collapse; close/leave cancels); UI-bound
  continuations rejoin the dispatcher.
- Template engine lesson: a literal `//#if` inside comments silently
  truncates generated files (matrix now asserts zero tokens); directives
  belong at statement level.
- Template package `DevTemWPF.Templates 1.4.0` (attached to the release).
- Perf (proxy hardware): cold-start 0.9 s / working set 134.6 MB Sidebar
  (budgets hold); Tabbed + 3 documents 0.92 s / 150.9 MB (per-document cost
  documented, no 8 GB claim); idle 0%.

## 1.3.0 (2026-10-15)

Core control and data completion. No new packages, no breaking changes.

- New Data gallery page: editable validated `DataGrid` (sort/filter/search,
  selection, add-rows, loading/error/empty states, 10k toggle), `TreeView`
  (selection, expand-all, persisted expansion), gallery-scoped time editor
  (WPF-UI `TimePicker` rejected: unnamed + broken popup on 4.3.0, spike
  evidence in report).
- Chart theming: transparent chart, paints rebound on theme/accent change,
  textual trend summary for screen readers; light + dark verified.
- Full gallery localization (220+ keys, en+de; automation names invariant;
  culture-aware weekday labels) + `Strings.OverrideCulture` fix for
  thread-culture page construction.
- Cancellable busy demo (owned CTS, `Idle` on cancel) with unit + UI tests.
- Fixes found by testing: `HasResults` no longer uses `IsEmpty`
  (`NewItemPlaceholder` keeps it false); `ReplaceAll` notifies `View`;
  no reload-on-navigation race; dashboard scrolls on short windows.
- Template package `DevTemWPF.Templates 1.3.0` (attached to the release).
- Perf (proxy hardware): cold-start ~0.8 s, working set ~145 MB, idle 0%;
  10k filter + 10k tree walk well under 2 s wall-clock.

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
