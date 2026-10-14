# ADR-0008: Core controls via stock WPF-UI + feedback services

- Status: Accepted
- Date: 2026-10-14
- Milestone: 0.4.0

## Context

Roadmap 0.4.0 needs everyday controls, validation, busy/disabled states, and
feedback patterns — without forking controls WPF-UI already provides well.

## Decision

- **Zero custom controls.** Gallery composes stock WPF-UI + styled natives
  (`ToggleButton`, `Expander`, `ProgressBar` have no `ui:` prefix — they are
  WPF controls wearing the WPF-UI theme; verified by compiler errors, fixed).
- **Feedback services:** `SnackbarService` + `ContentDialogService`
  singletons (main package); `SnackbarPresenter` + `ContentDialogHost`
  overlays attached in MainWindow ctor. VMs call interfaces only → fakes in
  tests. Current API verified against 4.3.0 sources (`SetDialogHost` overloads,
  obsolete `SetContentPresenter`, `ShowSimpleDialogAsync` extension).
- **Validation:** `ObservableValidator` + DataAnnotations; explicit per-field
  error strings (binding-engine-agnostic, testable); Save validates all.
- **Busy:** single-flight `IAsyncRelayCommand`
  (`AllowConcurrentExecutions=false`) + `IsBusy`/`IsIdle`. Observed under the
  MTP runner: a command-supplied `CancellationToken` arrives pre-cancelled,
  so the demo takes no token (documented; dialog path forwards its token to a
  fake harmlessly in tests and works live).
- **Budgets ratified** (modern HW, warm-cache, Release): start median ≤1.0s;
  working set median ≤150 MB; 0 warnings; 100% tests.

## Evidence

- 34/34 tests (validation, fake-based commands, page-XAML guards, graph
  resolution); launches in default + light states; 0.68s / 131.7 MB.
- `ui:ToggleButton`/`ui:Expander`/`ui:ProgressBar` guesses corrected by the
  compiler (recorded as the value of build-verified iteration).

## Consequences

- Gap notes (chips, skeleton, grid, TimePicker sample) carry to 0.6.0.
- Low-spec budget numbers still outstanding (D10).

## Revisit triggers

A proven control gap, or a custom control need from the dashboard (0.6.0).
