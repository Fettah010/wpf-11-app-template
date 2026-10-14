# ADR-0004: Test runner and performance methodology

- Status: Accepted
- Date: 2026-10-14
- Milestone: 0.1.0

## Context

Roadmap requires unit, integration, template-smoke, UI, accessibility,
packaging, and performance checks by 1.0, with reproducible baselines from
0.1.0. The test runner choice affects every later milestone.

## Decision: xUnit.net v3 on Microsoft Testing Platform (MTP)

- Test projects are **standalone executables**: `<OutputType>Exe</OutputType>`
  + `xunit.v3.mtp-v2 4.0.2` (stable 2026-10-09).
- `dotnet test` runs in **MTP mode** via `global.json`
  (`test.runner = Microsoft.Testing.Platform`). Required: VSTest-based runs
  are an error for MTP projects on the .NET 10 SDK
  (`learn.microsoft.com/dotnet/core/testing/unit-testing-with-dotnet-test`,
  updated 2026-09-12; verified by reproducing the MSBuild error, then the
  green run after migration).
- Rejected alternative: VSTest path (`Microsoft.NET.Test.Sdk` +
  `xunit.runner.visualstudio`) — errors on .NET 10 SDK for MTP-based
  projects; legacy mode slated for removal in MTP v2.
- Canonical gate: `eng/smoke.ps1` (restore → Debug → Release → test →
  launch check). Canonical commands: `docs/TESTING.md`.

## Decision: performance methodology

- Metrics: cold start (process launch → main-window handle) + working set
  after 5s idle, via `eng/measure-startup.ps1` (5 runs, median + max).
- Navigation/scroll/animation metrics are N/A until the surface exists —
  stated in each report, never silently omitted.
- Budgets stay **provisional** until ratified ≤0.4.0 per roadmap.
- Reference hardware is recorded per measurement (`docs/PERFORMANCE.md`);
  a low-spec reference environment is required from 0.2.0 onward.

## Evidence

- `dotnet test -c Release`: 3/3 passing (MTP mode, 2026-10-14).
- First baseline: cold-start median 0.61s / max 0.69s; working set median
  116.3 MB (warm-cache, Ryzen 7 8700G, 32 GB, Win11 b26300).

## Revisit triggers

MTP/VSTest bridge changes, or a milestone needing UI-automation runners
(FlaUI audit in 0.7.0–0.9.0 window).
