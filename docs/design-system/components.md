# Component catalog (1.4.0)

Living samples: Controls page (`src/DevTemWpf.App/Views/ControlsPage.xaml`,
nav item "Controls") and Data page (`Views/DataPage.xaml`, nav item "Data").
Shells, workspace, and palette: `docs/design-system/shells.md`.
Conventions: `docs/design-system/component-standards.md`. Localization rule:
display copy lives in resx (`Strings.*`, en+de); `AutomationProperties.Name`
stays English-invariant so UI tests and screen-reader references are stable.

## Buttons

| Control | Variants demonstrated | Notes |
|---|---|---|
| `ui:Button` | Primary / Secondary / Transparent / Danger, icon (`{ui:SymbolIcon …}`), disabled | `Appearance` sets intent — one Primary per view. Native `ToggleButton` (no `ui:` prefix — styled implicitly). |
| `ui:HyperlinkButton` | Inline navigation-style action | For secondary navigation, not primary actions. |
| `ui:SplitButton` | `Flyout` takes a `ContextMenu` | Verified against 4.3.0 source; dropdown items are plain `MenuItem`. |
| Busy pattern | Button + `IsIdle` + `ProgressRing` + **Cancel**, single-flight `IAsyncRelayCommand`, owned CTS | `AllowConcurrentExecutions=false`; cancel maps to `Idle`, never `Failed`. Unit + UI tested. |

## Inputs

| Control | Notes |
|---|---|
| `ui:TextBox` (`PlaceholderText`, `Icon`), `ui:PasswordBox`, `ui:NumberBox` | Placeholder + automation name on every field. |
| `ComboBox`, `CheckBox`, `RadioButton`, `ui:ToggleSwitch`, `Slider` | Stock WPF controls are WPF-UI-styled; group radios with a shared parent. |
| Time editor (recipe) | `TimeEditorViewModel`: two validated boxes + Now; `TimeSpan` commits only when valid; text error (never color-only). The WPF-UI `TimePicker` exists in 4.3.0 but neither surfaces its automation name nor opens its popup reliably (spike evidence in the 1.3.0 report) — do not adopt it without re-audit. |

## Content

`ui:Card`, `ui:CardExpander` (`Header`/`Content`), native `Expander`,
`ui:InfoBadge` (`Value`), `ToolTip` (explanation only — never the sole
information channel), `ui:BreadcrumbBar` (shell header, auto-syncs).

Known toolkit gap (not fixed): `ui:Card` does not surface
`AutomationProperties.Name` — name the interactive content inside instead.

## Feedback

| Pattern | Implementation |
|---|---|
| Inline severity | `ui:InfoBar` Informational/Success/Warning/Error, `IsOpen`. |
| Progress | Native `ProgressBar` (determinate + indeterminate), `ui:ProgressRing`. |
| Transient confirmation | `ISnackbarService.Show` via VM command (fake-tested). |
| Decisions | `IContentDialogService.ShowSimpleDialogAsync` → `ContentDialogResult` written to status (fake-tested both paths). |
| Validation | `ObservableValidator` + DataAnnotations with `ErrorMessageResourceType` (localized); per-field error text (never color-only); Save validates all. Same pattern drives form, grid rows, and time input. |
| Empty/loading/error | Text states + retry commands; grid `LoadState` (Loading/Loaded/Error); progress ring spins only while loading. |

## Data

Dashboard (`Views/DashboardPage`, nav item "Dashboard", Full preset) and
Data page (`Views/DataPage`) demonstrate the data patterns.
`SampleDataService` (dashboard) and deterministic builders (Data page) are
the only data sources.

| Pattern | Implementation |
|---|---|
| Stat cards | `ItemsControl` + `WrapPanel` (wraps on narrow), `MetricItem` template. |
| Chart | `lvc:CartesianChart`, transparent background, `ChartPalette.For` rebound on theme/accent change (light/dark/HC), textual `ChartSummary` for screen readers. Chart package stays dashboard-only (Full). |
| Read-only table | Dashboard `ListView` + `GridView`; header buttons sort (direction in status line); `ICollectionView` filter; virtualized (asserted); fixed height + outer page scroll on short windows. |
| Editable grid | Data page native `DataGrid`: explicit columns, `IDataErrorInfo` row validation, add-rows, filter/search, selection detail, load states. Never derive emptiness from `ICollectionView.IsEmpty` with `CanUserAddRows` — the `NewItemPlaceholder` keeps it false; count data rows instead. |
| Tree | Data page native `TreeView`: two-way `IsExpanded`/`IsSelected` containers, expand/collapse-all, selection sync in code-behind (`SelectedItem` is read-only), expansion persisted to settings on navigate-away and restored on navigate-to. |
| Master/detail | Table (2*) + detail card (1*, min 240); selection drives detail; clear button. Dashboard and grid samples both. |
| Empty state | Text notice bound to a data-count predicate (not `IsEmpty`) + retry where applicable. |
| Stepper | Index-bounded Next/Back + progress + summary, all in VM. |
| Paging (recipe) | Prefer virtualization (proven to 10k rows); if true paging is needed, page in the VM over `ICollectionView` with explicit page state — no custom control required. |
| Selectable cards (recipe) | Bind card selection visual to a VM boolean (see dashboard favorite toggle: glyph + state text, unit-tested). |

## Shells, workspace, palette

See `docs/design-system/shells.md` (1.4.0): Sidebar/Rail/Top/Tabbed presets
via the shared route registry; document workspace with tab reorder (drag +
keyboard Move), resizable detail split with persisted width, and state
persistence/recovery; Ctrl+K command palette with filter/execute/dismiss and
focus return. Shell selection applies at next launch (ADR-0015).

| Pattern | Implementation |
|---|---|
| Route registry | `Services/NavigationRoutes.cs`: stable keys shared by rail, palette, and documents; conditional entries. |
| Workspace documents | `WorkspacePage` + singleton `WorkspaceViewModel`; pages hosted in per-tab chromeless `Frame`s (WPF parents `Page` only to `Window`/`Frame`). |
| Tab reorder | Detail Move buttons (keyboard-operable, unit + UI proven). Pointer drag prototyped and deferred (unreliable OLE gestures across runners); see 1.4.0 report. |
| Split/detail | `GridSplitter` (arrow-key operable when focused) + toggle checkbox; width/visibility persisted. |
| Palette | `PaletteViewModel` (`nav.*` IDs, substring filter, enabled states) + shell overlay; Enter executes, Esc/scrim dismisses, focus returns to invoker. |

## Gaps carried (not built in 1.4.0)

- **Chips:** no WPF-UI control — compose from `ToggleButton` when needed.
- **Loading skeleton:** compose from shimmer-free placeholders.
- **Chart advanced theming** (legends, tooltips): deferred; current chart follows theme + accent with SR summary.
- **Live shell hot-switch:** deliberately restart-apply (ADR-0015); revisit only with a passing hot-switch UI test on a fixed toolkit.
- **Window tear-out / cross-window drag:** deferred (no consumer); drag stays in-workspace by construction.
