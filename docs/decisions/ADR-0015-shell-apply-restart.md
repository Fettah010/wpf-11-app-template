# ADR-0015: Shell changes apply at restart (no live PaneDisplayMode swap)

- Status: Accepted
- Date: 2026-10-16
- Milestone: 1.4.0

## Context

1.4.0 adds Top-navigation and Tabbed shells (`ShellMode.Top/Tabbed`,
`NavigationRoutes` registry). The initial design hot-swapped
`NavigationView.PaneDisplayMode` on the live control when the user changed
the shell in Settings.

## Evidence

Repeated UI-automation runs (Sidebar→Top through the real Settings UI,
`TopShell_*` in `tests/DevTemWpf.UiAutomation/ShellFlowsTests.cs`) crashed
the app intermittently with:

```text
System.InvalidOperationException: 'System.Windows.Media.Animation.DoubleAnimation'
cannot use default destination value of 'NaN'.
```

during template teardown (`Control.OnTemplateChanged`). Source inspection of
WPF-UI 4.3.0 (`Controls/NavigationView/NavigationViewCompact.xaml`) shows the
`PaneOpen` visual state animates `PaneGrid.Width` with
`To="{TemplateBinding OpenPaneLength}"`; re-templating re-evaluates that
binding to NaN. Upstream PR #1012 discussion and fix #1015 confirm the defect
class but fixed only the load path (`UpdateVisualState` early-return for
Top/Bottom/LeftFluent) — runtime Left→Top still crashes.

Mitigations proven insufficient by repeated UI runs (each 5–8 launches,
alternating pass/fail proving the crash, not test flake):

1. Close-pane-first + 500 ms settle before the swap — still crashed.
2. `GoToState(..., useTransitions: false)` + clearing held `Grid.Width`
   animations pre-swap — still crashed.
3. Recreating the `NavigationView` with bindings severed first — still
   crashed (checkpoint tracing proved the crash precedes the replacement:
   the binding-driven trigger fires during the originating transfer).

Launching directly into any persisted shell (Sidebar/Rail/Top) never
crashes — only the runtime swap does. Starts into Top/Tabbed were verified
by the passing legs of the same UI runs.

## Decision

- Shell selection persists immediately but applies at next launch, mirroring
  the established language-restart pattern (`Settings_LanguageNote` +
  `Restart now`). A `Settings_ShellNote` caption documents this next to the
  selector; the existing restart button is reused.
- `MainViewModel` applies the launch shell once (`_appliedShell`) and never
  re-applies from later settings updates; responsive collapse restores the
  applied shell.
- Scaffold-time `--Shell` selection (T08) remains the primary shell story:
  generated apps launch directly into the chosen chrome via the safe load
  path.

## Consequences

- No live shell hot-switch in 1.x on WPF-UI 4.3.0. Users get all four
  presets, stable and tested, with a documented restart step for runtime
  changes.
- If upstream fixes runtime `PaneDisplayMode` swaps (re-evaluate on every
  WPF-UI upgrade), this ADR can be revisited: the registry, shell enum, and
  template parameter already isolate the decision to `MainViewModel` +
  `SettingsPage`.

## Revisit triggers

WPF-UI ships a fixed NavigationView (verified by a hot-switch UI test, not
by release notes alone), or the shell host is re-implemented outside
`PaneDisplayMode` swaps (2.0 architecture).
