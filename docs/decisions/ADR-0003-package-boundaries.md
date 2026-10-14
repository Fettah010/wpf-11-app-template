# ADR-0003: Package boundaries, TFM, and OS floor

- Status: Accepted
- Date: 2026-10-14
- Milestone: 0.1.0

## Context

The roadmap sketches `UI / Windows / App / Services` assemblies but warns
that each assembly must be a real dependency boundary — not one assembly per
feature.

## Decision

- **0.1.0 ships one app only** (`src/DevTemWpf.App`). No `*.UI`, `*.Windows`,
  or `*.Services` libraries yet: creating them now would be speculative
  abstraction with a single consumer.
- **TFM: `net10.0-windows10.0.26100`** with
  `SupportedOSPlatformVersion 10.0.26100.0` for all projects (app + tests, so
  test assemblies can reference the app). This expresses the Windows 11-only
  product rule in the build.
- **Central Package Management** (`Directory.Packages.props`,
  `CentralPackageTransitivePinningEnabled`) is the single version source.
- **SDK pin** (`global.json`): `10.0.401` + `rollForward: latestFeature`.
- Future split (0.2.0+): UI tokens/controls → UI library; chrome/backdrop/
  tray/notifications → Windows layer; SQLite/HTTP/backup → Services. Each
  split gets an ADR update when its second consumer exists.

## Evidence

- `net10.0-windows10.0.26100` builds on SDK 10.0.401 with 0 warnings and the
  WPF-UI `net10.0-windows7.0` asset resolves (this milestone's builds).
- Fallback if reference-pack friction appears on other machines: retarget to
  `net10.0-windows` + `SupportedOSPlatformVersion` (recorded, not implemented).

## Revisit triggers

Second consumer for shared XAML/services, or TFM friction on clean machines.
