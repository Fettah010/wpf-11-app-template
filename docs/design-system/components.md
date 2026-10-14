# Component catalog (0.4.0)

Living sample: Controls page (`src/DevTemWpf.App/Views/ControlsPage.xaml`,
nav item "Controls"). Conventions: `docs/design-system/component-standards.md`.

## Buttons

| Control | Variants demonstrated | Notes |
|---|---|---|
| `ui:Button` | Primary / Secondary / Transparent / Danger, icon (`{ui:SymbolIcon …}`), disabled | `Appearance` sets intent — one Primary per view. Native `ToggleButton` (no `ui:` prefix — styled implicitly). |
| `ui:HyperlinkButton` | Inline navigation-style action | For secondary navigation, not primary actions. |
| `ui:SplitButton` | `Flyout` takes a `ContextMenu` | Verified against 4.3.0 source; dropdown items are plain `MenuItem`. |
| Busy pattern | Button + `IsIdle` + `ProgressRing`, single-flight `IAsyncRelayCommand` | `AllowConcurrentExecutions=false`; never block input on animation. |

## Inputs

| Control | Notes |
|---|---|
| `ui:TextBox` (`PlaceholderText`, `Icon`), `ui:PasswordBox`, `ui:NumberBox` | Placeholder + automation name on every field. |
| `ComboBox`, `CheckBox`, `RadioButton`, `ui:ToggleSwitch`, `Slider` | Stock WPF controls are WPF-UI-styled; group radios with a shared parent. |
| `ui:TimePicker` | Ships with WPF-UI; not yet demonstrated — first use must add a sample. |

## Content

`ui:Card`, `ui:CardExpander` (`Header`/`Content`), native `Expander`,
`ui:InfoBadge` (`Value`), `ToolTip` (explanation only — never the sole
information channel), `ui:BreadcrumbBar` (shell header, auto-syncs).

## Feedback

| Pattern | Implementation |
|---|---|
| Inline severity | `ui:InfoBar` Informational/Success/Warning/Error, `IsOpen`. |
| Progress | Native `ProgressBar` (determinate + indeterminate), `ui:ProgressRing`. |
| Transient confirmation | `ISnackbarService.Show` via VM command (fake-tested). |
| Decisions | `IContentDialogService.ShowSimpleDialogAsync` → `ContentDialogResult` written to status (fake-tested both paths). |
| Validation | `FormViewModel : ObservableValidator` + DataAnnotations; per-field error text (never color-only); Save validates all. |
| Empty/loading | InfoBar + progress patterns above; dedicated skeleton control deferred (no WPF-UI control — gap, 0.6.0). |

## Gaps (no custom control built in 0.4.0)

- **Chips:** no WPF-UI control — compose from `ToggleButton` when needed (0.6.0).
- **Loading skeleton:** compose from shimmer-free placeholders (0.6.0).
- **DataGrid/table/tree:** 0.6.0 data milestone.
- `ui:TimePicker` sample outstanding (first-use rule).

## Data (0.6.0)

Dashboard (`Views/DashboardPage`, nav item "Dashboard") demonstrates the data
patterns; `SampleDataService` is the only data source (deterministic seed).

| Pattern | Implementation |
|---|---|
| Stat cards | `ItemsControl` + `WrapPanel` (wraps on narrow), `MetricItem` template. |
| Chart | `lvc:CartesianChart` + VM-built `LineSeries<double>` (LiveCharts2, library-default theme for now). |
| Table | `ListView` + `GridView`; header buttons sort (direction in status line); `ICollectionView` filter; virtualized (asserted). |
| Master/detail | Table (2*) + detail card (1*, min 220); selection drives detail; clear button. |
| Empty state | `NoResultsMessage` text (not color-only). |
| Stepper | Index-bounded Next/Back + progress + summary, all in VM. |

Exclusion: dashboard file set + registrations + nav item + chart package ref
drop as one unit (`docs/template/scaffold-contract.md`). Gaps carried:
TreeView, full DataGrid, drag-and-drop, chart theming, top-nav/tabbed shells
(0.8.0).
