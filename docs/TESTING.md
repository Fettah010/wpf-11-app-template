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

## What is not covered yet

- UI automation (FlaUI), visual regression, packaging/installer tests:
  introduced with the milestones that ship the related surface
  (0.2.0+ shells, 0.7.0 distribution). See the version plan's test section.
- Manual checks per milestone (keyboard tab order, theme fallback, DPI)
  are recorded in the completion report with screenshots where useful.
