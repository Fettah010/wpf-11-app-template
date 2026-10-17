# Frozen surface (1.0.0-rc.1)

Any change here restarts candidate validation (new rc + full gate). Guarded
by `TemplateSurfaceTests` where machine-checkable.

## Template CLI (frozen)

- Short names: `devtemwpf-app`, `devtemwpf-page`, `devtemwpf-service`.
- `devtemwpf-app` symbols: `Preset` {Clean, Recommended (default), Full,
  Custom}, `Shell` {Sidebar (default), Rail, Top, Tabbed} (added in 1.4.0,
  roadmap-authorized additive change — seeds the first-launch shell default
  via the `ShellDefaultValue` switch symbol; all shells ship in every
  scaffold), `IncludeTests` (bool, default true). Computed: `IncludeDashboard`
  (Full only), `IncludeDiagnostics` (non-Clean, or Custom+`CustomDiagnostics`),
  and the per-module `IncludeLogging` / `IncludeLocalData` / `IncludeHttp` /
  `IncludeBackup` / `IncludeUpdates` / `IncludeTray` (non-Clean, or Custom with
  the matching `Custom<Module>` flag). Added in 1.6.0 (roadmap §8.6,
  roadmap-authorized): the single `IncludeServices` flag was replaced by these
  per-module flags; the `Custom` preset + `Custom<Module>` bools are new
  surface. Custom dependency edges: diagnostics → {logging, http, backup,
  updates}; backup → localdata; crash → logging. Opt-in samples added in 1.6.0
  (Custom preset only, never named presets): `IncludeAuth` / `CustomAuth` and
  `IncludeCrash` / `CustomCrash`. Named presets are byte-identical to 1.5.0.
- `sourceName`: `DevTemWpf`. No `--ThemeDefault` (cut in 0.8.0
  with reason; runtime switching is the supported path).
- Item templates: `-n` names the item (`sourceName`), `--Namespace` sets the
  root namespace.

## Shell contracts (frozen)

- `ShellMode` {Rail, Sidebar} plus 1.4.0 additions {Top, Tabbed};
  `NavigationView` pane mapping (Rail→LeftMinimal, Sidebar→Left, Top→Top,
  Tabbed→LeftMinimal + workspace); responsive collapse <800px. Shell
  selection applies at next launch (ADR-0015).
- Pages: `INavigableView<T>` + `INavigationAware`; registration as
  transients in `AppComposition`; nav items in `MainViewModel`.
- Composition root shape: `AppComposition.ConfigureForApp` + `AddAppLogging`
  (app only); services behind interfaces; code-behind holds lifecycle/chrome.

## Configuration shape (frozen)

- `ShellSettings` JSON fields: Theme, BackdropEnabled, Shell, Density,
  AccentOverride, Motion, EffectsEnabled, UpdateChannel, MinimizeToTray,
  Culture, plus 1.5.0 additions RippleEffectEnabled, GradientEffectEnabled,
  WarpEffectEnabled. Additive only — unknown fields ignored, missing fields
  default. `Motion` enum values are append-only (`Balanced = 4`, 1.5.0).
- `appsettings.json` sections: `App`, `Http`, `Update`, plus 1.6.0 additive
  empty-default sections `Auth` (public-client ids) and `Crash`.

## Generated structure (frozen)

- `src/<Name>.App/`, `tests/` (unless off), `eng/smoke.ps1`,
  `global.json`, props, `README.md` + `FEATURES.md` (module manifest).

## Still allowed without restart

Patch-level bug fixes, non-contract internals, docs, test-only changes.
Anything else → new rc.
