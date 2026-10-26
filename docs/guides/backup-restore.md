# Backup and restore

`BackupService` (module `--CustomBackup`, implies local data) produces a BCL
zip of the user state and restores it safely.

## What it captures

`settings.json`, `app.db` (plus `-wal`/`-shm` when present), and
`versions.txt` (schema/version marker). Backups are written atomically
(temp file + move) to `%LOCALAPPDATA%\Wpf11AppTemplate\backups`.

## Failure boundaries (all return a typed result, never a raw exception)

| Case | Behaviour |
|---|---|
| Missing/corrupt member | Rejected with a specific `BackupFailure`. |
| `settings.json` absent | Rejected (not a valid backup). |
| Unsupported/forward `versions.txt` | Rejected before extracting. |
| Oversized input / too many entries | Rejected by entry-count and size caps. |
| Path traversal / absolute entry names | Rejected; extraction stays inside the target. |
| Duplicate state | Handled deterministically. |
| Interrupted write | No partial backup: the temp file is discarded. |
| Restore to a new location | Supported and tested. |

## Usage

From the Diagnostics page: **Create backup**, then **Restore** (picks a
`.zip`). Programmatically, inject `IBackupService` and call its backup/restore
methods; failures surface as `BackupException` carrying a `BackupFailure`.

## Restore safety

Restore validates every entry before writing, extracts to a temp location, and
atomically replaces the target so a failed restore cannot leave a half-written
database. Keep a backup of the current state before a large schema change.
