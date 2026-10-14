# ADR-0002: CommunityToolkit.Mvvm + Generic Host application architecture

- Status: Accepted
- Date: 2026-10-14
- Milestone: 0.1.0

## Context

The roadmap fixes MVVM on CommunityToolkit.Mvvm and DI on
Microsoft.Extensions.Hosting abstractions, keeping the clean preset lean.

## Options considered

1. **CommunityToolkit.Mvvm (source generators) + Generic Host** — roadmap default.
2. **Competing MVVM frameworks (Prism, ReactiveUI, MVVM Light)** — heavier
   app-framework lock-in, rejected without a gap case.
3. **Hand-rolled MVVM / no DI** — less code up front, but no typed config,
   logging, or service lifetime story for later milestones.

## Decision

- `CommunityToolkit.Mvvm 8.4.2` (latest stable 2026-03-25; targets
  `net8.0`/`netstandard2.0`, net10-compatible).
- `Microsoft.Extensions.Hosting 10.0.12` (latest stable 10.x patch
  2026-09-08, MIT; v11 is RC — rejected per stable-only policy).
- Single shared composition helper `AppComposition.ConfigureForApp`
  (`src/DevTemWpf.App/AppComposition.cs`) used by both `App` and integration
  tests, so tests exercise the real registrations.
- Code-behind holds window lifecycle + host wiring only; state/commands live
  in ViewModels/services.

## Consequences

- `MainViewModel` uses `[ObservableProperty]`/`[RelayCommand]`; proven by
  `tests/DevTemWpf.Unit` (2 tests green).
- Host start/stop/dispose proven by `tests/DevTemWpf.Integration`.
- Transitive Hosting graph (~25 `Microsoft.Extensions.*` 10.0.12 packages)
  is accepted and pinned via CPM (see `docs/DEPENDENCIES.md`).

## Revisit triggers

A milestone demonstrates a concrete hosting cost problem for the clean preset
(e.g. startup budget pressure) — then split host/no-host flavors per the
scaffold contract instead of abandoning DI.
