# Changelog

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
