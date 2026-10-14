# Windows 11 WPF Template — Product and Release Roadmap

> **Status:** `1.0.0` stable — released 14 October 2026 ([release](https://github.com/Fettah010/wpf-11-app-template/releases/tag/v1.0.0))  
> **Research snapshot:** 13 October 2026  
> **Target:** Windows 11 desktop applications, initially on .NET 10, with future .NET upgrades designed in  
> **Project identity:** wpf-11-app-template (`Fettah010/wpf-11-app-template`); independent of the existing DevTem WinUI 3 repository  
> **Primary UI foundation:** WPF UI (`lepoco/wpfui`)  
> **Architecture:** CommunityToolkit.Mvvm + Microsoft.Extensions.Hosting / dependency injection

## 1. Product vision

Build a polished, reusable WPF application foundation that combines the practical strengths of modern Windows Fluent interfaces with the flexibility of WPF: a high-quality control library, rich but restrained animation, adaptable application shells, useful production services, and scaffolding that lets developers start either with a clean application or a feature-rich baseline.

The project is **both**:

1. A **WPF application template** with clean, recommended, and full-feature presets.
2. A **reusable UI foundation** containing the project's own design tokens, controls, layouts, animation helpers, window integrations, and documented patterns.

Its distinguishing value should be quality and coherence, not the number of NuGet packages installed. It must be easy for a developer or coding agent to understand, customize, remove optional features from, test, and ship.

### Product principles

- **Windows 11 first:** Fluent visual language, correct system window behaviour, polished title bars, sensible system integration, and excellent DPI scaling.
- **Premium, not noisy:** Native-feeling foundations with selective futuristic accents. Avoid a generic mobile-card aesthetic, excessive gradients, permanent glow, or animation for its own sake.
- **Motion is a system:** Reusable presets and consistent timing/easing; support reduced motion and low-power fallback paths.
- **Effects are optional:** Mica/acrylic-like backdrops, water-warp/ripple distortion, animated backgrounds, and other expensive effects can be disabled independently.
- **Quality before quantity:** A smaller set of excellent, accessible controls is better than a huge inconsistent gallery. Expand coverage release by release.
- **Production-ready but removable:** Logging, SQLite, updates, tray support, localization, notifications, backups, diagnostics, and release automation should be selectable modules, not hard dependencies of every scaffold.
- **Clean starts remain clean:** A generated project must be able to omit the sample dashboard, gallery content, and optional services without manual source-file surgery.
- **Performance is a feature:** Low-spec Windows 11 machines must remain usable. Rich effects may enhance capable devices but must never be required to use the app.
- **Agent-friendly:** Predictable project structure, clear conventions, explicit architectural decisions, repeatable CLI workflows, templates, examples, and verifiable acceptance criteria.
- **Open and maintainable:** Prefer permissive open-source packages, keep dependencies purposeful, document license and maintenance checks, and avoid unnecessary forks.

## 2. Decisions already made

| Area | Decision |
|---|---|
| OS support | Windows 11 only; design OS-facing integrations behind adapters where practical. |
| Framework | Target .NET 10 initially. Keep the solution upgradeable and test future .NET SDKs before adopting them. |
| MVVM | CommunityToolkit.Mvvm. |
| Dependency injection | Microsoft.Extensions.Hosting and Microsoft dependency-injection/configuration/logging abstractions. |
| Main UI toolkit | WPF UI as the primary Fluent/WPF component foundation. |
| Visual direction | Premium Fluent, with selected futuristic effects and experimental visual treatments. |
| Layouts | Navigation rail and expandable sidebar are first-class; ship multiple shell presets. |
| Animation | A reusable, configurable motion system, with performance-aware fallback behaviour. |
| Components | Broad coverage, prioritizing high-quality essentials before advanced components. |
| Example app | Premium dashboard and gallery, isolated so they can be excluded or deleted safely. |
| Services | Carry forward the current WinUI template's production capabilities and improve modularity and validation. |
| Distribution | Support portable/installed, Velopack update flows, and MSIX/Store paths as separate, tested configurations. |
| Scaffolding | CLI template, page/control generators, presets, and Visual Studio integration where feasible. |
| Quality | Automated unit, integration, generated-template, UI, accessibility, packaging, and performance checks. |
| Dependencies | Prefer maintained, permissively licensed packages; review direct and transitive dependencies. |
| Repository | Separate WPF repository and release identity. |

## 3. Research: existing libraries and starter approaches

The goal is to reuse mature foundations and build a coherent layer around them, rather than combining several overlapping design systems.

| Project | What it contributes | Decision for this project |
|---|---|---|
| [WPF UI](https://github.com/lepoco/wpfui) | Windows-inspired Fluent styles, navigation, dialogs, snackbars, `NumberBox`, title-bar/window features, tray support, and a gallery. Its 4.3.0 release is the current release found in this research pass and the project uses MIT licensing. | **Primary UI dependency.** Audit its public APIs and supported .NET targets before implementation. Extend through resource dictionaries, adapters, and composition wherever possible; do not create a private fork without a documented blocker. |
| [ModernWPF](https://github.com/Kinnara/ModernWpf) | Fluent styles and WinUI-inspired controls such as navigation, dialogs, `InfoBar`, `NumberBox`, and repeater-style controls. Its 1.x line is in release-candidate/preview development in this research snapshot. | Evaluate individual gaps and API direction. Do not add beside WPF UI by default because styling, resource keys, and controls overlap. Re-evaluate stable releases at dependency-audit milestones. |
| [MaterialDesignInXamlToolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) | Broad control gallery, themes, dialogs, cards, icons, and transitions; release 5.3.2 was found in this research pass. | Useful as a control-gallery and interaction reference, but not the visual foundation: the target aesthetic is Windows Fluent rather than Material. Add a dependency only for a proven gap that cannot be implemented cleanly in the project's UI layer. |
| [MahApps.Metro](https://github.com/MahApps/MahApps.Metro) | Metro-style windowing, flyouts, dialogs, controls, and theme approaches; its release history includes .NET 10-related fixes. | Reference for specific shell/window patterns only. Avoid combining its window/control theming with WPF UI unless a concrete compatibility spike proves a unique need. |
| [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) | Lightweight, source-generator-based MVVM support; version 8.4.2 is the latest stable release surfaced in this research pass. | Adopt. Keep ViewModels testable and avoid unnecessary application-framework abstractions. |
| [Microsoft.Extensions.Hosting](https://www.nuget.org/packages/Microsoft.Extensions.Hosting/) | Generic Host, DI, configuration, and logging infrastructure; stable 10.0.x packages are available. | Adopt for applications that enable the host/services preset. Ensure the clean preset does not inherit optional service dependencies unnecessarily. |
| [Microsoft XAML Behaviors for WPF](https://github.com/microsoft/XamlBehaviorsWpf) | Reusable XAML interaction behaviours; 1.1.142 is the latest release surfaced in this research pass. | Optional helper, not the animation engine. Adopt only where it reduces code-behind without obscuring behaviour or creating fragile XAML. |
| [Velopack](https://github.com/velopack/velopack) | Installer/update infrastructure suitable for desktop applications. | Candidate for installed/portable update channels; validate current release, licensing, framework support, signing, differential updates, and rollback before adoption. |
| [LiveCharts2](https://github.com/Live-Charts/LiveCharts2) | Open-source charting with WPF support and MIT licensing. | Optional chart adapter/sample. Keep charts out of the core UI package unless a real chart-control need is justified. |
| [FlaUI](https://github.com/FlaUI/FlaUI) | Windows desktop UI automation and interaction testing. | Candidate for smoke/regression automation. Verify project health, runtime compatibility, and test reliability before making it a required gate. |
| [Microsoft Windows App SDK samples](https://github.com/microsoft/WindowsAppSDK-Samples) | Reference implementations for Windows materials, notifications, and native Windows features. | Use as a reference when implementing optional OS integrations. Do not add Windows App SDK/WinUI dependencies to the core merely to imitate one visual feature. |

### Research conclusions

1. **WPF UI is the best starting point for the requested direction**, but it does not remove the need for a project's own design system, layout presets, motion conventions, production services, and scaffold generator.
2. **Do not merge complete UI suites by default.** WPF UI, ModernWPF, MahApps, and Material Design overlap. Layering multiple competing theme/control systems is likely to increase resource conflicts, complexity, and visual inconsistency.
3. **The main differentiator should be the layer above the UI toolkit:** polished layouts, custom controls where justified, optional effects, strong defaults, and working examples.
4. **Water-warp/backdrop distortion is an experimental effect, not a default background.** WPF does not automatically inherit WinUI's rendering/compositor model. Prototype a bounded, optional implementation, measure it, and keep a static fallback. Do not ship a permanent GPU-heavy shader simply because it looks impressive on one machine.
5. **Pin versions only after a fresh compatibility and license audit.** Package versions change; every major upgrade should be verified with a real consumer build, generated templates, theme tests, and a sample-app smoke test.

## 4. Proposed high-level solution structure

Names are placeholders and can be finalized when the project is branded. Do not create a separate assembly for every feature; each assembly must represent a meaningful dependency boundary.

```text
repo/
├─ src/
│  ├─ <Project>.UI/                 # Design tokens, styles, reusable controls and layouts
│  ├─ <Project>.Windows/            # Window chrome, OS integration, backdrop/effect adapters
│  ├─ <Project>.App/                # Reference gallery/demo application
│  └─ <Project>.Services/           # Optional service implementations and integration adapters
├─ templates/
│  ├─ clean/                        # Minimal WPF app scaffold
│  ├─ recommended/                  # Production-friendly shell and selected services
│  └─ full/                         # Full sample/gallery and all supported options
├─ tests/
│  ├─ Unit/
│  ├─ Integration/
│  ├─ TemplateSmoke/
│  ├─ UIAutomation/
│  └─ Performance/
├─ docs/
│  ├─ decisions/                    # ADRs and dependency evaluations
│  ├─ design-system/                # Tokens, themes, motion, component standards
│  ├─ guides/                       # Quick start, customization and feature guides
│  ├─ template/                     # Presets, CLI flags and page-generation guide
│  └─ release/                      # Packaging, signing and update workflows
├─ eng/                             # Build, validation, release and repository scripts
├─ Directory.Packages.props         # Central package version management
├─ global.json                      # Deliberate SDK baseline; avoid accidental SDK drift
└─ README.md
```

### Architectural boundaries

- **UI library:** reusable XAML, controls, themes, tokens, motion helpers, and UI abstractions. No app-specific business logic, network clients, SQLite databases, update engine, or authentication.
- **Windows integration layer:** title bar/window behaviour, native backdrop adapters, notifications, tray, startup behaviour, and other OS-specific operations. Every optional effect or integration must have a fallback or a clear unsupported state.
- **Sample/gallery app:** a reference implementation, not a mandatory dependency of generated applications. Dashboard, diagnostic demos, and gallery pages must be isolated from the default clean shell.
- **Services:** production integrations registered through interfaces and dependency injection. Feature selection must remove unused code and packages from generated projects wherever practical, not merely hide controls.
- **Templates and generators:** the source of truth for generated output. Generated projects must build independently from the repository and must not rely on absolute paths or hidden local tooling.

## 5. Design system and UI scope

### 5.1 Visual language

Use Windows Fluent as the foundation with a premium, restrained futuristic layer. Focus on typography, spacing, hierarchy, motion, and material quality before adding decorative effects.

- **Theme tokens:** semantic colors, surface levels, border/outline, typography, spacing, radii, shadows/elevation, icon sizes, and motion values.
- **Theme modes:** system, light, dark, and high-contrast compatibility; optional accent-color override and a compact-density mode.
- **Surface styles:** standard Fluent surfaces by default; optional elevated, translucent, gradient, and experimental surfaces.
- **Window experience:** custom title bar where appropriate, system-compatible minimize/maximize/close controls, resizing, snapping, drag regions, keyboard behaviour, and supported Mica/acrylic-like materials.
- **Responsive layouts:** resize-aware navigation, cards and grids; avoid hard-coded desktop-only dimensions and test at common Windows display scales.
- **Icons:** use a consistent open icon source with reviewed licensing. Do not redistribute Windows-bundled fonts or glyphs unless license terms permit it.

### 5.2 Shell and page templates

Ship multiple composable shell presets instead of forcing all apps into a single layout.

- **Navigation rail:** compact icon-led shell; expandable/collapsible state and selected-item indication.
- **Expandable sidebar:** full navigation labels, nested sections, optional footer/status areas, and compact mode.
- **Top navigation:** optional shell for simpler, focused apps.
- **Tabbed workspace:** document-oriented shell for multiple open workspaces or files.
- **Split-view/master-detail:** navigation/list alongside a detail surface.
- **Command-oriented shell:** optional command bar and keyboard-first actions.

Each shell must use common navigation contracts, settings, theme services, and keyboard/focus behaviour. The same page should not need to be rewritten to work in the rail and sidebar variants.

### 5.3 Component roadmap

Build a **working component gallery** as each component family is delivered. Priorities below are intentional; advanced controls should not delay the quality of everyday elements.

**Core controls**
- Buttons: primary, secondary/subtle, outline, icon, split/dropdown, toggle, loading, and destructive variants.
- Text and form controls: text/password/search fields, autocomplete, number input, combo/select, checkbox, radio, switch, slider, date/time/color inputs where appropriate, validation states, helper text, and form layouts.
- Navigation: nav items, breadcrumbs, tabs, menu/command bar, pagination, and selection indicators.
- Content: cards, grouped sections, separators, badges, chips, avatars, icons, tooltips, contextual menus, expanders, and accordions.
- Feedback: dialogs, content dialogs, flyouts, popovers, snackbar/toast patterns, progress indicators, loading skeletons, empty/error/success states, and confirmation flows.

**Data and advanced layouts**
- List and detail patterns, selectable/virtualized lists, table/DataGrid styling, sorting/filtering/search, tree views, grid/card catalog pages, key-value details, and paging where needed.
- Adaptive grids, resizable split panes, tabbed documents, wizard/step flows, settings sections, master/detail pages, and content containers.
- Optional chart module using a vetted chart library; charts must be isolated from the mandatory UI package.
- Optional drag-and-drop patterns and richer workspace layouts after core accessibility and performance gates pass.

Do not attempt to fork or reimplement controls that the selected toolkit already provides well. Add custom implementations where there is a demonstrable gap in appearance, capability, consistency, or integration.

### 5.4 Motion and effects

Create a coherent motion system rather than independent animations scattered across XAML.

- **Motion presets:** instant, subtle, standard, and expressive; shared durations, easing curves, and distance limits.
- **Interaction motion:** hover, press, focus, toggles, selection, expand/collapse, progress, loading, and state changes.
- **Navigation motion:** controlled fade/slide transitions, content entry, and optional staggered sequences.
- **Overlay motion:** dialogs, flyouts, menus, tooltips, snackbars, and drawers with consistent entry/exit timing.
- **Layout motion:** animate layout changes only when they improve comprehension and can be done without destabilizing measurement or scrolling.
- **Reduced motion:** honour Windows animation/accessibility preferences and expose an app-level setting when appropriate.
- **Performance adaptation:** `Auto` mode should reduce or skip expensive transitions based on available rendering capability, user settings, and observed performance. Never delay an essential action just to finish an animation.

**Optional premium effects:** supported window materials, subtle animated gradient surfaces, reveal/highlight effects, parallax in selected content, and experimental water-warp/ripple distortion. Keep these opt-in per shell/surface. Water warp belongs in a dedicated effects demo behind a feature switch; it must have a flat/static fallback, a reduced-motion mode, and a low-resource fallback. Do not make the whole application depend on a shader or transparency effect.

## 6. Production capabilities

The full preset should provide a stronger foundation than the existing WinUI template while allowing each capability to be selected independently.

| Capability | Direction |
|---|---|
| Application startup and DI | Generic Host, explicit service registrations, clean lifetime/shutdown, and predictable async startup. |
| Configuration and settings | Typed configuration, persistent user preferences, theme/density/effects state, and safe defaults. |
| Logging and diagnostics | Structured file logging, sensible retention, log levels, diagnostic status, filtered log viewer, and exportable support bundle. No telemetry by default. |
| Updates | Velopack-backed installed-app update flow where selected; channels, download/progress states, restart prompts, and explicit behaviour for unsupported/uninstalled development builds. |
| Distribution | Separate validated profiles for portable/installed distribution and MSIX/Store workflows; avoid confusing update mechanisms across package types. |
| Tray and notifications | Optional system tray integration, in-app notifications always available, and system notifications only where the selected distribution supports them. |
| Localization | Resource-based strings, culture selection, at least a clear English baseline, and a documented path to additional languages. No UI text hard-coded into control templates. |
| Data | Optional SQLite module with migrations, data access abstractions, and clear user-data location rules. No database in a clean scaffold unless selected. |
| HTTP | Optional typed clients, cancellation, timeout/error handling, configuration, and resilient service boundaries. No remote calls by default. |
| Backup/restore | Optional export/import and backup capability, with explicit paths, validation, and failure handling. |
| Crash and health reporting | Local diagnostics first. External reporting is opt-in, disabled without configuration, and disclosed in documentation. |
| Authentication | Optional sample integration only if license, maintenance, security, and configuration burden are justified; off by default and removable. |
| Release automation | GitHub Actions build/test/package workflow, version/tag conventions, release notes, artifact checksums, signing guidance, and clean reproducible build steps. |
| Installer UX | Professional install/uninstall experience where applicable, install location/shortcut choices if supported by the selected installer, and no custom installer UI unless it adds measurable value. |

The full preset must not silently enable telemetry, sign users in, open a network connection, or create scheduled/background behaviour without disclosure. Optional features need clear enablement flags and documentation.

## 7. Template presets and developer experience

Use one well-tested template engine and a small set of coherent presets. Prefer `dotnet new` for reproducible command-line scaffolding and add Visual Studio integration when it can be maintained without duplicating template logic.

### Presets

- **Clean:** minimal WPF app, navigation/shell choices only if selected, basic theme, MVVM, and no sample dashboard, database, updater, tray service, diagnostics UI, telemetry, or release infrastructure unless requested.
- **Recommended:** polished shell, theme/settings foundation, logging, diagnostics, standard error/empty/loading patterns, tests, and selected production conventions.
- **Full:** recommended foundation plus the premium demo dashboard, UI gallery, optional service modules, distribution options, and full release automation.
- **Gallery/demo:** the reference showcase application that demonstrates the complete UI system and optional visual effects. It is not a requirement for product projects.

### Scaffold options

Expose discoverable options for project/namespace identity, shell type, theme defaults, sample content, UI-library inclusion, logging, localization, SQLite, HTTP clients, updates, tray/notifications, backup, diagnostics, packaging/distribution, test project, and release workflows.

Example conceptual CLI surface (final names should be validated with the template engine):

```powershell
dotnet new <template> -n MyDeskApp --preset clean --shell rail --sample-content none
```

### Page/control generators

Provide supported commands or scripts to add common patterns: a basic page, settings page, list/detail page, data grid page, card/catalog page, tab/workspace page, service, ViewModel, custom control, and test. Generators should follow the same naming, DI, navigation, styling, and testing conventions as hand-written code.

Every option must be represented in template metadata/help and tested through actual generated-project builds. If an option selects a feature off, remove its implementation and dependency from the scaffold where practical; do not leave broken references or dead folders behind.

## 8. Release roadmap: 0.1.0 → 1.0.0

Every milestone adds usable value while keeping the reference app buildable. Release numbers describe delivered capability, not a fixed calendar. Each version must update the changelog, documentation, and acceptance checklist.

| Version | Milestone focus |
|---|---|
| `0.1.0` | Repository foundation, architecture decisions, minimal runnable app |
| `0.2.0` | Windows 11 window experience, navigation rail, expandable sidebar |
| `0.3.0` | Design tokens, themes, component standards |
| `0.4.0` | High-quality essential controls and forms |
| `0.5.0` | Motion system, performance fallbacks, optional visual effects |
| `0.6.0` | Advanced layouts, data controls, premium dashboard and gallery |
| `0.7.0` | Modular production services, integrations, distribution workflows |
| `0.8.0` | `dotnet new` scaffolding, presets, generators, developer tooling |
| `0.9.0` | Feature-complete beta, accessibility and performance hardening |
| `1.0.0-rc.1` | Public-surface freeze and release-candidate validation |
| `1.0.0` | Stable template and reusable UI foundation |

### `0.1.0` — Foundation and architecture decisions

**Goal:** establish a buildable repository and prevent costly architectural ambiguity.

- Create solution structure, project conventions, central package management, SDK policy, analyzers, formatting rules, and license/notice handling.
- Build a minimal WPF app on .NET 10 for Windows 11 with CommunityToolkit.Mvvm and a deliberately small dependency set.
- Prototype WPF UI integration, window startup, theme resource loading, navigation, and a generated consumer app.
- Record architecture decisions for UI foundation, package boundaries, native window integration, effects, distribution, and testing.
- Define the clean/recommended/full scaffold contract and performance measurement method.

**Exit criteria:** clean restore/build/run on a fresh Windows 11 development environment; no unexplained dependency overlap; first package/license inventory; one smoke test; initial documented performance baseline.

### `0.2.0` — Windows 11 shell and window experience

**Goal:** deliver a polished application frame before expanding controls.

- Implement title-bar/window integration and common shell layout contracts.
- Ship the first navigation-rail shell and expandable-sidebar shell.
- Add selection state, back/navigation behaviour where applicable, focus/keyboard rules, responsive sidebar collapse, and page lifecycle conventions.
- Add system/light/dark theme switching and persistent shell preferences.
- Prototype supported native window material behind a feature switch with a dependable opaque fallback.

**Exit criteria:** both shells work at common window sizes and display scales; window controls, resizing, keyboard navigation, and theme changes are tested; the shell works with effects disabled.

### `0.3.0` — Design tokens, themes, and UI standards

**Goal:** make every later control consistent and customizable.

- Establish semantic color, typography, spacing, corner, border, elevation, icon, and motion tokens.
- Standardize light/dark/system themes, high-contrast behaviour, compact density, and accent override.
- Define shared resource dictionary layers and override conventions without patching upstream resources in fragile ways.
- Establish component anatomy, focus/disabled/hover/pressed/error states, and documentation format.
- Add a small style/token preview page to the gallery.

**Exit criteria:** tokens are centrally defined; themes can be changed at runtime; core components do not use unexplained hard-coded style values; contrast and focus states are documented and checked.

### `0.4.0` — Core controls and polished forms

**Goal:** cover high-frequency product UI with production-quality components.

- Refine buttons, icon buttons, toggles, text/search inputs, selection controls, menus, tooltips, badges, chips, cards, expanders, and grouped content.
- Standardize validation, disabled/busy states, keyboard focus, command binding, and accessibility metadata.
- Add dialogs, snackbar/notification patterns, confirmation flows, empty states, error states, progress, and basic loading indicators.
- Add a working gallery section with interactive examples, variants, usage notes, and copyable snippets.

**Exit criteria:** each core component has a documented sample; major states are visually verified in light/dark and high-contrast contexts; input and command behaviour is testable without code-behind hacks.

### `0.5.0` — Motion system and optional visual effects

**Goal:** make the UI feel premium while keeping animation predictable and efficient.

- Deliver motion presets, easing/duration tokens, interaction transitions, expand/collapse, navigation transitions, overlay transitions, and selection/progress feedback.
- Implement reduced-motion handling and `Auto`/`Balanced`/`Rich` motion profiles.
- Add capability-aware fallback behaviour and ensure animations do not block input or essential state updates.
- Prototype optional backdrop/material adapters, subtle animated surfaces, and the water-warp/ripple showcase.
- Keep effects independent and switchable; document when a feature is experimental or unsupported.

**Exit criteria:** motion is consistent across core controls; reduced motion works; all expensive effects can be disabled; water warp is not enabled by default and passes agreed low-spec/performance checks before release.

### `0.6.0` — Advanced layouts, data controls, and sample content

**Goal:** support more than simple form applications.

- Add top-navigation, tabbed workspace, and split-view/master-detail shell presets.
- Add list/detail, card/catalog, settings-section, grid/table, tree/list, paging/search/filtering, and wizard/step-flow patterns as prioritized by real examples.
- Add optional charting and drag-and-drop samples behind separate dependencies/modules.
- Build the premium dashboard and a browsable control gallery with realistic sample data, responsive layouts, useful empty states, and convincing visual hierarchy.
- Isolate sample pages/content so that the dashboard can be excluded through scaffold options and removed without breaking the shell.

**Exit criteria:** each layout can be selected intentionally; generated pages follow common conventions; large lists use virtualization where appropriate; gallery content is modular and not coupled to services required by clean apps.

### `0.7.0` — Production services and Windows integrations

**Goal:** bring the app infrastructure up to or beyond the existing WinUI template, as optional modules.

- Add host/DI lifecycle, typed configuration, structured logging, diagnostics page, log filtering/export, and local support bundle.
- Add optional settings persistence, localization, SQLite, typed HTTP clients, tray support, in-app/system notification adapters, backup/restore, and optional authentication/reporting samples when approved by the dependency/security audit.
- Integrate Velopack update flow for supported installed distributions, including progress, channels, restart UX, failure paths, and non-installed-development behaviour.
- Establish distribution profiles for portable/installed and MSIX/Store paths, avoiding unsupported combinations by design.
- Add release workflow, version/tag rules, artifact verification, and signing documentation.

**Exit criteria:** each module can be enabled/disabled independently where promised; the clean preset remains free of unselected service dependencies; update/package combinations are tested rather than merely documented; telemetry is off unless explicitly configured.

### `0.8.0` — Template generator and developer tooling

**Goal:** make it easy to create, extend, and maintain real applications from the foundation.

- Publish `dotnet new` templates with clean, recommended, full, and gallery presets.
- Add validated flags for shell, dashboard/sample content, theme, service modules, distribution, tests, and release pipeline.
- Add page/control/service/ViewModel/test generators for the most common project patterns.
- Add Visual Studio project-template integration if it can use the same template source and pass the same tests.
- Generate app-specific README, feature manifest, configuration examples, and next-step instructions.
- Add version reset/rebrand support, initialization/validation scripts, and a troubleshooting guide.

**Exit criteria:** every supported preset and feature combination selected for 1.0 generates a standalone application that restores, builds, starts, and passes smoke tests; generated output contains no absolute paths, template placeholders, missing files, or unused required dependencies.

### `0.9.0` — Beta, full validation, and performance hardening

**Goal:** feature-complete beta; spend effort on reliability rather than adding speculative features.

- Freeze the intended 1.0 feature scope and mark deferred features explicitly.
- Test the reference app and generated templates across all supported shell, theme, service, and distribution combinations.
- Run UI automation for navigation, dialogs, settings, window controls, theme changes, updates, and shutdown.
- Validate keyboard-only navigation, focus visibility, high-contrast behaviour, reduced motion, and DPI scaling.
- Profile startup, memory, navigation, scrolling, large data sets, animations, and idle behaviour on both modern and low-spec Windows 11 machines.
- Remove avoidable allocations, redundant rendering, heavy defaults, unnecessary package references, and fragile resource overrides.
- Complete package/license/transitive dependency, security, and release-process audits.

**Exit criteria:** no known critical defects; performance budgets and test environments are published; all required gates are reproducible; README and quick-start instructions have been verified on a clean machine.

### `1.0.0-rc.1` — Release candidate and API freeze

**Goal:** prove the 1.0 candidate can be adopted without last-minute architecture changes.

- Freeze public UI APIs, template option names, shell contracts, configuration shape, and generated project structure except for critical fixes.
- Publish a migration/upgrade policy and a list of supported extension points.
- Run the full release gate from a clean checkout; verify packages, installers, checksums, update channels, MSIX artifacts, and generated consumer applications.
- Perform manual visual review of every shell, key component family, theme, fallback mode, and optional effect.
- Address release-blocking defects only; any public-surface change restarts the candidate validation cycle.

**Exit criteria:** all 1.0 release gates pass; release artifacts are reproducible; all dependencies have approved licenses and maintenance status; no blocking defect remains.

### `1.0.0` — Stable release

**Goal:** provide a dependable Windows 11 WPF foundation that developers can safely start projects with.

- Publish stable reusable UI library/package(s) and template package(s) with clear versioning and a license.
- Publish the gallery/reference app, documentation, feature matrix, architectural decision records, package inventory, and setup guide.
- Provide documented clean, recommended, full, and gallery flows; validated page generators; tested packaging/update instructions; and support/troubleshooting guidance.
- Commit to semantic versioning, changelog discipline, dependency reviews, regression checks, security handling, and documented deprecation/migration policies.

**1.0 success definition:** developers can install the template, generate a clean or full application, customize the design system and shell, add pages through documented patterns, run tests, and ship a supported distribution without depending on local repository paths or undocumented manual steps.

## 9. Quality, performance, and release gates

Performance and quality must be tested and verified from the start, not added just before release.

### Performance strategy

- Establish reproducible measurements in `0.1.0`, and keep tracked baselines for cold startup, first interactive frame, memory after startup, navigation latency, list scrolling, and animation cost.
- Maintain a **low-spec reference environment** representative of a Windows 11 PC with a low-power CPU, 8 GB RAM, integrated graphics, and SSD. Also test a modern reference machine.
- Use a default `Auto` profile that selects a sensible level of visual richness; provide `Balanced` and `Rich` options without making users understand graphics APIs.
- Prefer lightweight WPF storyboards/transforms for common transitions. Avoid perpetual animations, repeated layout invalidation, oversized blur regions, and unnecessary transparency layers.
- Virtualize long lists/tables and avoid loading hidden sample content or expensive resources on startup.
- Use hardware/rendering capability as one input, not the only input. Allow users to disable effects, and fail safely if a backdrop/shader API is unavailable.
- Set numerical budgets from the first reproducible baseline and ratify them before `0.4.0`. Report median and worst-case results; do not claim support based only on a high-end developer PC.

### Required automated checks by 1.0

- Clean restore/build/test from a clean checkout using documented SDKs.
- Unit tests for ViewModels, services, configuration, option handling, and business-free UI logic.
- Integration tests for DI setup, persistence when enabled, update state handling, and packaging-specific behaviours.
- Template smoke tests covering each supported preset and every supported feature combination chosen for the release matrix.
- Windows UI automation for the most important navigation, settings, dialog, notification, and update workflows.
- Visual regression snapshots for primary shells, light/dark themes, responsive/collapsed navigation, and high-risk component families; snapshots support review and do not replace functional testing.
- Accessibility checks for keyboard/focus, automation names, contrast/high-contrast behaviour, and reduced motion.
- Performance tests for startup, large lists, navigation, idle CPU/memory, and each optional effect on reference hardware.
- Packaging tests for every supported portable/installed/MSIX/Store workflow, plus install, update, rollback/recovery, uninstall, and first-run behaviour where applicable.
- Dependency/license/security audit with recorded decisions and no unexplained direct or transitive packages.

### Accessibility and fallback requirements

- All core actions must be possible by keyboard, with visible focus and predictable tab order.
- Reduced motion and high-contrast users must receive a usable experience, not a broken or empty visual design.
- Unsupported materials/effects must fall back cleanly to opaque/static surfaces.
- Effects must not reduce text readability, obscure focus, interfere with hit testing, or become the only way to communicate state.
- Shell and component layouts must tolerate common Windows scaling and resizing; avoid fixed sizes that clip content.

## 10. Dependency governance

Use central package management and keep a dependency decision record for every nontrivial direct dependency.

For each candidate, document: purpose, version and target-framework compatibility, repository activity/release cadence, issue health, license, transitive dependencies, native runtime requirements, security notices, package size/startup impact, alternatives, and removal cost.

### Preferred starting stack

| Concern | Initial candidate | Rule |
|---|---|---|
| Fluent controls/window shell | WPF UI | Primary toolkit; do not add another full styling suite by default. |
| MVVM | CommunityToolkit.Mvvm | Default application pattern. |
| DI/host/configuration | Microsoft.Extensions.Hosting and related packages | Include in app presets that require services; keep clean scaffold lean. |
| Logging | Serilog with hosting integration and file sink | Optional service module with sensible retention and no network telemetry by default. |
| Animation/behaviour helpers | WPF built-in animation primitives; XAML Behaviors only where useful | Keep common motion lightweight and testable. |
| Installer/updates | Velopack | Validate current compatibility/licensing and keep update logic out of the UI library. |
| Local database | Microsoft.Data.Sqlite | Optional, with explicit migrations and user-data paths. |
| Charts | LiveCharts2 or a proven alternative | Optional module, not part of the core install unless requested. |
| UI automation | FlaUI or a verified alternative | Validate reliability on supported Windows 11 environments. |

Do not hard-code “latest” package versions into architectural documentation. Store actual approved versions centrally and review them during scheduled dependency audits. Stable dependencies are preferred over previews; a preview can be used only when it provides essential functionality unavailable in stable packages and the risk is documented.

## 11. Agent execution rules

These rules are part of the implementation contract for coding agents working from this document.

1. **Read the roadmap and relevant ADRs before changing architecture.** Do not replace the selected UI toolkit or introduce a new application framework without a written comparison and explicit approval.
2. **Work milestone-by-milestone.** Keep the repository buildable, preserve the last passing baseline, and finish the current milestone's acceptance criteria before expanding scope.
3. **Inspect current upstream documentation before choosing package versions or APIs.** Do not copy stale examples or assume WPF and WinUI controls behave identically.
4. **Prefer composability over duplication.** Use WPF UI where it satisfies the requirement; implement a custom wrapper/control only when the gap is real and described.
5. **Do not add overlapping UI suites casually.** Any additional control/theme toolkit requires a proof-of-concept, conflict review, license check, and a recorded decision.
6. **Do not make expensive effects default.** Implement effects as independently toggleable capabilities with fallback styles and tests.
7. **No code-behind business logic.** Code-behind is acceptable for window lifecycle, native interop, and framework-specific UI behaviour when isolated and documented. Commands/state belong in ViewModels or focused services.
8. **Generated applications are first-class products.** Test the actual files produced by the template; never assume that compiling the repository proves scaffolding works.
9. **Optional means removable.** When a feature is disabled at scaffold time, remove package references and related registrations/source where promised, then build the resulting app.
10. **Keep samples disposable.** Dashboard and gallery pages may demonstrate advanced features but must not be needed by the basic shell.
11. **No silent network behaviour.** Telemetry, remote diagnostics, identity, and background network services are disabled unless the user explicitly enables/configures them.
12. **Verify, do not merely document.** Every supported install/update/distribution combination must have an automated or recorded manual proof. Mark unsupported combinations as guarded and explain why.
13. **Update docs with behaviour.** Every new public control, theme token, CLI flag, or service module requires a usage example and removal/configuration guidance.
14. **Protect the clean preset.** New features must not progressively turn the minimal scaffold into an all-on application.
15. **Avoid speculative abstractions.** Introduce public APIs only when used by the sample app or a real generated app and when their extension value is clear.

## 12. Release acceptance checklist

A milestone is not complete until all applicable items pass.

- [ ] Scope and limitations match the milestone definition.
- [ ] Repository restores, builds, and tests from a clean checkout.
- [ ] Sample/gallery demonstrates the feature in a real layout.
- [ ] Clean and recommended generated applications build and launch; the full preset is tested when its related features change.
- [ ] Themes, resizing, scaling, keyboard focus, reduced motion, and fallback behaviour are checked where relevant.
- [ ] Performance measurements are captured for new animations, effects, layouts, or service startup work.
- [ ] Dependencies and licenses are reviewed; version/source decisions are recorded.
- [ ] Documentation, changelog, configuration examples, and screenshots are updated where needed.
- [ ] No optional feature is silently introduced into the clean preset.
- [ ] No critical failure is hidden behind a catch-all handler, silent downgrade, or misleading success state.

## 13. Explicit non-goals for 1.0.0

To protect focus and reliability, 1.0.0 does not require:

- Reimplementing all WinUI 3 controls or guaranteeing pixel-perfect WinUI behaviour in WPF.
- Shipping multiple competing theme frameworks in the default scaffold.
- Making water-warp, blur, acrylic, shaders, or animated backgrounds a default experience.
- Building a business-specific dashboard that every generated app must retain.
- Bundling authentication, cloud services, telemetry, or a database into the clean preset.
- Supporting Windows versions earlier than Windows 11.
- Guaranteeing that every experimental effect runs on all GPUs; unsupported cases must use safe fallbacks.
- Adding features solely to increase the component count or to imitate another framework without a demonstrated user/developer benefit.

## 14. Reference links

Use these as starting points, not as substitutes for checking the exact package version and compatibility when implementation begins.

- Existing WinUI reference template: <https://github.com/Fettah010/winui-3-easy-template>
- WPF UI source, releases, and license: <https://github.com/lepoco/wpfui> · <https://github.com/lepoco/wpfui/releases>
- ModernWPF source and release readiness: <https://github.com/Kinnara/ModernWpf> · <https://github.com/Kinnara/ModernWpf/blob/master/docs/release-readiness-1x.md>
- MaterialDesignInXamlToolkit: <https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit>
- MahApps.Metro: <https://github.com/MahApps/MahApps.Metro>
- .NET Community Toolkit / MVVM: <https://github.com/CommunityToolkit/dotnet> · <https://github.com/CommunityToolkit/MVVM-Samples>
- Microsoft XAML Behaviors for WPF: <https://github.com/microsoft/XamlBehaviorsWpf>
- Microsoft.Extensions.Hosting: <https://www.nuget.org/packages/Microsoft.Extensions.Hosting/>
- Serilog hosting integration: <https://github.com/serilog/serilog-extensions-hosting>
- Velopack: <https://github.com/velopack/velopack>
- LiveCharts2: <https://github.com/Live-Charts/LiveCharts2>
- FlaUI: <https://github.com/FlaUI/FlaUI>
- Windows App SDK samples (native Windows feature reference): <https://github.com/microsoft/WindowsAppSDK-Samples>

---

**Definition of success:** A developer can create either a genuinely clean WPF app or a full-featured Windows 11 desktop app from the same maintained foundation. Both feel polished, remain fast and accessible, and can be built, tested, customized, packaged, and shipped using documented workflows.
