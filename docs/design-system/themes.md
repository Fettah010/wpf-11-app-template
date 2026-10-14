# Themes

## Modes

`ThemeMode`: System (default), Light, Dark — persisted in settings, applied
by `IThemeService` before the window exists, live-switchable from Settings.

- **System** tracks the OS via `SystemThemeWatcher` + explicit OS-theme
  resolution (so the persisted backdrop choice is honored).
- **High contrast:** WPF-UI HC dictionaries (`HC1/HC2/HCBlack/HCWhite`)
  load automatically; our aliases resolve into them with no token code;
  backdrop is forced to `None` (upstream behavior, kept).
- Runtime changes go through `ApplicationThemeManager.Apply` (DWM-only
  path). Never re-set `FluentWindow.WindowBackdropType` post-init
  (crashes on .NET 10 — ADR-0006).

## Accent

- Default: OS accent. Override: `#RRGGBB`/`#AARRGGBB` in Settings.
- Applied **after** every theme apply (theme apply resets accents), and
  re-applied on `ApplicationThemeManager.Changed` (OS switches reset them
  too; accent updates raise no event, so no recursion).
- Text-on-accent flips automatically (upstream brightness threshold).
- Under high contrast, a custom accent still applies — keeping the default
  (system) is recommended for HC users; author responsibility, documented.

## Contrast pairs (designed)

| Foreground | Background | Source |
|---|---|---|
| `AppTextPrimary/Secondary` | surfaces, cards, dialogs | WPF-UI text fills |
| `AppTextOnAccent` | `AppAccent` fills | auto-flipped by accent manager |
| `AppError` text | `AppErrorBackground` / surfaces | status fills |
| Focus ring | any surface | `AppFocusOuter/Inner` (light + dark ring) |

Custom accents should be checked against body text at 4.5:1 before shipping
as a default; the override is user choice and clearly labeled.
