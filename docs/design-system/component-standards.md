# Component standards

Applies to every control/page added from 0.3.0 onward (full control coverage
lands in 0.4.0; this is the contract those controls must meet).

## Anatomy

- View + ViewModel + page registration (`AppComposition`) + nav item where
  top-level. Pages implement `INavigableView<T>`; VMs implement
  `INavigationAware`.
- `DataContext = this` on the view; bind through `ViewModel.*`.
- Code-behind: lifecycle, event routing, chrome only. No business logic.

## States (every interactive control)

- Default, hover/pressed (inherited from WPF-UI control templates),
  **disabled** (`IsEnabled=False` must remain legible),
  **error** (`AppErrorText` + `AppErrorBackground` where applicable),
  **empty** (for lists/content — from 0.4.0/0.6.0),
  **focus-visible** (default Fluent ring; never suppressed without a
  documented replacement).

## Keyboard and automation

- Every interactive element sets `AutomationProperties.Name`; inputs add
  `HelpText` where the purpose isn't obvious.
- Logical tab order: navigation → header → content top-to-bottom.
- No keyboard traps; dialogs (0.4.0) return focus to the invoker.

## Styling rules

- Tokens only (`App*`); no hard-coded colors, margins, fonts, or radii
  except chrome configuration with a comment.
- Prefer WPF-UI controls; custom templates alias token brushes with
  DynamicResource; test in light + dark (+ HC argument) before merge.

## Documentation format (per component, from 0.4.0)

Purpose · variants/props · states matrix · accessibility notes ·
theme/token usage · removal/extension guidance · living sample location.
