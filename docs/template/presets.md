# Template presets & flags

Install from the repo (or a future template package):

```powershell
dotnet new install <path-to-repo>
dotnet new devtemwpf-app -n MyApp --Preset Full
```

## Presets

| Preset | Dashboard + charts | Diagnostics UI | Production modules | Tests |
|---|---|---|---|---|
| Clean | no | no | none | optional |
| Recommended (default) | no | yes | logging, local data, HTTP, backup, updates, tray | optional |
| Full | yes | yes | logging, local data, HTTP, backup, updates, tray | optional |
| Custom | no | if `--CustomDiagnostics` | only the `--Custom<Module>` flags you set | optional |

In **Custom**, each production module is selected with a `bool` flag.
`Diagnostics` implies `logging`, `http`, `backup`, and `updates`; `backup`
implies `localdata` (it protects the local database). The named presets are
unaffected by the `Custom<Module>` flags.

Module map: dashboard files + LiveCharts + sample data; diagnostics page + VM;
per-module service files + packages + tray/logging/update hooks.
Localization, tokens, motion, and feedback stay in every preset.

## Flags

| Flag | Values | Default | Notes |
|---|---|---|---|
| `--Preset` | Clean/Recommended/Full/Custom | Recommended | Drives module inclusion (computed symbols). |
| `--CustomLogging` | true/false | false | Custom only: Serilog file logging. |
| `--CustomLocalData` | true/false | false | Custom only: SQLite local data. |
| `--CustomHttp` | true/false | false | Custom only: typed HTTP client. |
| `--CustomBackup` | true/false | false | Custom only: backup/restore (implies local data). |
| `--CustomUpdates` | true/false | false | Custom only: Velopack update engine. |
| `--CustomTray` | true/false | false | Custom only: system tray icon. |
| `--CustomDiagnostics` | true/false | false | Custom only: Diagnostics page (implies logging, HTTP, backup, updates). |
| `--IncludeTests` | true/false | true | Excludes `tests/**` + solution entries when false. |
| `-n/--name` | text | directory name | Renames namespaces, files (`DevTemWpf.slnx`), and content via `sourceName`. |

Shell (rail/sidebar) and theme (system/light/dark) are **runtime** choices
(Settings page, persisted) — intentionally not scaffold flags. A first
attempt at template-time defaults was cut: the engine substitutes choice
*keys*, ignoring custom `value`s, and multi-line `#if` initializers are
invalid C# in repo builds (verified 2026-10-14).

## Generated app contents

`src/**`, `tests/**` (unless off), `eng/smoke.ps1` + `measure-startup.ps1`,
`global.json`, props, editorconfig, gitignore/attributes, LICENSE,
`appsettings.json`, generated `README.md` + `FEATURES.md` (module manifest).

Excluded: `docs/**`, roadmap/protocol/CHANGELOG, `.github/**` (generated
apps start CI-free), `templates/**`, `bin/obj`, `eng/test-templates.ps1`.

Notes: `Wpf11AppTemplate` AppData folder names are intentionally stable
(rename-safe). `DevTemWpf` must not appear anywhere in output (matrix asserts).
