# Frozen surface (1.0.0-rc.1)

Any change here restarts candidate validation (new rc + full gate). Guarded
by `TemplateSurfaceTests` where machine-checkable.

## Template CLI (frozen)

- Short names: `devtemwpf-app`, `devtemwpf-page`, `devtemwpf-service`.
- `devtemwpf-app` symbols: `Preset` {Clean, Recommended (default), Full},
  `IncludeTests` (bool, default true). Computed: `IncludeDashboard`
  (Full only), `IncludeDiagnostics`/`IncludeServices` (non-Clean).
- `sourceName`: `DevTemWpf`. No `--Shell`/`--ThemeDefault` (cut in 0.8.0
  with reason; runtime switching is the supported path).
- Item templates: `-n` names the item (`sourceName`), `--Namespace` sets the
  root namespace.

## Shell contracts (frozen)

- `ShellMode` {Rail, Sidebar}; `NavigationView` pane mapping
  (Rail→LeftMinimal, Sidebar→Left); responsive collapse <800px.
- Pages: `INavigableView<T>` + `INavigationAware`; registration as
  transients in `AppComposition`; nav items in `MainViewModel`.
- Composition root shape: `AppComposition.ConfigureForApp` + `AddAppLogging`
  (app only); services behind interfaces; code-behind holds lifecycle/chrome.

## Configuration shape (frozen)

- `ShellSettings` JSON fields: Theme, BackdropEnabled, Shell, Density,
  AccentOverride, Motion, EffectsEnabled, UpdateChannel, MinimizeToTray,
  Culture. Additive only — unknown fields ignored, missing fields default.
- `appsettings.json` sections: `App`, `Http`, `Update`.

## Generated structure (frozen)

- `src/<Name>.App/`, `tests/` (unless off), `eng/smoke.ps1`,
  `global.json`, props, `README.md` + `FEATURES.md` (module manifest).

## Still allowed without restart

Patch-level bug fixes, non-contract internals, docs, test-only changes.
Anything else → new rc.
