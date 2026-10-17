# Updates, channels, and rollback

The update flow is explicit-only: the app never checks or downloads on its
own. Every action is user-initiated and every failure becomes status text,
never a crash. Availability lives in the Diagnostics page.

## States

`Disabled` → `Idle` → `Checking` → (`Available` | `UpToDate`) → `Downloading`
→ `ReadyToRestart` → restart applies. `Failed` carries a message; `Retry`
re-runs the last operation.

## Configuration

Feed URLs live in `appsettings.json`:

```json
"Update": { "StableFeedUrl": "https://…/stable", "BetaFeedUrl": "https://…/beta" }
```

- Empty feed ⇒ `Disabled` (a portable/uninstalled build with no updater).
- `Stable`/`Beta` is a user setting (`ShellSettings.UpdateChannel`) and selects
  which URL is used.
- Checks are safe when not installed: `VelopackApp.Build().Run()` is a no-op and
  checks report `Disabled`/`Failed` rather than throwing.

## What the surface shows (1.6.0)

- Installed vs available version.
- Delta vs full — Velopack decides; the app only surfaces the decision
  (deltas fall back to full automatically).
- `Retry`, `Cancel` (cancels the in-flight check/download), and an explicit
  **Restart now** prompt after a download (downloads no longer auto-restart).
- **Downgrade** is opt-in: an "Allow older version" switch asks for
  confirmation before switching to a version *older* than the installed one.

## Rollback / recovery

- A previous full release can always be applied by enabling the downgrade
  switch and checking the feed that still serves it.
- If a feed is corrupt, the check maps to `Failed` with a message; fix the feed
  and `Retry`.
- Manual recovery is a fresh install of the prior Velopack package.

## Distribution

Portable, Velopack, and MSIX/Store profiles are kept separate — see
`docs/release/packaging.md`. Feed-based updating must not be combined with a
Store/MSIX build (leave feeds empty there).
