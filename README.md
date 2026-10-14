# DevTemWPF (working name)

Windows 11-first WPF application template + reusable UI foundation
(.NET 10, WPF-UI, CommunityToolkit.Mvvm, Generic Host).
Project identity is still to be finalized; `DevTemWpf` is the working name.

## Prerequisites

- Windows 11, .NET 10 SDK (`10.0.401`, pinned via `global.json`)

## Quick start

```powershell
dotnet restore
dotnet build -c Release
dotnet test -c Release
powershell -ExecutionPolicy Bypass -File eng/smoke.ps1
```

## Docs

- Product goals + milestones: `WPF-Template-Roadmap.md`
- Agent process contract: `WPF-AGENT-EXECUTION-PROTOCOL.md`
- Current plan/report: `docs/releases/0.1.0-plan.md`
- Testing: `docs/TESTING.md` · Performance: `docs/PERFORMANCE.md`
- Dependencies: `docs/DEPENDENCIES.md` · Decisions: `docs/decisions/`
- Scaffold contract: `docs/template/scaffold-contract.md`

## Status

`0.1.0` — repository foundation, architecture decisions, minimal runnable app.
