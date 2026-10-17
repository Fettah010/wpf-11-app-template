# ADR-0017: Per-service template modules

- Status: Accepted
- Date: 2026-10-17
- Milestone: 1.6.0

## Context

0.8.0 shipped a single `IncludeServices` flag (all-or-nothing). Roadmap §8.6
wants production modules individually selectable with an explicit dependency
graph, without breaking the named presets or the single-tree approach
(ADR-0012). The template engine's `computed` expressions support only
comparisons and `&&`/`||`/`!`/parentheses — no method calls — so an
`allowMultiple` choice of modules cannot be reduced to per-file conditions.

## Decision

- Split `IncludeServices` into per-module computed symbols: `IncludeLogging`,
  `IncludeLocalData`, `IncludeHttp`, `IncludeBackup`, `IncludeUpdates`,
  `IncludeTray`, each `(Preset == "Full") || (Preset == "Recommended") ||
  (Preset == "Custom" && <flag>)`.
- Add a `Custom` preset with one `bool` parameter per module
  (`CustomLogging`, `CustomLocalData`, `CustomHttp`, `CustomBackup`,
  `CustomUpdates`, `CustomTray`, `CustomDiagnostics`).
- Encode dependency edges in the computed expressions: diagnostics →
  {logging, http, backup, updates}; backup → localdata. (backup → localdata
  because backup protects the local database and its test seeds the DB through
  `FavoritesStore`.)
- Inclusion stays file-level via `sources[].modifiers[].condition`; C# `#if`
  remains only where a whole line must disappear (`App.xaml.cs` logging +
  Velopack hooks, `AppComposition` registrations, `MainWindow` tray).
- Named presets keep byte-identical output.

## Evidence

- Spike (2026-10-17): throwaway template proved `Custom` + a `CustomFeatureA`
  bool + computed `IncludeFeatureA` in a real `dotnet new` run — 5/5 cases
  correct, including that a named preset wins over the flag.
- Generation matrix (2026-10-17) over the real repo template: `empty`, `tray`,
  `localdata`, `backup`, `updates`, `http`, `logging`, `diagnostics` — file and
  package presence/absence all correct; `empty`, `backup`, `diagnostics` build
  `-c Release --warnaserror` clean.
- `TemplateSurfaceTests` updated: per-module symbols + `Custom` choice present,
  `IncludeServices` absent.

## Consequences

- More symbols, but no invented template-engine capability. The dependency
  graph is explicit and testable (presence/absence matrix in
  `eng/test-templates.ps1`).
- A `Custom` selection can pull related modules in (documented edges) rather
  than producing a non-building scaffold.
- The extension is additive to `Preset`; `Clean`/`Recommended`/`Full` are
  unaffected.

## Revisit triggers

A template-engine release adding method or comprehension support in `computed`
expressions would allow a single multi-select flag instead of one bool per
module.
