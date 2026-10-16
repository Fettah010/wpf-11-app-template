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
3. Confirm no `#if (` / `#endif` residue in `src/`, and no `//#if` token at
   all in generated `.cs`: a conditional token mentioned even inside a
   comment silently truncates the generated file from that line on
   (1.4.0 T08 evidence). The matrix asserts this.
4. `dotnet --version` must be 10.0.x (pinned band in `global.json`).

## Shell/theme defaults

`--Shell Sidebar|Rail|Top|Tabbed` (1.4.0+) seeds the first-launch shell
default; there is no `--ThemeDefault` by design. Runtime shell changes apply
at next launch (see the Settings page note). Never write the literal text
`//#if` in comments of template-processed files (see above).

## Rebrand / rename

`sourceName` renames at generation (`-n`). To rename later, search-replace
the chosen name across the tree (it appears in namespaces, file names, the
`.slnx`, and `appsettings.json`), then rebuild. `Wpf11AppTemplate` data
folders are stable on purpose — migrate user data explicitly if you change them.
