# Item templates (generators)

Item templates scaffold individual application elements into a generated app.
They live in this repository under `templates/items/` and ship inside
`DevTemWPF.Templates`, so one install covers the project template and every
generator.

```powershell
dotnet new install <path-to-repo>            # or a .nupkg: dotnet new install DevTemWPF.Templates.1.8.0.nupkg
dotnet new list devtemwpf                    # 1 project template + 11 item templates
```

## Catalog

| Template | Generates | Target project |
|---|---|---|
| `devtemwpf-page` | `Page` + XAML page + ViewModel (`INavigableView`, entrance motion) | app |
| `devtemwpf-settings` | Settings page with `CardControl` sections, toggles, reset command + ViewModel | app |
| `devtemwpf-listdetail` | Master-detail page (search, selection, inspector) + ViewModel + item model | app |
| `devtemwpf-datagrid` | DataGrid page (formatted columns, search, refresh) + ViewModel + row model | app |
| `devtemwpf-treeview` | Hierarchical TreeView page + ViewModel + observable node model | app |
| `devtemwpf-tabpage` | Document page for the Tabbed workspace shell + ViewModel | app |
| `devtemwpf-control` | Custom control + `Themes/Generic.xaml` default style + dependency properties + automation peer | app |
| `devtemwpf-viewmodel` | Standalone `ObservableObject` with `RelayCommand` commands and navigation hooks | app |
| `devtemwpf-service` | Service interface + implementation | app |
| `devtemwpf-test-vm` | Behavioral unit tests for a ViewModel (commands + state) | tests |
| `devtemwpf-test-service` | Behavioral unit tests for a service (method results) | tests |

Every scaffold uses project conventions: design tokens only (no hard-coded
colors or metrics), `CommunityToolkit.Mvvm` (`[ObservableProperty]`,
`[RelayCommand]`), `INavigableView<T>` for pages, and
`IMotionService.PlayEntranceAsync` for page transitions.

## Usage

Generate into the *project folder* (not the app root), and pass the app's root
namespace once:

```powershell
cd src/Orders.App

dotnet new devtemwpf-datagrid -n OrdersPage -o . --RootNamespace Orders
dotnet new devtemwpf-service  -n AuditService -o . --RootNamespace Orders
```

Test scaffolds go into the unit-test project:

```powershell
cd tests/Orders.Unit

dotnet new devtemwpf-test-vm      -n OrdersViewModelTests -o . --RootNamespace Orders
dotnet new devtemwpf-test-service -n AuditServiceTests   -o . --RootNamespace Orders
```

### Flags

| Flag | Default | Meaning |
|---|---|---|
| `-n`, `--name` | template sample name | Item name. Also drives companion type names (see below). |
| `-o`, `--output` | current directory | Target project or folder. |
| `--RootNamespace` | `DevTemWpf` | Replaces the `DevTemWpf` root everywhere. The `.App` / `.Unit` suffixes are preserved, so pass the app root (`Orders`), not `Orders.App`. |

### What lands where

The generated files are flat (the .NET template engine copies into `-o`), so
choose `-o` like you would choose a folder in Solution Explorer:

| Namespace written | Keep files in |
|---|---|
| `<Root>.App.Views` | `src/<Root>.App/Views` (optional; the folder is not part of the namespace) |
| `<Root>.App.ViewModels` | `src/<Root>.App/ViewModels` |
| `<Root>.App.Models` | `src/<Root>.App/Models` |
| `<Root>.App.Services` | `src/<Root>.App/Services` |
| `<Root>.App.Controls` | `src/<Root>.App/Controls` (`devtemwpf-control` also writes `Themes/Generic.xaml`) |
| `<Root>.Unit` | `tests/<Root>.Unit` |

Generating into `src/<Root>.App` root is supported and compiles; moving the
files into the conventional subfolders afterwards is cosmetic in C#, but the
control template's `Themes/Generic.xaml` must sit in a `Themes` folder.

### Companion type naming

One `-n` argument drives every type in the scaffold, so repeated generation
never collides:

| Template | `-n` | Types | Companion file |
|---|---|---|---|
| `devtemwpf-datagrid` | `OrdersPage` | `OrdersPage`, `OrdersPageViewModel`, `OrdersRow` | `OrdersPage.Row.cs` |
| `devtemwpf-listdetail` | `OrdersPage` | `OrdersPage`, `OrdersPageViewModel`, `OrdersItem` | `OrdersPage.Item.cs` |
| `devtemwpf-treeview` | `OrdersPage` | `OrdersPage`, `OrdersPageViewModel`, `OrdersNode` | `OrdersPage.Node.cs` |
| `devtemwpf-control` | `QuantityEditor` | `QuantityEditor`, `QuantityEditorAutomationPeer` | `Themes/Generic.xaml` |
| `devtemwpf-service` | `AuditService` | `AuditService`, `IAuditService` | - |
| `devtemwpf-test-vm` | `OrdersViewModelTests` | type under test `OrdersViewModel` | - |
| `devtemwpf-test-service` | `AuditServiceTests` | type under test `AuditService` | - |

Names without a `Page` suffix (for example `-n InventoryGrid`) keep that name
for the companion type as well (`InventoryGrid` in `InventoryGrid.Row.cs`),
which still compiles because the model lives in a different namespace. Because
every file name is derived from `-n`, generating a second data page into the
same project cannot collide with the first.

## After generating a page

The code-behind carries a checklist comment; the short version:

1. Register the page and its ViewModel as transients in `AppComposition`.
2. Add a `NavigationViewItem` to `MainViewModel` (use a verified
   `SymbolRegular` member; `DocumentAdd24` is a safe generic choice).
3. Remove the `Title`/header placeholders you do not need.

Orphan pages compile fine (DI resolves them on navigation only).

## After generating a service

Register it in `AppComposition` (`AddSingleton<IAuditService, AuditService>()`)
and keep it interface-first and inert without configuration (ADR-0011).

## Adding a second custom control

`devtemwpf-control` writes `Themes/Generic.xaml`, which must stay a single
dictionary per assembly. A second run of the template will therefore refuse to
overwrite it (exit code 73). Copy the new `<Style TargetType="{x:Type
local:YourControl}">` block into the existing `Themes/Generic.xaml` and delete
the generated duplicate, or move each control's style into its own dictionary
and merge it from `Generic.xaml`:

```xml
<ResourceDictionary.MergedDictionaries>
  <ResourceDictionary Source="/Themes/QuantityEditor.xaml" />
</ResourceDictionary.MergedDictionaries>
```

## Verifying the generators

`eng/test-generators.ps1` is the blocking generator gate. It installs the
templates into an isolated CLI home (your global `dotnet new` registrations are
not touched), generates an app, runs all eleven templates plus repeat and
edge-case generations, fails on any leftover template token, builds with
`--warnaserror`, and runs the generated tests:

```powershell
powershell -ExecutionPolicy Bypass -File eng/test-generators.ps1
powershell -ExecutionPolicy Bypass -File eng/test-generators.ps1 -Preset Full
powershell -ExecutionPolicy Bypass -File eng/test-generators.ps1 -PackagePath artifacts/DevTemWPF.Templates.1.8.0.nupkg
```

## Visual Studio

- **New Project:** `devtemwpf-app` appears after `dotnet new install` (Visual
  Studio lists installed `dotnet new` project templates).
- **Add -> New Item:** Visual Studio only lists Visual Studio item templates
  (`*.vstemplate`); `dotnet new` *item* templates are not surfaced there. Use
  the CLI commands above, then move the files if you want a different folder.
  A VSIX was evaluated and rejected for 1.x (ADR-0019); reopen D08r only with a
  measured consumer requirement.
