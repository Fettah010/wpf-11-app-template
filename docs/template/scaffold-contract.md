# Scaffold contract (0.6.0 baseline)

Defines what each preset promises so later milestones cannot silently
redefine them. The generator itself is built in 0.8.0; until then this is
the contract to implement against.

- **Clean:** minimal WPF app, chosen shell only, basic theme, MVVM.
  No dashboard, gallery, database, updater, tray, diagnostics UI, telemetry,
  or release infrastructure unless explicitly selected. Disabling a feature
  removes its package references and registrations — not just hides UI.
- **Recommended:** clean + polished shell, theme/settings foundation,
  logging, diagnostics, standard state patterns, tests.
- **Full:** recommended + demo dashboard, UI gallery, optional service
  modules, distribution options, release automation.
- **Gallery/demo:** reference showcase of the complete UI system and optional
  effects. Never a dependency of product presets.

The 0.1.0 minimal app (`src/DevTemWpf.App`) is the embryo of the **clean**
preset. Rule: new features must not turn the clean scaffold into an all-on
application (verified per milestone by building the clean output).

## Dashboard exclusion set (0.6.0, enforced by the 0.8.0 generator)

Removing the dashboard from a scaffold deletes exactly:

- `src/.../Views/DashboardPage.xaml(.cs)`, `ViewModels/DashboardViewModel.cs`,
  `Services/SampleDataService.cs`
- `AppComposition` lines: `SampleDataService`, `DashboardViewModel`,
  `DashboardPage` registrations
- `MainViewModel` "Dashboard" nav item
- `LiveChartsCore.SkiaSharpView.WPF` package reference (+ CPM entry if unused
  elsewhere)

Nothing outside this set may reference dashboard types (verified by review;
a compile check after deletion is the 0.8.0 acceptance test).

## Per-service modules (1.6.0)

The single `IncludeServices` flag was split into per-module flags. Each module
is independently removable in the `Custom` preset and covers the same file /
package / DI / config set:

| Module | Symbol / flag | Files | Packages | DI / hooks | Config | Tests |
|---|---|---|---|---|---|---|
| Logging | `IncludeLogging` / `--CustomLogging` | `Services/Logging.cs` | Serilog, Serilog.Extensions.Hosting, Serilog.Sinks.File | `AddAppLogging()` in `App.xaml.cs` | `appsettings.json` (Serilog) | host logging path |
| Local data | `IncludeLocalData` / `--CustomLocalData` | `Services/FavoritesStore.cs` | Microsoft.Data.Sqlite | `AddSingleton<FavoritesStore>` | — | `FavoritesStoreTests` |
| HTTP | `IncludeHttp` / `--CustomHttp` | `Services/StatusClient.cs`, `HttpOptions.cs` | Microsoft.Extensions.Http | `Configure<HttpOptions>` + `AddHttpClient<StatusClient>` | `Http` section | `StatusClientTests` |
| Backup | `IncludeBackup` / `--CustomBackup` | `Services/BackupService.cs`, `IBackupService.cs` | — | `AddSingleton<IBackupService, BackupService>` | — | `BackupServiceTests` |
| Updates | `IncludeUpdates` / `--CustomUpdates` | `Services/UpdateService.cs`, `IUpdateService.cs`, `UpdateOptions.cs`, `VelopackUpdateEngine.cs` | Velopack | `Configure<UpdateOptions>` + `AddSingleton<IUpdateService>` + `VelopackApp.Build().Run()` | `Updates` section | `UpdateServiceTests` |
| Tray | `IncludeTray` / `--CustomTray` | `TrayGlyph.cs` | WPF-UI.Tray | `AddSingleton<INotifyIconService>` + `MainWindow.SetupTray` | — | UI |

Dependency edges (Custom only): diagnostics → {logging, http, backup, updates};
backup → localdata. Disabling a module removes its files, packages,
registrations, and tests — never just hides UI. The Diagnostics **UI** module
still removes the dashboard set described above.
