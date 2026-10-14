# Design tokens

Single source of truth: `src/DevTemWpf.App/DesignTokens/` for XAML resources
plus `Services/MotionTokens.cs` for motion values (moves to the UI library
with its second consumer, per ADR-0003).

## Layers (`App.xaml` order)

1. `ui:ThemesDictionary` + `ui:ControlsDictionary` (WPF-UI).
2. `SemanticColors.xaml` — `App*` brush aliases, all
   `{DynamicResource <WPF-UI key>}`. Follows light/dark/HC automatically.
3. `Typography.xaml` — `AppFontFamily` (`Segoe UI Variable, Segoe UI`),
   `AppDisplay/Title/Subtitle/SectionHeading/Body/SecondaryBody/Caption/Error`
   text styles, `AppIconXLarge/Large/Small`.
4. `Spacing.<Density>.xaml` — `AppPageMargin`, `AppCardPadding`,
   `AppContentMaxWidth`, gap rhythm (`AppGapTitle/Item/Group/Section/Flow/Inline/Gutter`),
   plus scalar `AppSection/Item/ControlSpacing` Doubles for code/demo use.
   Comfortable default; Compact swapped at runtime.
5. `Shapes.xaml` — `AppCornerRadiusSmall/Medium/Large`,
   `AppBorderThickness`, `AppFocusThickness`.

## Gap rhythm (1.2.0)

Vertical rhythm flows downward through bottom margins. Comfortable values
preserve the pre-1.2.0 look; Compact tightens it (screenshot-visible):

| Token | Comfortable | Compact | Use |
|---|---|---|---|
| `AppGapTitle` | `0,0,0,4` | `0,0,0,4` | Label → field, heading → caption (tight pairs) |
| `AppGapItem` | `0,0,0,8` | `0,0,0,6` | Caption → content, control → control |
| `AppGapGroup` | `0,0,0,12` | `0,0,0,8` | Card-internal groups, form rows |
| `AppGapSection` | `0,0,0,16` | `0,0,0,12` | Between page sections |
| `AppGapFlow` | `0,0,8,8` | `0,0,6,6` | Wrap-flow children (buttons, badges) |
| `AppGapInline` | `0,0,16,8` | `0,0,12,6` | Horizontal option groups (check/radio) |
| `AppGapGutter` | `0,0,12,0` | `0,0,8,0` | Column gutters (dashboard grid) |

## Rules

- **Add-only.** Never redefine a WPF-UI key. Aliases use the `App` prefix.
- **Dynamic for color** (theme follows), **static for metrics** (density
  swaps whole dictionaries; StaticResource is required for the swap to be
  complete — see `DesignTokenBootstrap`).
- No unexplained literals in app XAML: rhythm margins must use `AppGap*`
  (guarded by `XamlTokenTests`); `0,4,0,*` optical lead-ins are the
  documented density-invariant exception. Content sizes (chart heights,
  column widths, ring diameters) stay literal. Numbers must otherwise come
  from tokens or be chrome configuration (window min-size) with a comment.
- `Thickness` gap tokens are for XAML margins; `Double` spacing tokens are
  for scalar/code use (demo bars, programmatic layout). Do not add a second
  unit for the same step.
- Upstream key references (WPF-UI 4.3.0 `Resources/Theme/Dark.xaml`):
  `TextFillColor*`, `ControlFillColor*`, `CardBackgroundFillColorDefault`,
  `CardStrokeColorDefault`, `DividerStrokeColorDefault`,
  `FocusStrokeColorOuter/Inner`, `SystemAccentColorPrimary/Secondary`,
  `SystemFillColorSuccess/Caution/Critical/Neutral(+Background)`,
  `SubtleFillColorSecondary`, `SolidBackgroundFillColorBase`.

## Density

`DesignTokenBootstrap.ApplyDensity` removes `Spacing.*` dictionaries and
adds the requested one; missing/corrupt files fall back to Comfortable.
Consumers must read spacing only via StaticResource (documented rule —
DynamicResource to a removed dictionary would silently dangle).
