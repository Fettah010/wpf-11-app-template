# Choosing modules (presets and flags)

Every production service is independently selectable. Named presets stay
minimal and predictable; the `Custom` preset exposes one flag per module.

## Presets

| Preset | What you get |
|---|---|
| `Clean` (minimal) | Shell, theme/tokens, motion, feedback, localization. No services. |
| `Recommended` (default) | Clean + diagnostics + logging, local data, HTTP, backup, updates, tray. |
| `Full` | Recommended + dashboard and charts. |
| `Custom` | Only the `--Custom<Module>` flags you set (dependency edges apply). |

```powershell
dotnet new devtemwpf-app -n MyApp --Preset Recommended
dotnet new devtemwpf-app -n MyApp --Preset Custom --CustomLogging true --CustomUpdates true
```

## Flags and dependency edges

| Flag | Adds |
|---|---|
| `--CustomLogging` | Serilog file logging (bounded, redacted). |
| `--CustomLocalData` | SQLite `FavoritesStore`. |
| `--CustomHttp` | Typed `HttpClient` (`StatusClient`). |
| `--CustomBackup` | Backup/restore. Implies local data. |
| `--CustomUpdates` | Velopack update engine. |
| `--CustomTray` | System tray icon. |
| `--CustomDiagnostics` | Diagnostics page. Implies logging, HTTP, backup, updates. |
| `--CustomAuth` | Opt-in MSAL auth sample (see [auth-and-crash](auth-and-crash.md)). |
| `--CustomCrash` | Local-first crash capture. Implies logging (shared redaction). |

Disabling a module removes its source files, NuGet packages, DI registrations,
and tests — it never merely hides UI. `eng/test-templates.ps1` asserts this
presence **and** absence in generated source and package closure.

Windows app notifications (`AppNotificationManager`) were evaluated in 1.6.0
and deferred (no-go) — no package or flag ships; the in-app Snackbar surface is
the fallback. See `docs/DEFERRED.md` (D07t) and ADR-0017.

## Where the machinery lives

- `.template.config/template.json` — symbols (`Custom<Module>`, computed
  `Include<Module>`) and per-file `sources[].modifiers` conditions.
- C# whole-line conditionals use `//#if (Include<Module>)`; csproj/XAML use
  `<!--#if (Include<Module>) -->`. In the repository's own build these are
  comments, so the reference app compiles every module.
- `docs/template/scaffold-contract.md` records each module's file / package /
  DI / config / test contract.
