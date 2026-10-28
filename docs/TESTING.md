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

Runs: `dotnet --version` → restore → Release build →
`dotnet test -c Release` → app launch check (window appears, then closed).
(Release only since 1.4.0: no DEBUG-conditional code exists, and all required
gates build Release with warnings as errors.)

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

## Generators and template matrix

```powershell
# item-template gate: install, generate all 11 templates, residue check, build, tests
powershell -ExecutionPolicy Bypass -File eng/test-generators.ps1

# package-boundary gate: pack DevTemWpf.UI, restore a clean consumer from the
# local package plus NuGet, build, and launch the consumer
powershell -ExecutionPolicy Bypass -File eng/test-ui-package.ps1

# same gate against the packed .nupkg (required when the package changes)
powershell -ExecutionPolicy Bypass -File eng/test-generators.ps1 -PackagePath artifacts/DevTemWPF.Templates.1.8.0.nupkg

# full preset/module matrix from source or package
powershell -ExecutionPolicy Bypass -File eng/test-templates.ps1
powershell -ExecutionPolicy Bypass -File eng/test-templates.ps1 -PackagePath <nupkg>
```

Both scripts install into a throwaway `DOTNET_CLI_HOME`, so the developer's
global `dotnet new` registrations are never modified. `test-generators.ps1`
fails on any leftover template token (`DevTemWpf`, `Sample*`, undefined `App*`
styles), on a non-zero-warning build, or on a failing generated test.
`test-ui-package.ps1` verifies the experimental `DevTemWpf.UI` package from a
clean temporary consumer with no reference to the gallery app; it also checks
Comfortable/Compact token values and runtime density replacement.

## UI automation (FlaUI)

`tests/DevTemWpf.UiAutomation` drives the built app (launch/close,
menu-navigate + keyboard walk, dialog round-trip) via UIA3. Rules learned
the hard way:

- Use `InvokePattern` for activation, not coordinate clicks (scrolled
  content misses).
- Poll `HasExited` for exit; never read `ExitCode` off a FlaUI handle.
- Retry-with-timeout everywhere; STA via `[WpfFact]`.
- Headless environments: set `DEVTEM_RUN_UI_TESTS=0` (or non-interactive
  sessions skip automatically). Skips use `Assert.Skip`, so they are counted
  as `skipped` — never as passes. The required gate
  (`eng/check-ui-tests.ps1`, wired into CI) fails on any skip or on zero
  executed tests; a green required job always means the 4 ShellFlows cases
  really ran. Record the `UI_TESTS total=/failed=/skipped=` line in the
  version completion report.
- Data-binding errors involving our views fail the integration suite
  (`TestResources` listener) — silent XAML bugs become test failures.
- UI tests launch the app exe from `src/.../bin`; the test projects do NOT
  reference it, so always build the solution first (`dotnet build` /
  `dotnet test` at root rebuilds everything). Project-only test runs reuse
  the last app build.
- TextBlock UIA names: an explicit `AutomationProperties.Name` replaces the
  text content for `ByName` lookups (verified by probe).

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
