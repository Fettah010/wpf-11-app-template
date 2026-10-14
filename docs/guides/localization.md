# Localization

Pattern: `.resx` string tables + hand-written accessor + culture setting.
Restart-applied (WPF static resources don't refresh at runtime).

## Current coverage (0.7.0)

- `src/DevTemWpf.App/Resources/Strings.resx` (English, invariant) +
  `Strings.de.resx` (German proof).
- `Resources/Strings.cs` — static accessor (no designer codegen, so plain
  `dotnet build` reproduces it). Missing keys return `!Key!` (loud, not silent).
- Code-built strings (nav items) use `Strings.*` directly.
- XAML uses `{x:Static res:Strings.*}` (`xmlns:res` namespace).
- Setting: `Culture` = `""` (system), `"en"`, `"de"`; applied at startup via
  `CultureService`; invalid names fall back to system with a log warning.
- Gallery page copy stays English — mechanical follow-up using this pattern
  (backlog entry, not a second system).

## Adding a language

1. Copy `Strings.resx` → `Strings.<bcp47>.resx` (e.g. `Strings.fr.resx`),
   translate every `<value>`.
2. Add the option to `SettingsViewModel.CultureOptions`.
3. Add a lookup test in `tests/DevTemWpf.Unit/LocalizationTests.cs`.
4. No code changes needed — satellites resolve automatically.

## Rules

- Never hard-code user-visible text in new XAML/code — add a key instead.
- Never ship a half-translated page silently: partial coverage is listed here.
- Right-to-left and CJK layout validation rides with the 0.9.0 hardening.
