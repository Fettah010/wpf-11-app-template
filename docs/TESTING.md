# Testing

Canonical verification commands for this repository (Windows 11 + .NET 10 SDK).

`dotnet test` runs in **Microsoft Testing Platform (MTP) mode**, enabled via
`global.json` (`test.runner = Microsoft.Testing.Platform`). This is required:
VSTest-based runs are blocked for MTP projects on the .NET 10 SDK
(see `docs/decisions/ADR-0004-test-and-perf-methodology.md`).

## Full gate (same as CI-equivalent local check)

```powershell
powershell -ExecutionPolicy Bypass -File eng/smoke.ps1
```

Runs: `dotnet --version` → restore → Debug build → Release build →
`dotnet test -c Release` → app launch check (window appears, then closed).

## Individual commands

```powershell
dotnet --version
dotnet restore
dotnet build -c Debug
dotnet build -c Release
dotnet test -c Release
dotnet list package --include-transitive
```

Expect **0 warnings** on both build configurations. Any warning must be
triaged in the version completion report — never silenced without a record.

## UI automation (FlaUI)

`tests/DevTemWpf.UiAutomation` drives the built app (launch/close,
menu-navigate + keyboard walk, dialog round-trip) via UIA3. Rules learned
the hard way:

- Use `InvokePattern` for activation, not coordinate clicks (scrolled
  content misses).
- Poll `HasExited` for exit; never read `ExitCode` off a FlaUI handle.
- Retry-with-timeout everywhere; STA via `[WpfFact]`.
- Headless environments: set `DEVTEM_RUN_UI_TESTS=0` (or non-interactive
  sessions skip automatically). Skips print loudly and are recorded in the
  release report — never fake-green.
- Data-binding errors involving our views fail the integration suite
  (`TestResources` listener) — silent XAML bugs become test failures.

## STA tests

Tests that construct WPF controls (`NavigationViewItem`, pages) use
`[WpfFact]`/`[WpfTheory]` from `Xunit.StaFact` (STA + Dispatcher) instead of
`[Fact]`. Pure-logic tests stay on `[Fact]`. Fixture/test-class constructors
run on worker threads — construct UI objects inside the test method.

## What is not covered yet

- UI automation (FlaUI), visual regression, packaging/installer tests:
  introduced with the milestones that ship the related surface
  (0.2.0+ shells, 0.7.0 distribution). See the version plan's test section.
- Manual checks per milestone (keyboard tab order, theme fallback, DPI)
  are recorded in the completion report with screenshots where useful.
