# Dependencies (0.9.0 inventory, verified 2026-10-14)

Two test-only additions (rows). Prior inventories otherwise current.
Nothing was removable in the 0.9.0 review: every direct package has a live
consumer (audited below); transitive notes follow.

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
| `LiveChartsCore.SkiaSharpView.WPF` | 2.0.5 | Dashboard charting, app-only/isolated (ADR-0010) | `github.com/Live-Charts/LiveCharts2` | MIT | `net8.0-windows` → net10 compatible; SkiaSharp 3.119 natives (win-x64 covered) | Stable 2026-06-18; 2.0.0 stable since 3/2026; 5.5k stars |
| `Serilog` | 4.4.0 | Structured logging core (ADR-0011) | `github.com/serilog/serilog` | Apache-2.0 | net10 compatible | Stable |
| `Serilog.Extensions.Hosting` | 10.0.0 | Serilog ↔ Generic Host bridge | `github.com/serilog/serilog-extensions-hosting` | Apache-2.0 | Matches Hosting 10.x | Stable |
| `Serilog.Sinks.File` | 7.0.0 | Rolling file sink (14-day retention) | `github.com/serilog/serilog-sinks-file` | Apache-2.0 | net10 compatible | Stable |
| `Microsoft.Data.Sqlite` | 10.0.13 | Local database (favorites store) | `github.com/dotnet/efcore` | MIT (verified in nuspec) | net10; native SQLite win-x64 bundled | Stable 10.0.x band |
| `Microsoft.Extensions.Http` | 10.0.12 | Typed `HttpClient` factory | `github.com/dotnet/runtime` | MIT | Matches Hosting 10.x | Same release train |
| `Velopack` | 1.2.161 | Installer/update engine (explicit flow) | `github.com/velopack/velopack` | MIT | `net10.0` asset, zero net10 deps | Stable 2026-09-29; heavy WPF adoption |
| `WPF-UI.Tray` | 4.3.0 | Tray icon + menu (same vendor) | `github.com/lepoco/wpfui` | MIT (same repo) | `net10.0-windows7.0` asset | Stable 2026-05-04, same 4.3.0 line |
| `FlaUI.Core` | 5.0.0 | UI automation driver, test-only (ADR-0013) | `github.com/FlaUI/FlaUI` | MIT (verified in nupkg LICENSE.txt) | `net8.0-windows` → net10 compatible | Stable; version parity with UIA3 |
| `FlaUI.UIA3` | 5.0.0 | UIA3 automation backend, test-only (ADR-0013) | `github.com/FlaUI/FlaUI` | MIT (same repo) | Inbox Windows UIA, no driver install | Stable |

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
- `SkiaSharp.* 3.119` (+ HarfBuzz, Views.WPF) — chart rendering + win-x64
  natives, app-only via LiveCharts. Accepted: isolated module.
- `System.Drawing.Common` (via Tray on older TFMs; inbox on net10-windows) —
  no action.

## 0.9.0 removal review

Every direct package has a live consumer; nothing removed: WPF-UI (+DI,
+Tray) shells; Mvvm all VMs; Hosting/Config/Options/Binder DI graph; Http
factory; Serilog×3 logging; Sqlite favorites; Velopack updates; LiveCharts
dashboard; xunit/MTP/StaFact/FlaUI test tiers. No duplicate UI suites, no
telemetry packages, no secrets in tree (grep-verified), no bin/obj tracked.

## Deferred candidates (not referenced)

| Package | Future candidate | First need |
|---|---|---|
| `Microsoft.Xaml.Behaviors.Wpf` | 1.1.161 (2026-09-15; roadmap snapshot 1.1.142 is stale) | only if a concrete behavior need appears (none through 0.9.0) |

No overlapping UI suite (ModernWPF/MahApps/Material) is referenced.
Any addition requires PoC + conflict review + license check + ADR.
