# ADR-0014 — Explicit resx culture override

- Status: Accepted (1.3.0)
- Date: 2026-10-15
- Context: gallery localization (D16), `Strings` resx system, WPF `x:Static` bindings

## Context

Gallery display copy moved to resx with `AutomationProperties.Name` kept
invariant. End-to-end verification under German settings showed a
mixed-language UI: nav rail and VM-bound strings rendered German while
`x:Static`-bound page content rendered English — in the same process.

Instrumentation (temporary, reverted) logged, for one capture run:

```text
MainViewModel thread=2 ui=de Nav_Home=Start
ctor thread=2 ui=en-US Ctl_Title=Controls
```

Page construction observed `CurrentUICulture=en-US` although the startup
path had applied German. `CultureService.ApplyCulture` ran after
`Host.StartAsync`, so any thread created before it kept its birth culture;
`DefaultThreadCurrentUICulture` does not retro-fix existing threads, and
`async` continuations do not guarantee which thread resumes.

## Decision

1. `Strings.OverrideCulture`: resx lookup prefers an explicitly-set culture
   over ambient thread culture. Set once at startup from persisted settings.
2. `CultureService.ApplyCulture` sets all four slots
   (`DefaultThreadCurrentCulture/UICulture` + current thread both).
3. Culture is applied before the host builds (direct `SettingsService`
   read, no host needed) and re-asserted on the dispatcher thread before
   `MainWindow` creation.
4. Language changes still require a restart (documented behavior).

## Consequences

- Lookups are deterministic regardless of which thread constructs pages;
  verified by full German UI screenshots.
- Unit tests set `OverrideCulture`/explicit cultures instead of depending
  on ambient thread state.
- A stale `OverrideCulture` would pin the wrong language; it is only ever
  set from persisted settings at startup.

## Alternatives rejected

- Page-constructor re-assertion in all 8 pages: scattered, easy to forget
  on new pages.
- Custom markup extension replacing `x:Static`: larger churn for the same
  guarantee; revisit if per-page live language switching is ever required.

## Revisit triggers

- Live (restart-free) language switching.
- WinUI/WPF framework changes to resource fallback.
