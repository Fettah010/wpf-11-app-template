# DevTemWPF Templates

Create a Windows 11-first WPF application with the .NET template engine.

## Install

```powershell
dotnet new install DevTemWPF.Templates
dotnet new devtemwpf-app --help
```

## App presets

| Preset | Includes |
| --- | --- |
| `Clean` | Minimal shell, settings, and essential tests |
| `Recommended` | Diagnostics, logging, updates, local data, and integration tests |
| `Full` | Recommended features plus dashboard charts |
| `Custom` | Choose optional modules explicitly |

```powershell
dotnet new devtemwpf-app -n MyApp --Preset Recommended --Shell Tabbed
```

The generated project includes its own `README.md`, `FEATURES.md`, build/test
scripts, and source. Restore and run its release checks with:

```powershell
dotnet restore
dotnet build -c Release --warnaserror
dotnet test -c Release
```

Update the generated README and feature checklist to match your application.
