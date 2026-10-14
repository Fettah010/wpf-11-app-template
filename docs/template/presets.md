# Template presets & flags

Install from the repo (or a future template package):

```powershell
dotnet new install <path-to-repo>
dotnet new devtemwpf-app -n MyApp --Preset Full
```

## Presets

| Preset | Dashboard + charts | Diagnostics UI | Production services | Tests |
|---|---|---|---|---|
| Clean | no | no | no | optional |
| Recommended (default) | no | yes | yes | optional |
| Full | yes | yes | yes | optional |

Module map: dashboard files + LiveCharts + sample data + favorites;
diagnostics page + VM; service files + packages + tray/logging/update hooks.
Localization, tokens, motion, and feedback stay in every preset.

## Flags

| Flag | Values | Default | Notes |
|---|---|---|---|
| `--Preset` | Clean/Recommended/Full | Recommended | Drives module inclusion (computed symbols). |
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
