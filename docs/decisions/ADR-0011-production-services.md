# ADR-0011: Production services and distribution

- Status: Accepted
- Date: 2026-10-14
- Milestone: 0.7.0

## Context

Roadmap 0.7.0 needs production infrastructure as optional, removable
modules: logging, SQLite, HTTP, backup, updates, tray, localization, CI,
and distribution docs.

## Decision

- **Logging:** Serilog 4.4.0 + Hosting 10.0.0 + File 7.0.0 (Apache-2.0),
  rolling daily, 14-day retention, app composes it (never tests).
- **SQLite:** Microsoft.Data.Sqlite 10.0.13 (MIT, verified in nuspec); `FavoritesStore`
  with `user_version` migrations, per-operation connections, pool clearing
  before backup, recreate-on-corrupt.
- **HTTP:** `AddHttpClient` typed `StatusClient` (Microsoft.Extensions.Http
  10.0.12); empty URL = disabled; mapped errors; fake-handler tests.
- **Backup:** BCL zip of settings + db (+wal/shm); presence-validated
  restore; restart recommended.
- **Updates:** Velopack 1.2.161 (MIT, net10 asset); explicit-only flow
  (Disabled→…→Failed states, fake-tested); `VelopackApp.Build().Run()` at
  startup (uninstalled-safe); channels = feed selection.
- **Tray:** WPF-UI.Tray 4.3.0 (MIT); icon only while enabled; runtime glyph;
  Open/Exit menu; unregister on close.
- **Localization:** resx EN + de proof, hand-written accessor (CLI-safe),
  culture setting, restart apply; gallery copy explicitly remaining.
- **CI:** `ci.yml` (windows-latest, pinned SDK, build `--warnaserror`,
  test, package audit).
- **Distribution:** portable/installed/MSIX profiles + signing guidance +
  version rules in `docs/release/packaging.md`.
- Deferred with reasons: auth/reporting (security review), system toasts
  (MSIX-gated), full-gallery l10n (mechanical follow-up).

## Evidence

- 77/77 tests; launches (default + motion-off states); log file on disk;
  settings/db backup round-trips; vuln scan clean.
- Pooled-connection file lock found by the backup test → `ClearAllPools`
  in product code (test-earned fix).

## Consequences

- Six packages, each inert without configuration; 0.8.0 template drops
  matching sets. No telemetry anywhere.

## Revisit triggers

Backend arrival (HTTP client gains real endpoints); Store submission
(toast/MSIX activation); auth need with security review.
