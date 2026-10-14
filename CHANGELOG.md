# Changelog

## 0.2.0 (2026-10-14)

- Application shell: `FluentWindow` + `TitleBar` + adaptive `NavigationView`
  with rail (`LeftMinimal`) and sidebar (`Left`) modes, responsive collapse,
  back navigation, `BreadcrumbBar`, Home + Settings pages.
- Services: JSON settings persistence (`%AppData%`), system/light/dark theme
  service, Mica backdrop service with opaque fallback (ctor-time DP rule).
- Tests: 16 passing (settings/theme/backdrop/shell units incl. `WpfFact` STA
  coverage, extended host integration test).
- New packages: `WPF-UI.DependencyInjection 4.3.0` (MIT, same vendor —
  v4 page provider), `Xunit.StaFact 4.0.24` (MS-PL, test-only).
- ADRs 0005–0006; startup 0.71s / working set 130.4 MB (vs 0.61s / 116.3 MB).

## 0.1.0 (2026-10-14)

- Repository foundation: `global.json` SDK pin, central package management,
  analyzers/formatting conventions, solution skeleton.
- Minimal runnable WPF app on `net10.0-windows10.0.26100` with Generic Host,
  typed options, MVVM ViewModel, and WPF-UI 4.3.0 theme prototype.
- xUnit.net v3 (MTP mode) test projects: ViewModel unit tests + host
  lifecycle integration test (3 passing).
- `eng/smoke.ps1` gate and `eng/measure-startup.ps1` baseline tooling.
- ADRs 0001–0004, dependency/license inventory, scaffold contract,
  first performance baseline.
