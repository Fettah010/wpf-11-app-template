# Dependencies (0.4.0 inventory, verified 2026-10-14)

No package changes in 0.4.0 (feedback services + validator ship in approved
packages). 0.2.0 inventory below still current.

Single version source: `Directory.Packages.props` (CPM + transitive pinning).

## Direct dependencies

| Package | Version | Purpose | Upstream | License | TFM fit | Maintenance |
|---|---|---|---|---|---|---|
| `WPF-UI` | 4.3.0 | Primary Fluent UI foundation (ADR-0001) | `github.com/lepoco/wpfui` | MIT (`LICENSE` on main) | Ships `net10.0-windows7.0`; consumer `net10.0-windows10.0.26100` builds 0 warnings | Latest stable 2026-05-04; older 4.x deprecated (critical bugs) |
| `CommunityToolkit.Mvvm` | 8.4.2 | MVVM source generators (ADR-0002) | `github.com/CommunityToolkit/dotnet` | MIT | `net8.0`/`netstandard2.0` → net10 compatible | Stable 2026-03-25 |
| `Microsoft.Extensions.Hosting` | 10.0.12 | Generic Host + DI/config/logging (ADR-0002) | `github.com/dotnet/dotnet` | MIT | `net10.0` compatible | Stable 2026-09-08; v11 is RC (rejected) |
| `xunit.v3.mtp-v2` | 4.0.2 | Test framework, MTP runner (ADR-0004) | `github.com/xunit/xunit` | Apache-2.0 / MIT (xUnit family; see NOTICE) | `net8.0` → net10 compatible | Stable 2026-10-09 |
| `WPF-UI.DependencyInjection` | 4.3.0 | DI page provider for NavigationView (ADR-0005; v4 has no `IPageService` impl) | `github.com/lepoco/wpfui` | MIT (same repo) | Ships `net10.0`; deps only DI.Abstractions + Abstractions | Stable 2026-05-04, same 4.3.0 line |
| `Xunit.StaFact` | 4.0.24 | STA/Dispatcher test attributes, test-only (ADR-0004 update) | `github.com/AArnott/Xunit.StaFact` | MS-PL (test-only, never shipped) | `net8.0` → net10 compatible | Stable 2026-10-01; used by Avalonia/Roslyn/WPF repos |

## Notable transitive dependencies

- `WPF-UI.Abstractions 4.3.0` (MIT, same repo) — sole WPF-UI runtime dep.
- `Microsoft.Extensions.* 10.0.12` graph (~25 packages, MIT) — Hosting's
  config/DI/logging/options closure. Accepted: no smaller stable host exists.
- `Microsoft.Testing.Platform 2.5.1` + extensions (MIT) — MTP runner closure.
- `xunit.v3.* 4.0.2` + `xunit.analyzers 2.2.0` — framework + analyzers
  (one analyzer warning found and fixed during 0.1.0: xUnit1051).
- `Microsoft.ApplicationInsights 2.23.0` — pulled by MTP telemetry extension,
  **test projects only**, never shipped in the app. Revisit if it leaks into
  app output or raises telemetry concerns.

## Deferred candidates (not referenced)

| Package | Future candidate | First need |
|---|---|---|
| `Microsoft.Xaml.Behaviors.Wpf` | 1.1.161 (2026-09-15; roadmap snapshot 1.1.142 is stale) | 0.4.0/0.5.0 |
| Serilog (+ hosting/file sinks) | fresh audit | 0.7.0 |
| `Microsoft.Data.Sqlite` | fresh audit | 0.7.0 |
| Velopack | fresh audit (license/compat) | 0.7.0 |
| LiveCharts2 | fresh audit | 0.6.0 (optional module) |
| FlaUI | fresh audit (health/reliability) | 0.7.0–0.9.0 |

No overlapping UI suite (ModernWPF/MahApps/Material) is referenced.
Any addition requires PoC + conflict review + license check + ADR.
