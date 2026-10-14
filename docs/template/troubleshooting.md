# Template troubleshooting

## `dotnet new install <repo>` finds no templates

The folder must contain `.template.config/template.json` at its root.
Verify with `dotnet new list devtemwpf` — all three templates
(`devtemwpf-app`, `devtemwpf-page`, `devtemwpf-service`) should appear.
Item templates install from their own folders (see generators.md).

## Stale output after editing the template

Uninstall and reinstall — the engine snapshots at install time:

```powershell
dotnet new uninstall <repo>
dotnet new install <repo>
```

`eng/test-templates.ps1` does this automatically per run.

## Generation fails with exit 103

Template not installed (or uninstalled by a previous matrix run's cleanup).
Reinstall first.

## Generated app build fails

1. Check `FEATURES.md` matches the intended preset.
2. Confirm no `DevTemWpf` residue: `Get-ChildItem -Recurse -Filter *DevTemWpf*`
   and content grep — the matrix asserts both.
3. Confirm no `#if (` / `#endif` residue in `src/`.
4. `dotnet --version` must be 10.0.x (pinned band in `global.json`).

## Shell/theme defaults

There are no `--Shell`/`--ThemeDefault` flags by design (verified engine
limitation, see presets.md). Switch at runtime in Settings; it persists.

## Rebrand / rename

`sourceName` renames at generation (`-n`). To rename later, search-replace
the chosen name across the tree (it appears in namespaces, file names, the
`.slnx`, and `appsettings.json`), then rebuild. `Wpf11AppTemplate` data
folders are stable on purpose — migrate user data explicitly if you change them.
