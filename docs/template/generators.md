# Generators

Item templates scaffold individual application elements (pages, controls,
ViewModels, services, tests) into a generated app by convention.

The full catalog, flags, naming rules, and post-generation checklists live in
[item-templates.md](item-templates.md). This page keeps only the two shortcuts
you need most often.

```powershell
dotnet new install <path-to-repo>   # one install covers app + item templates
```

## Page (`devtemwpf-page`)

```powershell
cd src/Orders.App
dotnet new devtemwpf-page -n OrdersPage --RootNamespace Orders
```

Creates `OrdersPage.xaml(+cs)` + `OrdersPageViewModel.cs` (tokenized XAML,
named controls, entrance wiring, `INavigableView`). Then, as the checklist
comment in the code-behind says:

1. Register page + ViewModel as transients in `AppComposition`.
2. Add a `MainViewModel` nav item (pick a verified `SymbolRegular` member -
   unverified names fail at compile; `DocumentAdd24` is a safe generic).

Orphan pages compile fine (DI resolves on navigation).

## Service (`devtemwpf-service`)

```powershell
dotnet new devtemwpf-service -n AuditService --RootNamespace Orders
```

Creates `ISampleService` -> `IAuditService` + implementation with a
registration comment for `AppComposition`. Keep services interface-first and
inert-without-configuration (ADR-0011 pattern).

## Visual Studio

Project templates (`devtemwpf-app`) appear in VS Create-a-new-project once
installed; item templates are CLI-only (the VS Add-Item dialog does not surface
`dotnet new` item templates). No VSIX ships in 1.x by design - it would
duplicate the same source for an unproven workflow (ADR-0019).

## Verification

```powershell
powershell -ExecutionPolicy Bypass -File eng/test-generators.ps1
```

Generates every template into an isolated app, checks for residue, builds with
`--warnaserror`, and runs the generated tests.
