# ADR-0013: Beta validation gates (UI automation, binding guard, scope freeze)

- Status: Accepted
- Date: 2026-10-14
- Milestone: 0.9.0

## Context

Roadmap 0.9.0 needs feature-complete confidence: UI automation, keyboard
proof, binding-error detection, startup/idle hardening, full audits, and a
frozen 1.0 scope — on this machine's capabilities, stated honestly.

## Decision

- **FlaUI 5.0.0** (MIT, UIA3 backend) in `tests/DevTemWpf.UiAutomation`:
  launch/close, menu-navigate + keyboard-focus walk, dialog round-trip.
  Coordinate clicks proved unreliable for scrolled content — `InvokePattern`
  is the rule. Exit-code via second process handle proved unreliable —
  `HasExited` polling is the rule. Headless runs skip loudly via
  `DEVTEM_RUN_UI_TESTS`/`UserInteractive` (never fake-green; recorded).
- **Binding-error guard:** `DataBindingSource` listener in test setup fails
  on `DevTemWpf`-mentioned errors. Suite stays green = our XAML bindings
  resolve.
- **Settings write-avoidance:** `LoadAsync` reports repair-need; App
  persists only then. Same behavior, less IO.
- **Idle:** 0% CPU observed, set stable — published, no invented gate.
- **Clean-machine proof:** fresh clone + README smoke green (in addition to
  CI's fresh VMs).
- **Scope freeze:** 1.0-rc takes bug fixes + release mechanics only; all
  feature work is post-1.0 or explicit backlog (triaged DEFERRED.md).
- Zero product packages; one test-only addition.

## Evidence

- 81/81 tests (incl. 3 UI flows locally); matrix + smoke + fresh-clone
  green; vuln clean; audits complete.

## Consequences

- Low-spec numbers, HC eyeball, and full manual walk remain open with
  owners (D10, automation follow-ups).
- Template nupkg publishing is a 1.0 distribution task.

## Revisit triggers

FlaUI flakiness pattern (harden or scope down); 1.0-rc entry review.
