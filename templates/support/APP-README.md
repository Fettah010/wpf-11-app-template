# DevTemWpf — your app starts here

Scaffolded with `dotnet new devtemwpf-app`. See `FEATURES.md` for what's
included in this preset.

## Run

```powershell
dotnet restore
dotnet build -c Release
dotnet test -c Release
powershell -ExecutionPolicy Bypass -File eng/smoke.ps1
```

The smoke script restores, builds (Debug + Release), runs tests, and checks
that the app window appears.

## Next steps

1. Search for `TODO-APP` (none yet - this is your canvas).
2. Add an element with a generator, for example
   `dotnet new devtemwpf-page -n OrdersPage --RootNamespace <YourRoot>`
   (run inside `src/<YourRoot>.App`; the item templates ship in the template
   package - see `docs/template/item-templates.md` in the template repo for the
   full catalog).
3. Pick a shell, theme, density, and motion in Settings; preferences persist.
4. Read `docs/` in the template repo for the design system, ADRs, packaging,
   and localization guides.

## Notes

- Windows 11 + .NET 10 SDK required (`global.json` pins the band).
- User data lives under `%LocalAppData%/Wpf11AppTemplate` (logs, database,
  backups) and `%AppData%/Wpf11AppTemplate` (settings).
- No telemetry, no network calls unless you configure them (see FEATURES.md).
