# ADR-0005: Shell and navigation (FluentWindow + NavigationView)

- Status: Accepted
- Date: 2026-10-14
- Milestone: 0.2.0

## Context

Roadmap 0.2.0 needs a rail shell, an expandable sidebar, back navigation,
and page lifecycle conventions on WPF-UI 4.3.0.

## Decision

- `MainWindow : FluentWindow` + `ui:TitleBar` + one adaptive `NavigationView`.
- `ShellMode.Rail → NavigationViewPaneDisplayMode.LeftMinimal` (icons only);
  `ShellMode.Sidebar → Left`. (The tutorial page's `LeftCompact` does not
  exist in 4.3.0 — verified against the API reference; `LeftMinimal` is the
  icons-only mode.)
- Narrow windows (<800px) collapse the pane (`IsPaneOpen=false`); widening
  restores the preference. Hamburger toggling stays session-local.
- Pages are `INavigableView<T>`; ViewModels implement `INavigationAware`.
  `CacheHistory=0` (default; state lives in VMs/services).
- Item clicks and the back button are NavigationView-automatic. Initial
  navigation (`HomePage`) happens in code-behind on first `Loaded`.
- v4 navigation truth (verified against 4.3.0 assemblies + migration guide):
  `IPageService`/`PageService`/`NavigationService` implementation **do not
  ship** — the docs' MVVM snippet is stale. Pages resolve through
  `INavigationViewPageProvider`, whose only implementation ships in the
  same-vendor `WPF-UI.DependencyInjection 4.3.0` package
  (`AddNavigationViewPageProvider()` + `SetPageProviderService`), added in
  this milestone. No VM-initiated navigation service is introduced until a
  milestone needs it (dashboard deep-links, 0.6.0).

## Evidence

- API reference `NavigationViewPaneDisplayMode` (Left/LeftMinimal/LeftFluent/
  Top/Bottom); assembly metadata inspection (no `NavigationService`,
  `IPageService`, or `PageService` types in 4.3.0); v4 migration guide
  (`IPageService` → `INavigationViewPageProvider`);
  gallery `App.xaml.cs` (same-vendor DI package + own nav service).
- App launches to the shell and navigates (smoke launch check + manual
  settings-file persistence check).

## Consequences

- One justifiable same-vendor package added (MIT, 4.3.0 line).
- VM-initiated navigation deferred with a clear trigger (0.6.0 deep links).

## Revisit triggers

WPF-UI ships a navigation-service implementation, or a milestone needs
programmatic navigation from ViewModels.
