# ADR-0006: Settings, theme, and backdrop services + fallback policy

- Status: Accepted
- Date: 2026-10-14
- Milestone: 0.2.0

## Context

0.2.0 needs system/light/dark switching, a Mica prototype with opaque
fallback, and persisted shell preferences — all testable without a running
WPF application.

## Decision

- `ISettingsService` (`SettingsService`): JSON at
  `%AppData%/Wpf11AppTemplate/settings.json`, injectable path. Missing/
  corrupt files → defaults (startup never fails on settings); first launch
  persists the file back.
- `IThemeService` (`ThemeService`) over an `IThemeApplier` seam
  (`WpfUiThemeApplier` calls `ApplicationThemeManager` statics). System mode
  resolves the OS theme explicitly so the persisted backdrop choice is
  honored (`ApplySystemTheme()` would force Mica).
- `IBackdropService` (`BackdropService`): Mica iff enabled + supported +
  not high-contrast, else `None`. Support/high-contrast checks are injected
  delegates (unit-testable, no HWND).
- **Backdrop timing rule** (verified by a real crash): `FluentWindow` applies
  `WindowBackdropType` itself at `OnSourceInitialized`. Set the DP **once in
  the window constructor** (pre-init). Never re-set it afterwards —
  re-setting post-init crashes in `SetWindowChrome` (Freezable context
  `ArgumentException`) on .NET 10. Runtime material changes go through
  `ApplicationThemeManager.Apply(theme, backdrop)`, which is a DWM-only path
  (`WindowBackgroundManager`: `RemoveBackdrop`/`ApplyBackdrop`, no
  `WindowChrome` touch) — verified against 4.3.0 sources.
- `SystemThemeWatcher.Watch(window)` in System mode (single main window;
  no unwatch needed for app lifetime).
- Startup order: load settings → persist → apply theme → create window
  (ctor chrome) → navigate Home → show.

## Evidence

- Crash reproduced (`0xC000041D` via `ApplyChrome` in `Loaded`) with full
  stack; fixed by ctor-time DP; verified by relaunch matrix (Mica default,
  persisted off/dark/rail → opaque, no crash).
- 4.3.0 sources read: `FluentWindow.cs` (`OnBackdropTypeChanged`,
  `SetWindowChrome`), `ApplicationThemeManager.cs` (`Apply`,
  `ApplySystemTheme`), `WindowBackgroundManager.cs` (`UpdateBackground`).
- 13 new unit/integration tests green (incl. `WpfFact` STA coverage).

## Consequences

- Theme/backdrop mapping is unit-tested via fakes; window chrome stays in
  code-behind (view boundary). No new packages for services
  (`System.Text.Json` is in-box).

## Revisit triggers

Token/density/accent systems (0.3.0) build on `IThemeService`; backdrop
rules extend (never bypass the ctor-time DP rule without re-validation).
