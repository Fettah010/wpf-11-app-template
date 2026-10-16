# Shells, workspace, and command palette (1.4.0)

Four shell presets share one navigation contract. All shells ship in every
scaffold (they are runtime modes, not file sets); `dotnet new --Shell`
selects the first-launch default (`Sidebar` unless chosen).

## Presets

| Preset | Chrome | Best for |
|---|---|---|
| `Sidebar` (default) | Expandable left nav (`Left`) | General desktop apps |
| `Rail` | Icon-only left nav (`LeftMinimal`) | Narrow/focused tools |
| `Top` | Horizontal top bar (`Top`) | Small apps, wizard-like flows |
| `Tabbed` | Rail + document tabs (`LeftMinimal` + workspace) | Multi-document workflows |

Rail/Sidebar/Top reuse pages untouched. Tabbed hosts the same page types as
documents (see below). Narrow windows (<800 px) collapse the left pane in
every mode; widening restores the applied shell.

## Restart-apply rule (ADR-0015)

Shell selection persists immediately but applies at **next launch**, like the
language setting. Swapping `PaneDisplayMode` on a live WPF-UI 4.3.0
`NavigationView` crashes intermittently (NaN `DoubleAnimation` during
re-template; upstream fixed only the load path). The Settings page documents
this next to the selector and reuses the Restart button. Do not reintroduce
live swapping without a passing hot-switch UI test on the new toolkit version.

## Route registry (`Services/NavigationRoutes.cs`)

Stable string keys (`home`, `controls`, ...) are the identity used by the
rail, the palette, and workspace documents/persistence — never page `Type`s.
Adding a page: register it in `AppComposition`, add a registry entry (with
the preset guard when conditional), and add the resx title. Page/ViewModel
implementations work across all shells without shell-specific rewrites.

Template-authoring rule (1.4.0 T08 evidence): never write the literal text
`//#if` inside comments of template-processed files — the engine treats it
as a conditional and silently truncates the file from that line on. The
matrix asserts zero remaining tokens for this reason.

## Document workspace (Tabbed shell)

`WorkspacePage` + `WorkspaceViewModel` (singleton, lazy restore so other
shells pay no startup cost). Rail clicks open/focus route-keyed documents;
the rail highlight follows tab selection; document Back/Forward history is
kept per session.

- Documents host full pages through a chromeless per-tab `Frame` (WPF only
  parents a `Page` to a `Window`/`Frame` — hosting pages directly in tab
  content crashes at layout; proven in 1.4.0).
- Hooks follow tab semantics: `OnNavigatedToAsync` once on open,
  `OnNavigatedFromAsync` once on close; tab switches only change selection.
- Opens are independent per route key; same-key double opens collapse into
  one; closing cancels an in-flight open; leaving the page cancels all.
  UI-bound continuations rejoin the dispatcher (see `RunOnUiThreadAsync`).
- Persistence (`ShellSettings.Workspace`): open keys in order, selected key,
  detail width, detail visibility. Malformed/stale state falls back to a
  single home document; startup never fails because of settings.
- Reorder: drag tab headers (private `DevTemWpf.DocumentTab` format — never
  leaves the workspace, Esc cancels, one gesture at a time) or the detail
  pane Move buttons (the keyboard alternative, always available).
- Split: `GridSplitter` between tabs and the detail inspector; width
  persists on release; the toggle checkbox collapses the pane (keyboard
  reachable). The splitter itself is arrow-key operable when focused.
- Accessibility: tabs are natively keyboard-navigable; every action has a
  named button equivalent; validation/state is text, never color-only.
  Automation probes must target peer-backed elements — plain `Border`s
  expose no UIA peer (proven by UI runs), so hooks live on buttons/text.

## Command palette (Ctrl+K + header button)

`PaletteViewModel` (stable `nav.{key}` IDs, substring filtering,
explicit enabled states) with a shell overlay: filter box, results list,
empty notice. Contract: keyboard-complete (type/arrows/Enter/Esc), mouse
usable (click select, double-click execute, scrim click dismisses), initial
focus in the filter, focus returns to the invoker on dismiss, executed
routes focus their nav item. In Tabbed mode commands open documents.

## Testing a new shell or page

Unit: registry completeness, mapping, workspace open/close/move/persist/
recovery, palette filter/IDs/disabled, race guards. UI: navigate every page
per shell, tab open/focus/close/persist across restart, palette keyboard
round-trip + focus return, drag reorder, detail toggle, rapid navigation.
Matrix: all four `--Shell` values generate + build + launch (source and
packed `.nupkg`); presets keep full smoke; Full+Tabbed is the high-risk
full smoke (charts inside tabs).
