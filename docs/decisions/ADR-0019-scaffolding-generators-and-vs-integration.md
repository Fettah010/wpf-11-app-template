# ADR-0019: Item-template scaffolding and CLI-first Visual Studio integration

- Status: Accepted
- Date: 2026-10-27
- Milestone: 1.7.0
- Supersedes/extends: ADR-0012 (template architecture), D08r in `docs/DEFERRED.md`

## Context

Roadmap §8.7 requires expanding `dotnet new` item templates for standard
application elements (standard page, settings page, list-detail page, DataGrid
page, TreeView page, tabbed-workspace page, custom control, standalone
ViewModel, service, and companion test scaffolds with real behavioral
assertions), and it requires a recorded decision on Visual Studio integration
(D08r).

`1.6.0` shipped two item templates (`devtemwpf-page`, `devtemwpf-service`).
They exposed a real weakness: the only documented namespace knob was
`--Namespace <App>.App`, every scaffold wrote a single namespace, and the test
scaffolds could not resolve the `App` namespace at all. The expanded catalog
had to fix naming/namespace substitution before adding ten more templates.

Options considered:

1. **Dedicated Visual Studio extension (VSIX)** with `*.vstemplate` item
   definitions and/or wizard UI.
2. **CLI-first templates** packaged in `DevTemWPF.Templates`, discovered by
   `dotnet new` everywhere, with Visual Studio support limited to what the
   platform already provides.

## Evidence

Verified in this repository and against Visual Studio behavior:

- `dotnet new install <repo-or-nupkg>` registers both the project template
  (`devtemwpf-app`) and every item template located under
  `templates/items/*/.template.config/`. Verified output of
  `dotnet new list devtemwpf` lists 12 entries (1 project + 11 item).
- Visual Studio's **New Project** dialog lists installed `dotnet new`
  *project* templates, so `devtemwpf-app` is available after install with no
  extension.
- Visual Studio's **Add → New Item** dialog lists only Visual Studio item
  templates (`*.vstemplate`) shipped by VSIX/VS-installed packages. `dotnet new`
  *item* templates are not surfaced there — consistent with ADR-0012 and the
  `0.8.0` research (`docs/releases/0.8.0-plan.md`). ADR-0019 therefore records
  this as a known, accepted limitation rather than a claim of native support.
- Closing that gap with a VSIX requires `Microsoft.VSSDK.BuildTools`, a
  code-signing certificate, per-Visual-Studio-version shims, Marketplace
  publisher maintenance, and duplicating every item template as `*.vstemplate`
  content that must be kept byte-compatible with the CLI versions.
- Every consumer workflow exercised by this repository (PowerShell gates, CI,
  coding agents) is CLI-driven; the CLI templates are the single source that
  those workflows can verify automatically (`eng/test-generators.ps1`).
- The .NET template engine supports exactly the substitution this catalog
  needs: `sourceName` for the item name, a `RootNamespace` parameter that
  replaces the `DevTemWpf` root in every template (`<Root>.App` and
  `<Root>.Unit` are preserved), and `forms`-based `replace` transforms that
  derive companion type names (for example `OrdersPage` → `OrdersRow`) from
  the single `-n` argument.

## Decision

1. **CLI-first item templates.** Ship eleven templates under `templates/items/`
   in `DevTemWPF.Templates`:
   - `devtemwpf-page` — NavigationView page + ViewModel + entrance motion.
   - `devtemwpf-settings` — settings page with `CardControl` sections,
     toggles, and reset command.
   - `devtemwpf-listdetail` — master-detail page + ViewModel + item model.
   - `devtemwpf-datagrid` — DataGrid page with column formatting, search, and
     row model.
   - `devtemwpf-treeview` — hierarchical TreeView page + observable node model.
   - `devtemwpf-tabpage` — document page for the Tabbed workspace shell.
   - `devtemwpf-control` — custom control with `Themes/Generic.xaml` default
     style, dependency properties, and an automation peer.
   - `devtemwpf-viewmodel` — standalone `ObservableObject` with `RelayCommand`
     commands and navigation lifecycle hooks.
   - `devtemwpf-service` — service interface + implementation.
   - `devtemwpf-test-vm`, `devtemwpf-test-service` — behavioral unit-test
     scaffolds that assert command/state/service behavior, not file existence.
2. **D08r closed as CLI-first, no VSIX in 1.x.** Visual Studio support is:
   project template in **New Project** after `dotnet new install`; item
   templates through the CLI (or by copy/paste). Building a VSIX is deferred
   indefinitely because the maintenance cost is not justified by the workflows
   this template targets; if demand appears, reopen D08r with a measured
   consumer requirement.
3. **Namespace and naming contract.** All item templates expose
   `--RootNamespace <Root>` (default `DevTemWpf`) and substitute the whole
   `DevTemWpf` root, so generated files land in `<Root>.App.Views`,
   `<Root>.App.ViewModels`, `<Root>.App.Models`, `<Root>.App.Services`, or
   `<Root>.Unit`. Companion types derive from `-n` via template `forms`, so
   repeated generation never collides and users never hand-edit type names.
4. **Verification is automated, not manual.** `eng/test-generators.ps1` is the
   blocking generator gate: it installs the templates (source folder or
   `.nupkg`) into an isolated CLI home, generates an app, runs all eleven
   templates plus repeat/edge-case generations, fails on any residual template
   token (`DevTemWpf`, `Sample*`, undefined `App*` styles), builds
   `--warnaserror`, and runs the generated tests.

## Consequences

- One canonical artifact (`DevTemWPF.Templates`) carries the app template and
  every generator; CI and agents verify the same bytes users install.
- Item scaffolding is one command per element and stays inside project
  conventions (tokens, `INavigableView`, CommunityToolkit.Mvvm, entrance
  motion), so generated code compiles without manual repair.
- Visual Studio users get no Add-Item dialog integration; the limitation is
  documented in `docs/template/item-templates.md` and can be revisited only
  with new evidence.
- Generated companion types are deterministic for a given `-n`, which removes
  the collision class of defects that flat scaffolds otherwise have.
