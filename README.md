# WPF 11 App Template

[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Windows 11](https://img.shields.io/badge/Windows-11-0078D4)](https://www.microsoft.com/windows/windows-11)
[![WPF-UI 4.3.0](https://img.shields.io/badge/WPF--UI-4.3.0-8A2BE2)](https://github.com/lepoco/wpfui)
[![License: MIT](https://img.shields.io/badge/License-MIT-green)](LICENSE)
[![Stable 1.0.0](https://img.shields.io/badge/release-1.0.0-green)](https://github.com/Fettah010/wpf-11-app-template/releases)

A premium, Windows 11-first **WPF application template** and reusable **UI
foundation** — Fluent design, MVVM, dependency injection, modular production
services, and `dotnet new` scaffolding with clean / recommended / full presets.

> **Status:** `1.0.0` — stable. Install the template, generate your app,
> and ship it: `dotnet new install DevTemWPF.Templates` (see
> [Roadmap](#roadmap--milestones) for what's next).
>
> ```powershell
> dotnet new install <repo>
> dotnet new devtemwpf-app -n MyApp --Preset Full
> ```

## ✨ Highlights

- 🪟 **Windows 11 first** — Fluent visuals, correct window behavior, DPI-aware
  layouts, Mica/acrylic-style materials (opt-in, with opaque fallbacks).
- 🧱 **Reusable UI foundation** — design tokens, shells (rail, sidebar, tabs,
  …), high-quality controls, and a coherent motion system.
- 📦 **Clean / Recommended / Full presets** — start minimal or
  feature-rich; optional modules (logging, SQLite, updates, tray, …) are
  removable, never forced.
- ⚡ **Performance as a feature** — measured baselines, `Auto`/`Balanced`/`Rich`
  effect profiles, reduced-motion and low-power fallbacks.
- 🤖 **Agent-friendly** — predictable structure, ADRs, repeatable CLI
  workflows, verifiable acceptance criteria per milestone.

## 🚀 Quick start

**Prerequisites:** Windows 11 · [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

```powershell
git clone https://github.com/Fettah010/wpf-11-app-template.git
cd wpf-11-app-template
dotnet restore
dotnet build -c Release
dotnet test -c Release
# full gate: build + tests + app launch check
powershell -ExecutionPolicy Bypass -File eng/smoke.ps1
```

## 🖼️ Screenshots

> Coming soon — the reference gallery app lands in `0.6.0`.
> Tracking: [Roadmap](WPF-Template-Roadmap.md) §8.

| Shell | Theme |
|---|---|
| _placeholder_ | _placeholder_ |

## 🗺️ Roadmap & milestones

| Version | Focus | Status |
|---|---|---|
| `0.1.0` | Foundation, ADRs, minimal app | ✅ Released |
| `0.2.0` | Window experience, nav rail + sidebar | ✅ Released |
| `0.3.0` | Design tokens & themes | ✅ Released |
| `0.4.0` | Essential controls & forms | ✅ Released |
| `0.5.0` | Motion system & optional effects | ✅ Released |
| `0.6.0` | Layouts, data controls, dashboard & gallery | ✅ Released |
| `0.7.0` | Production services & distribution | ✅ Released |
| `0.8.0` | `dotnet new` scaffolding & generators | ✅ Released |
| `0.9.0` | Beta hardening | ✅ Released |
| `1.0.0-rc.1` | API freeze & candidate validation | ✅ Released |
| `1.0.0` | Stable template and UI foundation | ✅ Released |
| `1.0.0` | Stable template + UI foundation | ⬜ |

Full product plan: [`WPF-Template-Roadmap.md`](WPF-Template-Roadmap.md) ·
per-version plans/reports: [`docs/releases/`](docs/releases/) ·
decisions: [`docs/decisions/`](docs/decisions/).

## 🛠️ Tech stack

| Concern | Choice |
|---|---|
| UI toolkit | [WPF-UI 4.3.0](https://github.com/lepoco/wpfui) (MIT) |
| MVVM | [CommunityToolkit.Mvvm 8.4.2](https://github.com/CommunityToolkit/dotnet) |
| DI / hosting | Microsoft.Extensions.Hosting 10.0.12 |
| Tests | xUnit.net v3 (Microsoft Testing Platform mode) |
| Updates / distribution | Velopack + MSIX paths (from `0.7.0`) |

Dependency inventory & licenses: [`docs/DEPENDENCIES.md`](docs/DEPENDENCIES.md)
· [`NOTICE-THIRD-PARTY.md`](NOTICE-THIRD-PARTY.md).

## 📁 Project structure

```text
src/DevTemWpf.App/   # reference app (embryo of the clean preset)
tests/               # unit + integration (xUnit v3, MTP mode)
docs/                # decisions, guides, per-release plans/reports
eng/                 # smoke.ps1 gate, measure-startup.ps1 baselines
```

Conventions: central package management, pinned SDK (`global.json`),
analyzers + formatting enforced, zero-warning builds.

## 🤝 Contributing

Issues and PRs are welcome. Please keep the current milestone's scope
(`docs/releases/`), add tests with behavior changes, and update docs in the
same change. See `WPF-AGENT-EXECUTION-PROTOCOL.md` for the working contract.

## 📄 License

MIT — see [LICENSE](LICENSE). Third-party notices:
[NOTICE-THIRD-PARTY.md](NOTICE-THIRD-PARTY.md).
