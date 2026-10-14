# Generators

Item templates scaffold by convention over the generated app. Install them
alongside the repo (each has its own template):

```powershell
dotnet new install <repo>/templates/items/devtemwpf-page
dotnet new install <repo>/templates/items/devtemwpf-service
```

## Page (`devtemwpf-page`)

```powershell
cd src/MyApp.App
dotnet new devtemwpf-page -n OrdersPage --Namespace MyApp.App
```

Creates `OrdersPage.xaml(+cs)` + `OrdersPageViewModel.cs` (tokenized XAML,
named controls, entrance wiring, `INavigableView`). Then, as the checklist
comment in the code-behind says:

1. Register page + ViewModel as transients in `AppComposition`.
2. Add a `MainViewModel` nav item (pick a verified `SymbolRegular` member —
   unverified names fail at compile; `DocumentAdd24` is a safe generic).

Orphan pages compile fine (DI resolves on navigation).

## Service (`devtemwpf-service`)

```powershell
dotnet new devtemwpf-service -n AuditService --Namespace MyApp.App
```

Creates `ISampleService`→`IAuditService` + implementation with a registration
comment for `AppComposition`. Keep services interface-first and
inert-without-configuration (ADR-0011 pattern).

## Visual Studio

Project templates (`devtemwpf-app`) appear in VS Create-a-new-project once
installed — no VSIX needed. Item templates are CLI-only (the VS Add-Item
dialog does not surface `dotnet new` item templates — Microsoft documented).
No VSIX ships in 0.8.0 by design (it would duplicate the same source).
