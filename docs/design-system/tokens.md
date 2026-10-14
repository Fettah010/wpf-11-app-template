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
   `AppSectionSpacing`, `AppItemSpacing`, `AppControlSpacing`,
   `AppContentMaxWidth`. Comfortable default; Compact swapped at runtime.
5. `Shapes.xaml` — `AppCornerRadiusSmall/Medium/Large`,
   `AppBorderThickness`, `AppFocusThickness`.

## Rules

- **Add-only.** Never redefine a WPF-UI key. Aliases use the `App` prefix.
- **Dynamic for color** (theme follows), **static for metrics** (density
  swaps whole dictionaries; StaticResource is required for the swap to be
  complete — see `DesignTokenBootstrap`).
- No unexplained literals in app XAML: numbers must come from tokens or be
  chrome configuration (window min-size) with a comment.
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
