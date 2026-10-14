# ADR-0010: Dashboard data layer and chart module

- Status: Accepted
- Date: 2026-10-14
- Milestone: 0.6.0

## Context

Roadmap 0.6.0 needs data UI (table, sorting/filtering, master/detail,
stepper, charts) on sample data, isolated for template exclusion — without
a backend, a grid vendor, or unstyled controls.

## Decision

- `SampleDataService` singleton: seeded (`Random(42)`) metrics, 1500
  activities, 7-day series. Deterministic, asset-free, count-tested.
- Table = `ListView` + `GridView` (WPF-UI has ListView brushes; its DataGrid
  section is an explicit TODO = unstyled). Sort via `SortDescriptions`
  toggle, filter via `ICollectionView`, virtualization asserted in test.
  Header sort buttons bind through `ElementName` (standard practice).
- Charts = `LiveChartsCore.SkiaSharpView.WPF 2.0.5` (MIT, stable 2026-06-18,
  net8.0-windows asset, SkiaSharp 3.119 natives, 5.5k stars, real WPF
  consumers). App-only, Dashboard-isolated; VM-built `LineSeries`; theming
  follows later (limitation).
- Master/detail + compact stepper as page patterns; full top-nav/tabbed
  shells deferred to 0.8.0 template surface (no in-app consumer justifies
  them in 0.6.0); TreeView/DataGrid/DnD recorded as gaps.
- Exclusion contract for 0.8.0: dashboard file set + registrations + nav
  item + chart package ref drop as one unit (scaffold-contract.md).

## Evidence

- 56/56 tests (determinism, filter/sort/selection/stepper, virtualization
  assert, page guards); launches; 0.77s / 132.2 MB (budgets hold); vuln scan
  clean.
- RelayCommand naming verified the hard way: `SortBy` method →
  `SortByCommand` property (XAML fixed to match).

## Consequences

- Dashboard resolves on navigation; startup unaffected (measured).
- Chart uses library-default theme until the theming pass.

## Revisit triggers

Backend arrival (0.7.0 data module replaces samples behind the same VM
shape); chart theming; shell contracts in 0.8.0.
