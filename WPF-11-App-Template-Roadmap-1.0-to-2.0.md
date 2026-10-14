# WPF 11 App Template — Roadmap from 1.0.0 to 2.0.0

> **Document type:** Product roadmap and version-level execution specification for coding agents  
> **Repository:** [Fettah010/wpf-11-app-template](https://github.com/Fettah010/wpf-11-app-template)  
> **Current baseline:** `1.0.0` stable, released 14 October 2026  
> **Research snapshot:** 14 October 2026  
> **Target:** Windows 11 desktop applications, initially targeting .NET 10  
> **Related governing documents:** `WPF-AGENT-EXECUTION-PROTOCOL.md`, `docs/DEFERRED.md` (legacy `WPF-Template-Roadmap.md` removed in `1.1.0`; 0.x history in `CHANGELOG.md` + `docs/releases/`)
> **Scope note:** This roadmap was prepared outside the repository. It does not change repository files, branches, tags, or releases.

---

## 1. Mission

Evolve the stable WPF 11 App Template from `1.0.0` to a substantially stronger `2.0.0` platform that combines:

1. **Premium Windows 11 UI:** an intentional Fluent visual system, exceptional core controls, multiple application shells, sophisticated but restrained motion, adaptive layouts, and an impressive living gallery.
2. **Genuinely reusable architecture:** shared UI and Windows integration libraries, separately selectable production services, and a reference/gallery app that is not a runtime dependency of generated applications.
3. **Reliable developer experience:** `dotnet new` presets and item templates, dependable generators, optional-module switches, package-based installation, clear documentation, and Visual Studio integration where its maintenance burden is justified.
4. **Evidence-based quality:** robust unit, integration, UI automation, accessibility, visual-regression, package-consumer, and performance tests—with no required check reported as successful when it did not run.
5. **Practical performance:** useful on a modest Windows 11 PC with 8 GB RAM and integrated graphics, while making higher-end effects optional and adaptive.
6. **Safe, documented evolution:** preserve the `1.x` compatibility contract; use the `2.0.0` major boundary for reviewed breaking changes, with migration guidance and consumer-project tests.

The objective is not to maximize the number of controls, dependencies, animations, or assemblies. It is to produce a cohesive foundation that developers can understand, customize, test, and ship confidently.

## 2. Reading this roadmap: evidence labels

These labels distinguish verified state from recommendations and experiments.

- **[FACT — 1.0.0]** Verified in the repository, its release/report, or the linked current source as inspected on 14 October 2026. A fact describes the current inspected state, not a guarantee that it remains current forever.
- **[RECOMMENDATION]** Proposed work or an engineering decision for the roadmap. It is not a claim that the repository already implements it.
- **[PROTOTYPE REQUIRED]** A decision must be earned through a working spike, measurements, compatibility tests, and a documented go/no-go result. A visually promising demo alone is not acceptance evidence.
- **[OPTIONAL MODULE]** Must be independently enableable/disableable and must not leak dependencies or source files into presets that do not select it.
- **[BLOCKING GATE]** A required failure or missing evidence prevents the milestone release. Do not change a test to make a failure disappear without understanding and recording the cause.

Version numbers identify delivered capability, not dates. A release may not start implementation until the current repository has been inspected and the agent has written the corresponding version plan as required by `WPF-AGENT-EXECUTION-PROTOCOL.md`.

## 3. Verified 1.0.0 baseline

### 3.1 Current product and technical state

- **[FACT — 1.0.0]** The repository has a stable `v1.0.0` GitHub release with a `DevTemWPF.Templates.1.0.0.nupkg` asset. The release body reports 83/83 tests passing. See [release](https://github.com/Fettah010/wpf-11-app-template/releases/tag/v1.0.0) and [1.0.0 report](https://github.com/Fettah010/wpf-11-app-template/blob/main/docs/releases/1.0.0-report.md).
- **[FACT — 1.0.0]** The app targets `net10.0-windows10.0.26100` and is Windows 11-first. It uses WPF UI 4.3.0, CommunityToolkit.Mvvm 8.4.2, Microsoft.Extensions.Hosting 10.0.12, xUnit v3/Microsoft Testing Platform mode, FlaUI 5.0.0, and supporting modules listed in `Directory.Packages.props` and `docs/DEPENDENCIES.md`.
- **[FACT — 1.0.0]** The current reference app project is `src/DevTemWpf.App`. It contains the app shell, design token dictionaries, service implementations, ViewModels, and gallery pages in one WPF application project. The template includes Clean, Recommended, and Full presets, plus page and service item templates.
- **[FACT — 1.0.0]** Current example pages include Home, Controls, Design Tokens, Effects, Settings, Diagnostics, and a Full-preset Dashboard. The Effects page demonstrates animation presets, a ripple surface, and an animated gradient; it does not establish that an HLSL water-warp shader is production-ready.
- **[FACT — 1.0.0]** There are unit and integration tests, FlaUI UI automation tests, `eng/smoke.ps1`, startup/idle measurement scripts, and a template matrix script.
- **[FACT — 1.0.0]** The current GitHub Actions CI workflow uses `windows-latest` and runs restore, Release build with warnings as errors, tests, package inventory, and the **Clean preset** template gate. It does not currently run the full named-preset matrix in that workflow.
- **[FACT — 1.0.0]** `ShellFlowsTests.cs` can report UI tests as skipped when its opt-in environment configuration is not enabled. Therefore, a generic successful `dotnet test` result does not, by itself, prove the UI automation cases actually ran.
- **[FACT — 1.0.0]** `eng/test-templates.ps1` supports Clean, Recommended, and Full, but its setup modifies the machine's `dotnet new` template registration. It also uses shared TEMP output filenames. Isolating the test CLI home and per-run logs is a reliability improvement worth implementing.
- **[FACT — 1.0.0]** `eng/measure-startup.ps1` resolves the app executable through a fixed project/output path; that assumption must be removed or made configuration-aware before the solution is split into multiple projects or generated project names change.
- **[FACT — 1.0.0]** `README.md` has a stale screenshot placeholder saying the gallery arrives in `0.6.0` even though `0.6.0` and `1.0.0` have shipped, and its roadmap table repeats the stable `1.0.0` row with inconsistent status. This is a confirmed documentation defect.
- **[FACT — 1.0.0]** The `1.0.0` completion report recorded a startup median around 0.81 seconds and a working set around 146.6 MB, close to the existing 1.0-era 150 MB working-set budget. This was measured on the available machine; it is **not** proof of performance on an 8 GB low-end reference PC.
- **[FACT — 1.0.0]** `docs/DEFERRED.md` lists water-warp/HLSL, visual regression, low-spec hardware evidence, drag-and-drop workspace patterns, top-navigation/tabbed shells, complete gallery localization, chart theming, TreeView/full DataGrid/TimePicker examples, per-service template flags, richer notifications, authentication/crash-reporting samples, signing execution, and Visual Studio integration as post-1.0 candidates.

### 3.2 Baseline risks to close before major feature work

1. **Truthful quality gates:** UI tests must not silently skip while CI stays green. A test tier must report pass, fail, or explicit not-run; required release tests must actually execute in a supported environment.
2. **Whole-product testing:** every supported named preset must be tested. If the package is distributable through NuGet, the packed `.nupkg` must be tested separately from installing the repository folder.
3. **Consumer isolation:** template tests must not permanently change a developer's global template registrations, local SDK state, or unrelated TEMP output files.
4. **Performance assumptions:** current startup/working-set measurements are not low-spec evidence. Acquire or document repeatable results on an 8 GB/IGPU Windows 11 machine before promising hardware-specific performance.
5. **App vs library:** the product's stated goal is reusable UI foundation, but most implementation currently lives in the sample app. Do not describe it as a polished standalone public UI library until a downstream consumer can reference and use it independently.
6. **Documentation consistency:** screenshots, roadmap status, install instructions, package content, and report links must match the released artifact.

These are priorities, not accusations that the shipped app is unusable. Fix demonstrable issues, measure uncertain behavior, and preserve 1.0.0 as a reproducible baseline.

## 4. Research and competitive analysis

Research is a decision input, not a package-adoption checklist. The agent must refresh any finding that affects implementation before each milestone; versions, maintainership, licenses, platform support, and release states can change.

### 4.1 WPF UI — retain as the primary UI dependency for now

- **Evidence:** [WPF UI repository](https://github.com/lepoco/wpfui), [releases](https://github.com/lepoco/wpfui/releases), [MIT license](https://github.com/lepoco/wpfui/blob/main/LICENSE).
- **[FACT — research snapshot]** The latest stable release shown by the inspected release page was 4.3.0, published 4 May 2026. It includes fixes and enhancements in controls, tray support, and NumberBox behavior. The existing template is already on 4.3.0.
- **[RECOMMENDATION]** Keep WPF UI as the default Fluent control foundation during 1.x. Do not upgrade merely because a newer tag appears; check release notes, dependency graph, supported targets, migration impact, and generated consumer builds first.
- Add the template's differentiation above WPF UI: polished design tokens, layout recipes, additional controls only where gaps are proven, a cohesive motion API, Windows integrations, accessible interaction patterns, and a better sample/gallery app.
- Do not fork the library or duplicate an existing control without a concrete issue, documented workaround cost, tests, and an exit/migration strategy.

### 4.2 ChainJG/WpfTemplate — visual and interaction benchmark

- **Evidence:** [ChainJG/WpfTemplate](https://github.com/ChainJG/WpfTemplate).
- It presents its own token-based UI library, animated lookless controls, adaptive navigation rail, custom title bar, notifications, spotlight/tilt effects, and a living gallery. It explicitly discusses avoiding effects on text-containing subtrees to preserve text clarity.
- **[RECOMMENDATION]** Use it as a source of ideas for control anatomy, attached behavior ergonomics, gallery composition, and interaction detail. Independently validate each idea; do not copy visual assets, substantial code, or implementation without license/attribution review and a clear need.
- Its described product surface is a useful benchmark, not independent proof of production maturity or performance on low-end hardware.

### 4.3 Dane-ME/WPFTemplate — navigation and lifecycle benchmark

- **Evidence:** [Dane-ME/WPFTemplate](https://github.com/Dane-ME/WPFTemplate).
- It advertises Clean Architecture boundaries, ViewModel-first mapping, an LRU navigation cache, back-stack history, asynchronous navigation, cancellation, and lifecycle hooks. The surfaced repository is small, so treat its architecture claims as patterns to evaluate rather than a dependency-quality endorsement.
- **[RECOMMENDATION]** Test the underlying problems directly: rapid route changes, canceled requests, disposal, cache eviction, back/forward history, repeated navigation, and stale ViewModels updating a page after navigation.
- Implement a navigation framework only if it solves demonstrated gaps in the current shell. Avoid introducing an elaborate navigation framework where simple WPF UI navigation is sufficient.

### 4.4 ModernWPF — monitor and prototype, do not stack it blindly with WPF UI

- **Evidence:** [ModernWPF repository](https://github.com/Kinnara/ModernWpf), [release history](https://github.com/Kinnara/ModernWpf/releases), [1.0 roadmap](https://github.com/Kinnara/ModernWpf/blob/master/docs/roadmap-1.0.md).
- **[FACT — research snapshot]** The latest surfaced development line was `1.0.0-rc.1`, following previews with WinUI-shaped controls and window-material work. It was still marked pre-release in the inspected release evidence on 14 October 2026.
- **[RECOMMENDATION]** Do not make ModernWPF a default runtime dependency beside WPF UI. Their themes, resource keys, and controls overlap. Re-evaluate it when a specific gap (for example, a control or window-material capability) cannot be solved well by WPF UI plus a small local adapter.
- A trial requires a separate spike branch/sample, theme-resource conflict tests, light/dark/high-contrast verification, package-size/runtime measurements, and an ADR. A library's WinUI-like appearance is not proof that it can be safely mixed with another theme system.

### 4.5 Testing tools: UI automation plus carefully chosen visual comparison

- **FlaUI:** [repository](https://github.com/FlaUI/FlaUI), [releases](https://github.com/FlaUI/FlaUI/releases). The current solution uses FlaUI 5.0.0. Keep it for real UIA interactions if current compatibility and stability checks remain green.
- **Accessibility Insights for Windows:** [repository](https://github.com/microsoft/accessibility-insights-windows). Evaluate its workflows and accessibility scans as part of manual/CI-assisted verification; do not treat a scan as a replacement for keyboard and assistive-technology testing.
- **Verify snapshot tools:** [Verify](https://github.com/VerifyTests/Verify), [Verify.ImageMagick](https://github.com/VerifyTests/Verify.ImageMagick). Research found a material licensing/business-model caveat: Verify's official binaries released after 1 September 2026 are subject to its Open Source Maintenance Fee for certain revenue-generating organizations and government agencies. The underlying source being open does not mean the post-cutoff official binaries are cost-free for every downstream organization. **Do not make the Verify binary packages the default dependency without explicit license/cost evaluation.**
- **[RECOMMENDATION]** Prototype screenshot capture plus image comparison in a dedicated test project. Compare available no-fee/acceptable-license approaches and custom lightweight comparison. Choose based on license, transitive/native footprint, reliability, baseline review workflow, and stability in the pinned Windows runner—not based only on convenience.
- **[RECOMMENDATION]** Keep functional UI tests and visual snapshot tests separate. A screenshot diff cannot prove navigation, accessibility, data correctness, or correct interaction; a passing UI automation test cannot prove the interface looks right.

### 4.6 WPF performance and effects

- **Evidence:** Microsoft's [WPF performance overview](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/performance), [application performance planning](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/planning-for-application-performance), [performance optimization guidance](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/optimizing-wpf-application-performance), and [layout/design optimization](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/optimizing-performance-layout-and-design).
- Microsoft's guidance emphasizes scenario-based measurement and layering visual richness onto a lightweight base. WPF layout invalidation, excessive elements, expensive effects, rendering, data binding, and resource usage must be investigated through measurements rather than intuition.
- **[PROTOTYPE REQUIRED] Water warp:** WPF pixel-shader/`ShaderEffect` approaches, a bitmap-based distortion, and any Direct3D interop alternative have different compatibility, quality, deployment, and cost profiles. Prototype more than one plausible approach if the first is not clearly viable. Do not assume WinUI compositor effects are available in WPF with the same runtime characteristics.
- Text readability is a hard requirement: avoid placing costly bitmap effects on large text-containing visual subtrees, and test ClearType/readability, high-DPI scaling, interaction hit-testing, and reduced-motion behavior.

### 4.7 Windows notifications and signing

- **Evidence:** Microsoft [Windows notifications overview](https://learn.microsoft.com/en-us/windows/apps/develop/notifications/) and [app notifications quickstart](https://learn.microsoft.com/en-us/windows/apps/develop/notifications/app-notifications/app-notifications-quickstart).
- Microsoft's current notifications overview lists `AppNotificationManager` as the recommended modern approach for WPF, WinForms, and unpackaged Win32 desktop scenarios; legacy WinRT toast approaches are primarily maintenance paths.
- **[RECOMMENDATION]** Build the feature as an optional Windows-integration module, with activation routing and clean fallback when the SDK/package identity/environment does not support a requested capability. Validate package/version requirements, publisher identity, uninstall/update behavior, activation arguments, and Windows 11 settings before committing to a public API.
- Real code-signing and Store/publisher credentials are user-owned secrets. CI may validate scripts and unsigned packaging but must not invent credentials, commit certificates, or claim a signed release when no signing actually occurred.

### 4.8 Research decisions that must remain open until a spike proves them

| Decision | Default position | Evidence needed to change the default |
|---|---|---|
| Replace WPF UI | No | Material functional/visual benefit, viable maintenance path, license review, migration plan, independent consumer build, theme/perf proof |
| Add ModernWPF beside WPF UI | No | Specific unmet need and successful conflict-isolation prototype |
| Add a visual-regression package | Undecided | Current release/license, commercial downstream implications, CI reliability, native footprint, useful baseline-review workflow |
| Ship HLSL water warp | No, not in default UI | Real prototype, 8 GB/IGPU measurements, reduced-motion and static fallback, text clarity and hit-testing proof |
| Add Windows App SDK for notifications/materials | Optional-only | Package identity/distribution compatibility and footprint demonstrated per enabled/disabled preset |
| Create many libraries | No by default | Each project must define a meaningful dependency boundary and independent consumer/test use |

## 5. Product principles for 1.x and 2.0

1. **Balanced progress:** every few releases should bring visible UI improvements while architecture, performance, test coverage, and developer tooling improve continuously.
2. **Windows 11 Fluent first:** futuristic details are accents, not a competing theme system. Typography, spacing, contrast, window behavior, responsive content, and input feel come first.
3. **One coherent design system:** shared semantic tokens govern controls, states, charts, overlays, motion, and themes. No inconsistent one-off styles in the gallery.
4. **Useful examples, not sample clutter:** every demo showcases a real reusable capability and must be removable from generated apps.
5. **Optional features are truly optional:** disabled modules leave no orphaned files, packages, DI registrations, settings, app-startup work, or template directives.
6. **Prototype visual risk:** water warp and advanced rendering cannot enter default presets based on a screenshot-only review.
7. **Performance measured on named hardware:** use the reference environment for comparisons; don't compare numbers gathered on different machines as if equivalent.
8. **Tests must genuinely run:** required checks must be configured to fail or report blocked if they did not execute. A skip is not a pass.
9. **Stable `1.x`, planned `2.0` break:** additive fixes and features go into 1.x. Breaking public UI/package/template changes are staged and documented for 2.0.
10. **Maintainable over clever:** no custom framework, generator, shader, or service is accepted without consumers, meaningful tests, docs, and a maintenance case.

## 6. Proposed 2.0 target architecture

**[RECOMMENDATION]** Evolve to a small set of libraries with deliberate responsibility boundaries. Names below are proposed names; the per-version plan must confirm naming, package IDs, project references, and distribution details before implementation.

```text
src/
├─ DevTemWpf.UI/                 # Public tokens, styles, components, layouts, control contracts
├─ DevTemWpf.Windows/             # Window integration, supported backdrops, tray/notifications adapters
├─ DevTemWpf.Services/            # Service abstractions and common integration registration
├─ DevTemWpf.Services.*/          # Only split modules that need independent packages/dependencies
├─ DevTemWpf.Gallery/             # Reference app: shells, dashboard, demos, gallery and diagnostics UI
└─ DevTemWpf.Templates/           # dotnet new templates and pack/build integration

tests/
├─ DevTemWpf.Unit/
├─ DevTemWpf.Integration/
├─ DevTemWpf.TemplateSmoke/
├─ DevTemWpf.UiAutomation/
├─ DevTemWpf.VisualRegression/
├─ DevTemWpf.Accessibility/
└─ DevTemWpf.Performance/

docs/
├─ decisions/                     # ADRs for changes to the architecture
├─ design-system/                 # Tokens, shells, controls, motion and accessibility contracts
├─ guides/                        # Quick starts, recipes, integration and deployment
├─ releases/                      # Every version plan and report
├─ migration/                     # Explicit 1.x -> 2.0 guide and examples
└─ research/                      # Dated short research memos where a decision needs evidence
```

### Assembly/package boundary rules

- **`DevTemWpf.UI`:** XAML resource dictionaries, design tokens, reusable controls, layout primitives, motion abstractions/low-cost behaviors, documentation, and no app-specific business/service logic. Avoid a mandatory dependency on charting, SQLite, Velopack, telemetry, or Windows App SDK unless a specific UI function truly requires it.
- **`DevTemWpf.Windows`:** HWND/window integrations, system materials, tray/notifications and OS-specific features. Each capability must be guarded behind interfaces and feature registration, with documented unsupported/fallback behavior.
- **Services:** avoid making one enormous services package a transitive dependency of the Clean preset. Split only where modules have separate dependencies/distribution value; a package/assembly per trivial service is not a goal.
- **`DevTemWpf.Gallery`:** showcase pages, sample data, chart package, demo-only interactions, screenshots/manual review instructions. Generated production apps must not reference it.
- **Templates:** package and source layout verified from the produced `.nupkg`, never only from a repository-folder install. Keep the public template identity and CLI behavior predictable.
- **Composition root:** generated apps control which services register. Provide safe defaults and explicit options; avoid magic global static service access.
- **Public API discipline:** document public types/resource keys/attached properties, test a downstream project consuming the packaged libraries, and do not expose internals only to save a few lines of sample code.

### Compatibility strategy

- `1.x`: preserve existing template IDs, established options, and current public contracts where possible. Additions should be additive; deprecate before removing if feasible. Bug fixes may change behavior only where current behavior is defective, unsafe, or clearly undocumented.
- `2.0.0-preview.*`: allow reviewed breaking changes in public APIs, resources, project layout, CLI flags, template output, and registration patterns. Maintain a migration mapping.
- `2.0.0-rc.1`: freeze the intended stable public API, supported template flags, project/package boundaries, and public resource keys. Any material change requires a new RC and a fresh complete gate.
- Do not promise source compatibility with every internal class from the old single app project. The migration guide must state which existing patterns migrate automatically, which require manual edits, and which are sample-app-only rather than public APIs.

## 7. Release map at a glance

| Release | Theme | Main outcome | Must not become |
|---|---|---|---|
| `1.1.0` | Stabilization and test truth | Close known documentation defects, fix test isolation, remove false-green gates, audit dependencies and current behavior | Unbounded refactoring |
| `1.2.0` | Visual-system upgrade | A coherent, polished reference UI and reusable design-system improvements | A purely decorative redesign |
| `1.3.0` | Core control and data completion | Missing high-value controls, full data-grid/tree/time-picker demos, localization/chart polish | A giant custom control suite with no proven need |
| `1.4.0` | Shells and workspaces | Top navigation, tabs, keyboard/command patterns, drag/drop proof and selected stable workspace patterns | Shipping incomplete drag-and-drop because a demo looks good |
| `1.5.0` | Motion and effects | Better motion primitives and a measured go/no-go on water warp | A mandatory GPU-heavy background |
| `1.6.0` | Production modules | Per-service flags, notifications and signed/distribution guidance with real optionality | Pulling heavy service/Windows SDK dependencies into Clean |
| `1.7.0` | Developer tooling | Better page/control/service generators, docs, and a Visual Studio integration decision/implementation | An unmaintained VSIX created just to tick a box |
| `1.8.0` | Architecture and 2.0 design | Package/API boundary experiments, dependency map, downstream proof harness | Premature migration of all code without tests |
| `1.9.0` | 1.x hardening and migration baseline | Feature freeze for 2.0, full 1.x gates and compatibility contract | Sneaking major breaking changes into the last 1.x release |
| `2.0.0-preview.1` | Modular core | First end-to-end new package architecture and clean scaffold | Publishing a stable API before downstream evidence |
| `2.0.0-preview.2` | Feature parity and new DX | Port the validated components/layouts/generators/modules, fix migration gaps | Shipping all optional modules by default |
| `2.0.0-rc.1` | API freeze and candidate | Complete migration canaries, UI/performance/accessibility gates, package validation | Adding features during the RC freeze |
| `2.0.0` | Stable major release | Reusable modular platform, audited migration guidance, verified package set | A version bump without verified downstream usage |

Every release has its own `docs/releases/<version>-plan.md` and `<version>-report.md`. The table above is a roadmap, not permission to skip the existing execution protocol.

## 8. Milestones and acceptance criteria

## 8.1 `1.1.0` — Stability, correctness, and honest release gates

**Outcome:** make the 1.0 baseline easier to trust and repeat before major feature work starts.

### In scope

- Fix README defects: replace obsolete screenshot placeholder with actual current screenshots, remove duplicate/inconsistent 1.0 roadmap row, verify package installation commands and current release links.
- Update roadmap state and deferred-work status to reflect the real stable 1.0 release. Keep evidence links for shipped versus not-yet-implemented claims.
- Audit the current test behavior, including FlaUI opt-in/skip paths. Add machine-readable summaries for passed/failed/skipped tests and assert required test tiers actually ran in the release gate.
- Make the full Clean, Recommended, and Full template matrix run in CI at least on the release candidate and in the main quality workflow. Prefer all named presets on PR CI if runtime remains practical; otherwise use a fast PR matrix plus mandatory full PR/release workflow that branch protection requires.
- Make template tests use an isolated `DOTNET_CLI_HOME`/template-hive strategy and unique temp directories/logs. Restore/cleanup must run in `finally` without removing a developer's existing template registrations or clobbering shared log files.
- Test templates from both the source folder and the packed `.nupkg`; verify the package artifact installs and generates a project equivalent to source-template output.
- Make startup/performance scripts locate the intended project/executable robustly for solution configurations, renamed app projects, and subsequent modular architecture. Fail clearly on ambiguous/missing outputs.
- Verify `Directory.Packages.props`, `docs/DEPENDENCIES.md`, third-party notices, README badges, and release assets agree. Run current package vulnerability, outdated-package, license, and transitive-dependency audits; document accepted findings instead of blindly upgrading.
- Recheck current settings persistence, update-service cancellation/errors, backup/restore validation, culture/theme switching, shutdown cleanup, logging paths/retention, and no-op idle behavior. Implement only reproducible defects with focused regression tests.

### Explicitly out of scope

- No large architecture extraction.
- No unproven UI redesign or water-warp production effect.
- No unreviewed dependency suite additions.
- No breaking template CLI contract changes.

### Tests and gates

- Debug and Release build, warnings as errors.
- Unit/integration suite with recorded counts and no unexplained skips.
- Full three-preset source-folder matrix.
- Full three-preset built-package matrix, including `IncludeTests=false` where supported and item templates.
- UI automation test tier runs in its designated environment and the report shows the actual executed count; if unavailable, the full release gate is blocked rather than green-by-skip.
- Smoke launch/shutdown, application logs checked for binding errors, template name-residue/directive checks, dependency/security/license audit.
- Baseline startup/working set/idle CPU measured on available developer hardware; report machine specs and don't label the result as low-end evidence.

### Acceptance criteria

- [ ] README screenshots and release/roadmap status accurately reflect the 1.0.0 implementation.
- [ ] No required UI test can be skipped unnoticed by the release gate.
- [ ] Clean, Recommended, and Full presets build and smoke-test from source and from the generated package.
- [ ] Template validation does not corrupt the machine's pre-existing `dotnet new` registrations or share temp outputs across concurrent runs.
- [ ] No orphaned generated files, unresolved template tokens/directives, or `DevTemWpf` name residue remain in renamed projects.
- [ ] Dependency inventory, license notices, package contents, and version metadata agree.
- [ ] Existing published `v1.0.0` tag and asset remain unchanged; this is an additive 1.1.0 release.

## 8.2 `1.2.0` — Major UI quality and design-system refresh

**Outcome:** the reference app becomes the polished, coherent showcase the README promises and a reliable reference for consumers.

### In scope

- Run an interface audit of every current page at several window sizes, common display scales (100%, 125%, 150%, 200%), light/dark/system themes, keyboard-only use, and high-contrast mode.
- Define/clean semantic tokens for backgrounds, surface levels, text levels, separators, accent/status colors, typography, spacing, radii, elevation/shadows, focus rings, density, and motion. Remove near-duplicate hard-coded values in favor of tokens.
- Establish a consistent page layout: page title/subtitle, actions, scroll/content margins, content maximum width, section hierarchy, empty/loading/error states, and responsive behavior. Keep a clear relationship between shell, page chrome, cards, and controls.
- Redesign the gallery as a product-quality reference: component category index, preview, variants, key properties/options, states, keyboard interaction, theme view, and copyable XAML/C# snippets where practical. Avoid turning the page into an uncontrolled wall of controls.
- Improve navigation rail/sidebar sizing, selected/focus/hover feedback, compact/expanded transitions, scroll handling, overflow, and state persistence. Preserve platform conventions for title-bar drag, caption buttons, maximize/restore, resize, and snap layouts.
- Improve dashboard content hierarchy, spacing, chart/table alignment, useful empty/error/loading states, filter and selection clarity, and narrow-window behavior. Make dashboard/gallery isolation from Clean presets explicit and tested.
- Add approved screenshots for README and docs. Record the exact Windows build/scale/theme/font settings used for each baseline.

### Visual direction

Use premium Windows 11 Fluent as the foundation, with selective futuristic materials and depth. Prefer beautiful spacing, typography, clear contrast, clean edges, precise animation, and responsive layouts over constant gradients, glass overlays, glows, or exaggerated card stacks. Effects must be opt-in where costly.

### Tests and gates

- Visual audit checklist for each primary page in Light, Dark, System, and high-contrast configurations.
- UI automation assertions for accessible names, selected-navigation semantics, keyboard focus movement, dialog/flyout focus return, and theme changes.
- Introduce visual regression only after a license/footprint/runner spike. Pin rendering environment; store approved baselines and diff images as artifacts. Do not auto-approve changed screenshots. If cross-machine pixel-perfect snapshots are not stable, separate deterministic layout snapshots from tolerant perceptual comparison and manual review.
- Contrast checks for semantic text/status/focus combinations; no state represented by color or animation alone.
- Responsive/layout tests for common window widths and scaling; no clipped required controls or inaccessible overflow.
- Startup, idle, memory and navigation baseline recorded before/after redesign.

### Acceptance criteria

- [ ] All existing pages share the token and layout standards; documented exceptions are justified.
- [ ] Gallery demonstrates the actual released component behavior—not static mockups alone.
- [ ] All critical shell and dashboard layouts are verified at documented window sizes and DPI settings.
- [ ] High contrast, keyboard focus, and reduced motion remain usable.
- [ ] Any visual regression baseline change has a human-reviewed diff and matching commit evidence.
- [ ] Performance budgets from 1.1.0 still hold on the same reference machine or deviations are measured and explicitly ratified before the release plan is approved.

## 8.3 `1.3.0` — Complete core control and data-pattern coverage

**Outcome:** developers can build everyday data-heavy desktop applications without replacing the template's basic conventions.

### In scope

- Implement or style the **full DataGrid sample**: sorting, filtering/search, selection, editing/validation, empty/loading/error states, keyboard behavior, sensible column sizing, and scalable scrolling. Reuse WPF/native toolkit controls where possible.
- Add a TreeView/navigation example with selection, expansion persistence if appropriate, keyboard navigation, names/automation peers, and large-tree test data.
- Add a TimePicker sample/control only after the library audit confirms no dependable existing control meets the use case; avoid introducing a conflicting control suite just to fill a checklist.
- Complete the control gallery's common states: normal, hover, pressed, selected, disabled, focus-visible, invalid, loading, success/warning/error, and empty state where relevant.
- Add paging/virtualization patterns, list/detail, master/detail, selectable cards, and search/filter examples as recipes instead of creating a custom control for every page pattern.
- Complete dashboard chart theme integration for light/dark/accent and accessibility-friendly descriptions. Keep LiveCharts2 and its native SkiaSharp dependencies in the dashboard-only/Full module.
- Finish gallery localization coverage and ensure resource keys are stable and documented.
- Add consistent form validation helpers, async command/loading/error states, cancellation for long operations, and clear user-facing recoverable error patterns.

### Tests and gates

- Unit tests for sorting/filtering/selection, form validation, commands, cancellation, culture changes, and state persistence.
- UI automation for DataGrid editing, column/row keyboard focus, TreeView expansion/selection, TimePicker inputs, form errors, and empty/loading/error transitions.
- Large-data tests: representative 1k and 10k row/item samples; verify that the selected virtualization path actually virtualizes (measure and inspect, don't infer solely from the control name).
- Verify no service/chart package leaks into Clean or any preset that excludes it.
- DataGrid/TreeView tests must include keyboard-only scenarios, automation properties, scaling, theme changes, and no-data states.

### Acceptance criteria

- [ ] Deferred D16/D17/D18 items are completed or explicitly re-scoped with evidence and a new date/version—not silently removed.
- [ ] Each new major control has a gallery page, a usage guide, and tests for normal and edge behavior.
- [ ] Data views remain responsive at the agreed 1k/10k data test sizes; measured limitations and mitigation are documented.
- [ ] Clean and Recommended projects do not acquire dashboard chart dependencies unless selected.
- [ ] Localization works across both controls and the complete gallery, not only the Settings page.

## 8.4 `1.4.0` — Additional shell presets and reliable workspaces

**Outcome:** the template supports more real application shapes while keeping navigation and lifecycle behavior understandable.

### In scope

- Add a top-navigation shell preset for focused/small apps and a tabbed/document-workspace preset for multi-document workflows. Treat rail and expandable sidebar as existing presets to preserve.
- Standardize navigation contracts across shells: route identity, selection, back navigation where appropriate, view/view-model mapping, state restoration, async loading, and cancellation behavior.
- Add a command/search palette as an optional keyboard-first surface only after usability and accessibility are defined. Commands need stable IDs, labels, keyboard navigation, execution/disabled/loading states, and discoverable shortcuts.
- Prototype drag-and-drop and resizable split panes using a real representative workspace scenario. Define drag hit targets, keyboard alternatives, data-transfer contracts, cancellation, feedback, persistence, and high-DPI behavior.
- Implement the best validated interaction patterns; defer complex window tear-out or cross-process drag if it adds substantial complexity without a demonstrated consumer need.
- Expose shell choices through valid `dotnet new` parameters/presets; generated output must not contain unselected shell source files, registrations, or broken navigation references.
- Improve windowing to multiple windows only if a testable scenario benefits from it. Do not make multi-window a default architecture requirement purely as a future possibility.

### Tests and gates

- Navigation race tests: fast repeated navigation, cancellation, disposed ViewModels, failed page creation, back/forward behavior, and cache eviction if caching is adopted.
- UI automation through keyboard and UIA patterns; avoid relying only on absolute coordinate clicks or fixed sleeps.
- Workspace persistence tests across app restart and malformed/stale settings files.
- Drag-and-drop must have keyboard-accessible alternatives where a drag-only interaction would exclude users.
- All shell presets must generate, build, launch, navigate, and shut down independently.

### Acceptance criteria

- [ ] Every documented shell preset is selectable at scaffold time and independently smoke-tested.
- [ ] A page/ViewModel implementation can be reused across compatible shells without shell-specific rewrites.
- [ ] No stale asynchronous operation updates a page after cancellation/disposal in tested navigation races.
- [ ] Tab/workspace state has a documented persistence and failure-recovery contract.
- [ ] Drag-and-drop ships only if its prototype passes accessibility, reliability, and performance gates; otherwise it remains documented as deferred.

## 8.5 `1.5.0` — Advanced motion system and effects prototype

**Outcome:** animation and rendering feel more deliberate and premium without turning the default application into an expensive effects demo.

### In scope

- Audit the existing motion service/policy and establish consistent motion tokens for duration, easing, offset/distance, opacity, stagger, and interrupt/cancel behavior.
- Provide reusable motion behaviors/primitives for property transitions, enter/exit, selection, expansion/collapse, feedback overlays, and reduced-motion-aware page transitions. Avoid attaching multiple animation writers to the same dependency property.
- Add a consistent configurable policy such as `Auto`, `Balanced`, `Rich`, `Reduced`, and `Off`, but keep the final enum and semantics aligned with existing current API contracts; don't rename settings casually in 1.x.
- Ensure animations are cancelable on navigation, unload, theme changes, window deactivation where necessary, and app shutdown. Avoid permanent per-frame `CompositionTarget.Rendering` handlers after a demo/page leaves the visual tree.
- Prototype optional water-warp/ripple rendering. Compare candidate approaches, including WPF pixel shader / `ShaderEffect`, a lower-cost simulated distortion, and any interop approach only if there is a valid reason. Record toolchain, packaging requirements, rendering tier behavior, GPU/CPU/working-set cost, and visual limitations.
- Keep text and interactive controls outside expensive offscreen effects unless the prototype proves text quality and input behavior remain correct. Prefer treating backdrop artwork and decoration as separate visuals behind actual content.
- Add a dedicated effect settings/showcase page with per-effect on/off switches, preset selection, fallback status, and measured profile information for development builds.

### Water-warp ship/no-ship gate

**[PROTOTYPE REQUIRED]** Water warp is an evaluated candidate, not an unconditional feature promise. Ship it only if all of these are proven:

1. It runs on the declared low-resource reference machine or cleanly falls back to a static/simple effect.
2. It never blocks input, navigation, text rendering, window interaction, or app startup.
3. It respects reduced motion, high contrast, transparency/effect settings, and explicit Off mode.
4. It has a reliable non-shader fallback and doesn't require an undocumented external tool or GPU capability.
5. CPU/GPU cost, memory, package size, deployment compatibility, and rendering behavior are measured and documented.
6. It has no material leak/hang during repeated navigation, resize, minimize/restore, theme change, and shutdown.

If any condition fails, document the result and defer the effect. Do not hold unrelated 1.5.0 motion improvements hostage to an experimental shader; do not quietly waive its gate to hit a version number.

### Tests and gates

- Unit tests for motion policy, reduced motion, interruption, cancellation, and fallback-selection logic.
- UI test coverage for focus/selection/hover state with animations on and off.
- Stress repeated page navigation/resize/toggle cycles while watching memory growth, animation cleanup and idle CPU.
- Benchmark startup and responsiveness at `Auto`/`Balanced`/`Rich`/`Reduced`/`Off` where supported, with machine profile and repeatable steps.
- Record screen capture and side-by-side effect comparisons; these are supplementary evidence, not a replacement for measurements.

### Acceptance criteria

- [ ] Every shipped animation path has a reduced-motion/static behavior.
- [ ] Leaving a page does not leave ongoing timers/rendering callbacks/animations alive unintentionally.
- [ ] Effects are opt-in and absent from Clean's startup path unless expressly selected.
- [ ] Water-warp has a recorded go/no-go decision backed by measurements; a failed spike does not become a shipped default.
- [ ] No effect is approved if it materially degrades text legibility, accessibility, input, or the agreed 8 GB performance target.

## 8.6 `1.6.0` — Production modules, Windows integration, and optionality

**Outcome:** production services become safer, clearer, easier to switch on/off, and more complete for Windows desktop applications.

### In scope

- Add individually selectable service/module flags where dependencies and generated files are independently meaningful: logging, SQLite/local data, HTTP clients, backup/restore, update engine, tray, localization, diagnostics, notifications, and optional auth/crash-reporting samples.
- Begin with a dependency graph: map each feature flag to source files, packages, registrations, configuration sections, resources, installer requirements, output files, and test requirements. Use derived symbols/templates rather than inconsistent manual `#if` blocks.
- Evaluate Windows app notifications using `AppNotificationManager` via an optional Windows integration module. Test unpackaged and packaged identity scenarios supported by the feature; document cases that intentionally do not work.
- Add optional authentication/crash-reporting sample integrations only with a security/privacy review, environment-based configuration, secret redaction, data-minimization defaults, explicit opt-in, and no network telemetry by default.
- Improve update experience: channel selection where supported, current-versus-available version, download/update failure handling, retry, cancellation where safe, restart prompt, delta/full update validation, and documented rollback/recovery paths.
- Improve package/signing/distribution workflows for portable/Velopack and MSIX/Store paths as supported. Separate script verification from real signing; don't report a real signed artifact without a valid certificate run.
- Test backup/restore against missing/corrupt files, schema/version change, interrupted writes, duplicate state, path traversal, and restore-to-new-location scenarios.
- Keep logs bounded and private: documented retention, no secrets/tokens/passwords, safe exception rendering, and opt-in redaction. Keep logs/DB/config under correct per-user paths.

### Tests and gates

- Full feature-flag presence/absence matrix for each independent module and valid combinations; use pairwise combination coverage plus every named preset rather than assuming the entire Boolean power set is affordable.
- Package/output inspection: unselected module DLLs, NuGet references, settings keys, source files, and registrations must be absent where the contract says absent.
- Notification activation tests for routing/arguments and process startup. Validate Windows SDK versions/identity requirements using a real environment.
- Update service tests for failure boundaries, canceled operations, interrupted downloads, corrupted metadata, downgrade prevention, and safe recovery.
- Backup/restore tests with corrupt, partially written, oversized, unsupported-version, and malicious paths.
- Security scans and secret-leak checks; authentication/crash samples disabled by default.
- Distribution smoke tests for package install, launch, update path, uninstall, user data retention policy, and clean upgrade across versions wherever the environment supports it.

### Acceptance criteria

- [ ] Each advertised service flag has a file/dependency/registration contract, tests, and documentation.
- [ ] Clean remains minimal; Recommended and Full include only their documented feature sets.
- [ ] Notification integration has a functional in-app fallback and correct activation behavior or clearly reports unsupported modes.
- [ ] Auth/crash reporting are strictly opt-in, disabled without configuration, and never contain embedded secrets or default telemetry.
- [ ] Portable and MSIX/Store paths are kept separate and every claimed combination has evidence; unsupported update/distribution combinations are rejected explicitly.
- [ ] Third-party licenses/notices and package assets are refreshed after every dependency change.

## 8.7 `1.7.0` — Advanced scaffolding and developer productivity

**Outcome:** new pages and features can be generated consistently, quickly, and with fewer code-agent mistakes.

### In scope

- Expand `dotnet new` item templates for page, settings page, list/detail page, DataGrid page, TreeView page, tab/workspace page, reusable custom control, ViewModel, service, and corresponding test scaffold where each generator provides meaningful value.
- Add page/navigation registration patterns matching all supported shells and module flags. Avoid generators that require hand-editing hidden resource paths for the typical case.
- Add a control/scaffold generator only after its public API, dependency properties, styling hooks, default token use, automation properties, documentation, and test requirements are settled.
- Improve generated `README.md`, `FEATURES.md`, `Directory.Packages.props`, and migration notes so they describe the actual generated preset/options accurately.
- Evaluate Visual Studio integration. Compare .NET CLI template visibility, an item-template VSIX, and an extension/add-in approach using current Microsoft tooling, installation/update requirements, licensing, maintenance, and multi-version Visual Studio compatibility. The extension is not mandatory unless it substantially improves workflow beyond what the CLI already provides.
- Add template help/discovery docs with examples; ensure CLI flags have validated default values, clear conflicts, stable casing/aliases where applicable, and useful error messages.
- Provide a repeatable script to generate a sample app, add a page/service/control, build, test, launch, and verify no unresolved template tokens. Optionally include an agent recipe or skill document only if it is maintained alongside the generator contract.

### Tests and gates

- Golden/expected generated-file checks for each item template and supported naming/namespace patterns.
- Build and smoke generated samples, including names with spaces/keywords where supported, multiple namespaces, optional modules, and renamed solution/project IDs.
- Verify file add/remove and project reference behavior when generators run more than once or when the target feature already exists.
- If a Visual Studio extension is selected, package install/uninstall/update tests and clean Visual Studio environment validation; don't test only with the developer's already-loaded experimental extension.

### Acceptance criteria

- [ ] Generator output uses current architecture and tokens, and compiles without manual repair.
- [ ] Generated tests actually test behavior rather than assert only that a file exists.
- [ ] Item templates have unique IDs, clear help text, correct namespace substitution, and deterministic output.
- [ ] A Visual Studio integration decision is recorded. Ship an extension only if its support/packaging cost is justified; otherwise provide clean CLI-first documentation.
- [ ] No generator silently expands a preset or re-adds dependencies that the user explicitly omitted.

## 8.8 `1.8.0` — Architecture preparation and 2.0 compatibility contract

**Outcome:** decide and prove the modular boundaries before starting the breaking conversion.

### In scope

- Use the current source and tests to map all existing public-ish types, resource keys, services, view models, template parameters, navigation assumptions, settings files, generated project shape, package IDs, and dependencies.
- Classify each item as public API, internal implementation, gallery-only sample, optional integration, generated-app contract, deprecated candidate, or removal candidate.
- Prototype the proposed `DevTemWpf.UI`, `DevTemWpf.Windows`, service module boundaries, gallery, and template package in an isolated branch or experimental solution. Do not destabilize `main` while proving architecture assumptions.
- Build one minimal downstream application using the proposed package(s) with no reference to the gallery. Build a full sample consuming the UI and optional modules. Verify design-time XAML, resource loading, custom controls, themes, and pack URIs from compiled libraries.
- Determine resource dictionary merge rules, public style keys, theme switching, image/icon resources, control default styles, animations, and module dependency lifetimes across assembly boundaries.
- Define a 1.x-to-2.0 migration document draft and a compatibility mapping for CLI parameters, file layout, DI registration, resource keys, and common control usage.
- Establish a rule for library package versions/release notes/API change review; record an ADR for each boundary with alternatives and a reason it matters.

### Tests and gates

- Downstream canary projects: minimal UI-only consumer, recommended application, full gallery/services consumer, and generated custom control/page sample.
- Build package and consume it from a clean temp repository, not only with project references.
- Check theme resource resolution at runtime and in a built/packed consumer, ensuring resources are not accidentally present only through the gallery's app-wide dictionary.
- Measure startup and publish footprint for the experimental assembly layout to ensure modularity does not add pointless startup or packaged file overhead.

### Acceptance criteria

- [ ] Proposed 2.0 architecture has an accepted ADR and clear assembly dependency diagram.
- [ ] Each assembly exists for a demonstrable boundary, not for a superficial folder-to-project conversion.
- [ ] At least one independent package-consuming app builds/launches with no reference to the gallery app.
- [ ] Public API/resource/CLI compatibility inventory and 1.x-to-2.0 migration draft exist.
- [ ] Package separation does not introduce unresolved theme/resource, design-time, publish, licensing, or performance defects.

## 8.9 `1.9.0` — 1.x stabilization and 2.0 migration baseline

**Outcome:** finalize the best possible 1.x foundation, set the boundary of supported 1.x behavior, and prepare honest upgrade evidence.

### In scope

- Resolve confirmed P0/P1 defects and high-value P2 defects in the current supported 1.x surface. Do not add new large feature families merely because a roadmap item remains tempting.
- Complete all named presets and item-template tests from source and packed package; include any documented per-service and shell flags supported by the 1.x branch.
- Re-run dependency, license, package-content, security, and documentation audits. Re-check the exact UI library and integration package versions before locking the first 2.0 preview.
- Freeze the 2.0 public API plan: library names, core resource keys, theme tokens, control properties, module registration, template identity/flags, supported shell presets, and package split.
- Complete user-facing migration notes with examples for common `App.xaml`, `MainWindow`, control styling, navigation, services, package references, project naming, and generated-template upgrades.
- Define 1.x support policy after 2.0: whether a maintenance branch will receive security/correctness fixes, for how long, and how compatibility bugs will be classified. Do not promise support periods without capacity to honor them.

### Tests and gates

- Full 1.x release gate on clean clone and packaged template.
- Capture stable baseline: startup, idle CPU, working set, large-list interactions, shell navigation, per-effect performance, and package size.
- Run all required UI automation in a supported interactive Windows environment; the report must show non-zero executed test counts for required suites.
- Validate accessibility and visual snapshots; snapshot changes are explicitly reviewed and never accepted mechanically.
- Test a 1.x app and template install before and after template upgrade so the starting state for migration is reproducible.

### Acceptance criteria

- [ ] `1.9.0` does not introduce unannounced breaking changes to the stable 1.x contracts.
- [ ] Every deferred backlog item is marked shipped, still deferred with rationale, or rejected with evidence.
- [ ] All 2.0 breaking changes are known, grouped, versioned, documented, and assigned migration examples.
- [ ] A clean clone and package-consumer run reproduce the release evidence.
- [ ] Stable 1.x baselines and known limitations are available for objective 2.0 comparison.

## 8.10 `2.0.0-preview.1` — Modular foundation and new package boundaries

**Outcome:** deliver the first end-to-end modular proof, not just a reorganized repository.

### In scope

- Introduce the accepted class libraries and move reusable control/theme/token/motion assets into the public UI library while preserving intended behavior.
- Move native/window/effect/notification/tray integration behind Windows adapters and module registration interfaces.
- Keep the gallery app as a consumer of the UI packages—not as a dependency of those packages. Remove app-only services, fake/sample data, charts, dashboards, and diagnostics from the core UI library.
- Rework app composition to use explicit module registration methods. Keep Clean genuinely clean and make Recommended/Full an intentional composition of packages and samples.
- Define public APIs for design tokens/themes, custom controls, motion policy, shells/navigation integration, diagnostics and optional service integration. Document what remains an app-level concern.
- Package the UI library and optional libraries as NuGet packages if distribution testing proves this is useful; otherwise define a stable repository/project-reference use path first. Every published package needs independent versioning and license/notice metadata.

### Tests and gates

- Build and test each library independently.
- Consume the built `.nupkg` from a clean sample project with no reference to repository source paths or gallery assemblies.
- Test library-owned resource dictionaries/styles in a sample app's `App.xaml`, including light/dark/system theme, high contrast, and control templates.
- Validate custom resource URIs and assembly pack paths after publication/renaming.
- Compare clean output footprint/startup with 1.9.0 and explain any regressions.

### Acceptance criteria

- [ ] The UI library is independently consumable and usable without the demo app.
- [ ] Gallery-only code is not required to register or render common UI components.
- [ ] Optional modules can be added/removed without hand-editing unexplained code-behind or hidden global state.
- [ ] Published/packed libraries load styles and resources correctly in a clean consumer application.
- [ ] The preview release notes identify known gaps and do not claim full parity prematurely.

## 8.11 `2.0.0-preview.2` — Complete validated components, layouts, modules, and DX

**Outcome:** bring the approved 1.x feature surface into the modular architecture and complete the remaining public developer experience.

### In scope

- Port and validate the accepted control gallery, DataGrid/TreeView/TimePicker patterns, shell presets, settings/forms, overlays, and workspace patterns onto the reusable packages.
- Complete optional service registration and package boundaries; ensure charting, notification APIs, updates, storage, diagnostics, auth, and crash reporting do not become transitive dependencies of the minimal UI package.
- Update templates and item templates to emit the new project layout. Add migration-friendly CLI parameters and remove old hidden template directives where a clearer template configuration is possible.
- Update developer docs, component API pages, XAML snippets, architectural diagrams, dependency/license inventory, settings migration docs, and upgrade guide.
- Decide the final status of experimental effects. Water warp may be included only as an optional package/sample feature if the 1.5 prototype gates passed; otherwise make the lighter alternative and the decision rationale explicit.

### Tests and gates

- Full source and package matrix across the three named presets and each advertised independent module option.
- API/contract tests covering public resource keys, public control properties and default styles, navigation hooks, template CLI options, and module registration behavior.
- Generated-app test: Clean app, add page, add service/control, build, run, theme switch, navigation, shutdown. Repeat against the packed template package.
- Full UI automation and visual regression across supported shells, light/dark, reduced motion, scale profiles, and common widths.
- Performance comparison against 1.9 baseline on a low-spec reference machine and an ordinary development machine.

### Acceptance criteria

- [ ] All documented common component/layout features are available from the intended package(s) and shown in the modular gallery.
- [ ] The Clean preset contains only dependencies needed for the clean application contract.
- [ ] Full and Recommended presets include only their documented feature sets.
- [ ] All advertised CLI combinations have successful generated consumer tests, or invalid combinations fail clearly at generation time.
- [ ] Migration docs show the old and new shape for all high-impact changes.

## 8.12 `2.0.0-rc.1` — API freeze, migration canaries, and full release candidate

**Outcome:** verify the exact intended 2.0 public surface with no remaining planned feature expansion.

### In scope

- Freeze public API, resource keys, package IDs/relationships, template IDs, CLI flags, theme tokens, shell contracts, module registration methods, settings migration, and output-file naming for the candidate.
- Cut an immutable candidate and run the full set of gates against its exact source/tag/package artifacts.
- Run multiple clean consumer canaries: Clean UI-only app, Recommended production app, Full gallery/services app, and at least one representative migrated 1.x application. Canaries must not use repository project references for the package-validation path.
- Complete dependency/license/security review and package/NOTICE validation. Ensure package content does not carry unnecessary development docs, test binaries, secrets, `bin/obj`, or hidden local paths.
- Complete documentation screenshots, installation/update/uninstall, support policy, known issues, release notes, migration guide and rollback guidance.
- Start a review/soak period after all evidence is accepted. Any public contract change or release-blocking correction requires a new candidate and rerunning the applicable gates.

### Tests and gates

- Full Debug/Release builds and required unit/integration tests.
- Full template and package matrices.
- UI automation must execute on a suitable Windows 11 UI-enabled runner; reports cannot be green with required UI suites skipped.
- Visual regression results reviewed; any intentional change has updated baselines and review evidence.
- Accessibility/keyboard/high-contrast/reduced-motion checks complete.
- Performance: cold start, working set, idle CPU, large data, shell transition and optional effects measured on the reference low-end PC and comparison PC.
- Package/signature/hash, dependency/license/security scans, and release artifacts verified.

### Acceptance criteria

- [ ] No open P0/P1 security, data-loss, crash/startup, core-input, migration, or package-install blocker.
- [ ] No unresolved failed required test; environment skips are explicitly classified and cannot count as passes for a required gate.
- [ ] Each public contract is frozen and documented; migration canaries pass.
- [ ] API/resource/package diff since the accepted candidate baseline is reviewed.
- [ ] Performance results fit the approved budgets or have a formally accepted exception based on measured evidence.

## 8.13 `2.0.0` — Stable modular platform

**Outcome:** ship a new major release that makes the product's reusable-library claim true and verifiable.

### Release contents

- Public UI foundation package(s) with documented themes, tokens, controls, layouts, and motion contracts.
- Optional Windows integration and production service modules with intentional dependency boundaries.
- Reference gallery app showcasing actual components, shells, visual states, accessibility affordances, and optional effects.
- Clean/Recommended/Full (and any validated shell presets) via a tested `dotnet new` package plus tested item templates/generators.
- Complete migration guide and compatibility notes for 1.x applications.
- End-to-end quality evidence: generated consumer builds, automated tests, UI automation, visual regression review, accessibility checks, dependency/maintenance/license audit, low-spec performance reports, and package hashes.

### Final acceptance criteria

- [ ] A developer can install the released template package, generate a Clean project, build it, open a usable WPF UI, add a page/service, and run without referencing this repository or its gallery.
- [ ] A developer can install the modular UI package(s) into an existing WPF application and use documented controls/tokens without importing the demo app.
- [ ] All published library packages and template packages contain the expected files and are verified by clean package consumers.
- [ ] Required CI checks executed and passed; UI automation isn't counted as passing when skipped.
- [ ] Low-end/8 GB performance claims match measured results and explicit machine profiles.
- [ ] All required licenses/notices are complete, and no newly adopted package violates the project's no-surprise-cost or permissive-license policy.
- [ ] Migration documentation and a tested migrated application are published with the release.
- [ ] GitHub release, tag, packaged artifacts, checksums, changelog, README and roadmap agree on version and contents.

## 9. Test strategy and release quality architecture

### 9.1 Test layers

| Layer | Purpose | Examples | Release requirement |
|---|---|---|---|
| Unit | Deterministic behavior of isolated classes | motion policy, settings validation, sorting/filtering, update state machine, backup validation, feature selection | Runs on every PR; required tests cannot silently skip |
| WPF component/STA | Correct WPF object/control semantics | default styles, dependency properties, commands, converters, resource resolution, validation states | Run on Windows with STA Dispatcher support |
| Integration | Real service and composition behavior | Generic Host lifecycle, DI modules, SQLite, backup/restore, update engine abstraction, navigation cancellation | Runs on every PR for changed modules and on full gate |
| Template smoke | Generated project is a valid independent app | Clean/Recommended/Full restore/build/test/launch | Every named preset tested from source and packaged artifact before release |
| UI automation | Actual interactive app behavior through UI Automation | navigate, keyboard, dialogs, theme/settings, DataGrid, shell, windows, update flow | Required on release; must prove tests executed |
| Visual regression | Detect unintended UI differences | shell/page/control screenshots, light/dark, high contrast, scale/width variants | Approved baselines; diff artifacts; human review for changed baselines |
| Accessibility | Input and assistive semantics | keyboard order, focus return, names/roles, high contrast, contrast, reduced motion | Automated checks plus documented manual review |
| Performance | Verify speed/resource goals | start-up, idle CPU/memory, list scale, navigation, animations/effects, package size | Repeatable measurements on named hardware |
| Package/consumer | Verify actual distributable artifacts | install template nupkg, build UI packages from NuGet/local feed, no source-path references | Mandatory before any release that changes packaging or public packages |
| Security/dependency | Audit delivered dependencies and content | vulnerable packages, licenses, native assets, secrets, output file inspection | Mandatory per release, deeper audit when dependency graph changes |

### 9.2 UI automation: no false green

- UI automation may require an interactive logged-in desktop. Separate ordinary hosted CI jobs from any UI-enabled runner requirement; do not disguise a headless/environment restriction as a passed test.
- A required test tier must publish counts for `passed`, `failed`, `skipped`, and `not run`. If the required suite did not execute, fail the required release job or mark the release blocked.
- For untrusted external pull requests, do not pass repository secrets into a privileged self-hosted runner. Use a safe trigger model; run trusted UI-release validation on an appropriately secured runner.
- Prefer UI Automation patterns such as Invoke, Selection, Value, and Keyboard over hard-coded coordinates. Use coordinates only when the specific interaction lacks an accessible pattern, and document the reason.
- Avoid fixed sleeps where an observable state can be awaited. When settling delays are necessary, record why and keep timeouts bounded.
- On failure, capture useful diagnostics: test result, screenshots/screen recording where safe, UI tree/automation names, process exit details, app logs and runner state. Never include secrets or user data.
- Keep flaky-test triage separate from product defects. A retry can provide diagnostic evidence but cannot permanently mask a reproducible failure.

### 9.3 Visual regression contract

- Use a deterministic reference Windows environment with recorded OS build, installed fonts, display scale, window bounds, theme, accent, animation profile and rendering mode.
- Baselines include the primary shells, navigation expanded/collapsed, key control families and states, dashboard/data views, overlays/dialogs, settings, and effect fallbacks.
- Compare screenshots with a documented pixel/perceptual tolerance and masks only for known nondeterministic regions. Masks must not hide layout regressions.
- Store diff images as CI artifacts. Baseline updates must be reviewed; never regenerate all snapshots automatically to clear a failed test.
- Research package licenses and commercial downstream terms before taking a snapshot library dependency. Prefer a no-cost approach compatible with this template's open-source/permissive dependency stance.
- Keep functionality tests and visual comparison as independent gates.

### 9.4 Accessibility contract

- Every interactive control must have predictable keyboard reachability and visible focus.
- UI Automation names and control types must describe actual semantics; don't assign a broad parent name that hides child content from accessibility tools.
- High-contrast mode must preserve readable text/borders/focus/selection; do not rely on blurred backdrops, gradients, or accent colors that disappear in high contrast.
- Respect OS reduced-animation/accessibility preferences; expose app-level controls only where they make sense and never override a stronger OS accessibility preference without a reason.
- Dialogs/popovers must have correct initial focus, keyboard dismissal, focus containment when modal, and focus return.
- Drag-only interactions require a keyboard/menu equivalent if they are part of core workflows.
- Automated scans supplement manual keyboard walkthrough and available screen-reader checks; reports must state which tools and workflows actually ran.

## 10. Performance contract for an 8 GB Windows 11 PC

### 10.1 Measurement method

**[RECOMMENDATION]** Establish a dedicated low-spec reference machine/profile with 8 GB RAM, integrated graphics, documented CPU/storage, Windows build, power plan, scale, and display configuration. If physical hardware is unavailable at a milestone, mark low-spec validation as pending; do not infer it from a developer desktop or virtual machine.

Measurements must be repeatable, report raw samples and median/worst results, include warm/cold scenario descriptions, and use the same machine for before/after comparisons. Separate the base Clean app, Recommended services, Full gallery/dashboard, and every optional expensive effect. Note debugger/non-debugger and framework/runtime configuration.

### 10.2 Initial budgets to ratify in 1.1.0

The following are **proposed budgets, not yet verified guarantees**. Measure the baseline first and document any ratification or reasoned adjustment in an ADR; do not silently loosen the thresholds because a new effect is expensive.

| Metric | Initial target for Clean | Recommended/Full policy | Notes |
|---|---|---|---|
| Cold launch to usable window | Median ≤ 1.2 s; p95/worst under documented bound | Measure service initialization separately; show nonblocking loading only where needed | Keep old 1.0 baseline for same-machine comparison |
| Idle CPU after settle | Approximately 0% sustained; no recurring polling/render loop without user-visible value | No busy timers or permanent render callbacks for disabled effects | Report method and process/core normalization |
| Working set after warm-up | Target ≤ 150 MB on the defined reference machine | Optional modules/effects publish incremental cost; don't imply zero overhead | 1.0 was around 146.6 MB on different/available hardware, so the low-end target needs proof |
| Navigation response | Common page changes feel immediate; establish measured p95 interaction timing | Expensive loading must be async/cancelable | Avoid speculative fixed timing claims before baseline |
| 1k/10k item display | No obvious UI freeze; virtualization proven and responsiveness measured | Demo includes realistic data and keyboard/accessibility paths | Use repeatable data and target hardware |
| Off/Reduced effect profile | No shader/ambient animation work while disabled/reduced | Expensive effects are stopped/unloaded, not merely transparent | Check CPU, memory and lifecycle |
| App idle after 30 s | No sustained unnecessary CPU use | Notification/update checks are scheduled intentionally and documented | A single noisy system counter sample is not enough |
| Startup/package footprint | Record clean output size and dependency graph | Document incremental modules and native assets | Package size and working set are distinct metrics |

Budgets must use a consistent metric definition. Do not compare `Process.WorkingSet64` with private bytes or publish CPU percentages without stating normalization. A budget breach triggers investigation and either a measured fix or a documented, approved exception before release.

### 10.3 Performance engineering rules

- Prefer virtualization, async work, cancellation, sensible collection updates, and minimal layout invalidation before micro-optimizing code.
- Avoid creating all gallery pages/controls eagerly in Clean startup. Instantiate only what the selected shell/page needs.
- Stop clocks, animations, event handlers, rendering callbacks and observers when a page/control unloads; make event subscriptions disposable or use appropriate weak/event patterns.
- Keep effects isolated behind feature switches and profile policies. Do not run per-frame updates while offscreen, inactive, minimized, or unselected unless the user-visible function explicitly requires it.
- Measure chart/native runtime and service package increments; do not equate a modular package boundary with lower memory at runtime.
- Use BenchmarkDotNet only where repeatable microbenchmarks help answer an isolated CPU question. Use app-launch and UI scenario measurement for real WPF end-to-end performance.
- Record improvements/regressions with same-machine comparisons and raw results. Avoid synthetic benchmark claims without reproducible commands, workload and hardware details.

## 11. Dependency, license, and maintenance policy

- Refresh package research for every package changed in a release. Prefer stable, actively maintained, permissively licensed dependencies; do not add libraries just because competitors use them.
- Check release/tag status, target framework support, license and third-party notices, direct/transitive dependencies, native assets, security advisories, known issues, maintenance cadence, API fit, packaging, startup footprint, and practical removal cost.
- Prefer one primary visual/control foundation. Do not mix WPF UI, ModernWPF, MahApps.Metro and MaterialDesign themes together by default.
- Keep LiveCharts2/SkiaSharp in the Full/dashboard path unless an app explicitly opts in to charts.
- Verify packages against the actual downstream commercial use terms—not only source-code license labels. In particular, the research snapshot found new official-binary fee terms in Verify's ecosystem after 1 September 2026; re-evaluate any proposed use, and don't quietly introduce a dependency that creates unexpected fees for template consumers.
- Pin package versions centrally, update dependency inventories and notices with each change, and test old/new package combinations intentionally. Never float core package versions in stable generated templates.
- For an upgrade that changes a major version, public resources, controls, native assets, or theme behavior, write a short ADR/spike record and test a clean consumer application before adoption.
- No package should be retained by inertia: each release includes a removal/consumer audit. But don't remove a current dependency without proving its consumer has moved or has been intentionally eliminated.

## 12. Packaging, releases, and repository hygiene

Follow `WPF-AGENT-EXECUTION-PROTOCOL.md` for the full workflow. This section defines the release-specific bar for the 1.x/2.0 effort.

### Every minor 1.x release

- Write `docs/releases/<version>-plan.md` before implementation.
- Record baseline repo/branch/commit, relevant research and URLs/date, scope, exclusions, tasks, risks, test matrix and acceptance criteria.
- Implement tests with behavior changes; update docs/changelog/feature inventories in the same release.
- Run required gates, review `git diff`, audit dependencies/license changes, validate generated projects and package artifacts if applicable.
- Create commits on the intended version/release branch; push only when tests and policies allow. Don't force-push, rewrite history, or overwrite tags.
- Publish a release only when every blocking gate actually passed; include checksums for packages and exact test/performance evidence.
- Write `docs/releases/<version>-report.md` with real counts, executed/skipped test details, artifact hashes, exact CI runs/commit IDs/release links, performance hardware/profile, known issues and deferred items.

### 2.0 prereleases and stable

- Use explicit prerelease identifiers: `2.0.0-preview.1`, `2.0.0-preview.2`, and `2.0.0-rc.1` (or the same SemVer-compatible naming if the accepted per-version plan gives a documented reason).
- Keep the API/CLI/resource/package freeze from `rc.1` onward. Bug fixes may require `rc.2`; don't sneak features into a candidate.
- Verify package contents from the exact artifact to be released. Compute SHA-256 and attach the matching asset; don't publish a package built from a different commit than the release tag.
- The NuGet feed push, real certificate signing, and Store submission may require owner-controlled credentials. The agent must report them as owner steps if credentials/authorization aren't available; it must not invent success.
- Keep old 1.0.0 and other published release assets immutable. Never move existing tags or replace their published contents.

## 13. Documentation to maintain

- `README.md`: truthful current features, real screenshots, getting started, install commands, supported presets, supported platform/SDK, version badges and links.
- `CHANGELOG.md` + `docs/releases/`: 0.x milestone history (the legacy `WPF-Template-Roadmap.md` was removed in `1.1.0`; this document is the single source of truth from `1.0.0` onward).
- `WPF-AGENT-EXECUTION-PROTOCOL.md`: execution contract; update only if the user-approved workflow changes.
- `docs/DEFERRED.md`: every deferred item resolved, shipped, rejected with evidence, or assigned to a later release.
- `docs/DEPENDENCIES.md` and third-party notices: direct/transitive deps, versions, license evidence, targets, native assets, maintenance and removal audit.
- `docs/decisions/`: ADRs for UI library changes, public package boundaries, motion/water-warp, visual-regression strategy, notifications, Visual Studio integration, and public API migration.
- `docs/design-system/`: token contracts, theme and high-contrast behavior, control catalog, shell recipes, motion policy, accessibility standards, gallery contribution instructions.
- `docs/guides/`: add a page/control/service, theme application, module selection, update/notification integration, packaging/signing, diagnostics, and testing a generated app.
- `docs/migration/1.x-to-2.0.md`: working draft from 1.8, proofed in 1.9/2.0 RC, final published for stable 2.0.
- `docs/releases/`: an actual plan before each release and a truthful completion report after it. Do not rewrite old reports to claim tests or features that were not part of the original run.
- Screenshots/snapshots: include the pinned environment metadata, and keep only approved, legally usable visual assets.

## 14. Deferred backlog reconciliation

Use `docs/DEFERRED.md` as the source record, updating it when an item ships or its disposition changes.

| Existing item | Target milestone | Disposition rule |
|---|---|---|
| D01 — `Microsoft.Xaml.Behaviors.Wpf` | Research in 1.2–1.5 only if concrete need | Do not add if local behaviors/motion APIs solve the need with less complexity |
| D07r — authentication/crash-reporting samples | 1.6.0 | Explicit opt-in, privacy/security review, no default telemetry/secrets |
| D07t — system toast notifications | 1.6.0 | Optional Windows integration; validate packaged/unpackaged activation |
| D07s — real signing execution | 1.6.0 and release docs | Script verification is not signing; credentials owner-controlled |
| D08r — VSIX/Add-Item integration | 1.7.0 | Research and decide; deliver only if worth long-term support cost |
| D09r — visual regression snapshots | 1.1.0 research; 1.2.0 initial deployment | No paid-surprise package dependency; reviewed baselines; stable runner |
| D10 — low-spec reference machine | 1.1.0 acquisition/definition, continuous thereafter | Don't claim the 8 GB target until measured and reported |
| D12 — test-only ApplicationInsights transitive | Every dependency audit | Confirm remains test-only; remove if no consumer or creates unwanted telemetry/weight |
| D13 — HLSL water warp | 1.5.0 prototype | Ship only if all prototype gates pass; otherwise defer with evidence |
| D14 — drag-and-drop workspace | 1.4.0 | Prototype on an actual workspace; accessibility alternative required |
| D15 — top-nav and tabbed shells | 1.4.0 | Each must generate and run as a named/tested shell preset |
| D16 — full-gallery localization | 1.3.0 | Test all gallery pages and missing-resource fallbacks |
| D17 — chart theming | 1.3.0 | Verify light/dark/contrast, text readability and package isolation |
| D18 — TreeView/full DataGrid/TimePicker sample | 1.3.0 | Reuse/make a custom control only after WPF UI/native toolkit audit |
| D19 — per-service template flags | 1.6.0 | Exact source/package/reference/registration absence checks |
| D20 — template NuGet publishing | Already shipped in 1.0.0 | Keep package build/push flow accurate; artifact validity is a gate |

Newly identified risks (UI test skip visibility, template test isolation, hard-coded measurement path, stale README) should be logged as new IDs in `docs/DEFERRED.md` or closed directly by 1.1.0 with links to the corresponding issue/commit/report.

## 15. Agent operating rules specific to this roadmap

1. **Inspect before coding.** At the beginning of every milestone, inspect `git status`, recent commits/tags, the exact previous release plan/report, current CI, source files/tests, package versions, deferred backlog, and open repository issues relevant to the milestone.
2. **Fresh research for material decisions.** Reverify current package status/releases/licenses and cite sources in the version plan. Do not copy old research dates or package versions without checking them.
3. **Plan the full version first.** Create `docs/releases/<version>-plan.md`, including researched findings, repository-specific impact, task IDs, dependencies, out-of-scope items, test mapping, acceptance criteria, risk plan and release method—before production code or feature-test implementation changes begin.
4. **Keep strict scope.** Every task must map to the approved version plan. Record adjacent improvements in `docs/DEFERRED.md`; don't implement them because they are convenient.
5. **Incremental proof.** Add tests alongside each feature/fix. Run focused tests immediately, then integration/template/UI/performance gates at planned checkpoints.
6. **Do not weaken tests to get a green build.** Fix the root cause, classify genuine environment restrictions and require the alternative evidence, or mark the version blocked. A skipped required test is not a pass.
7. **Test the shipped artifact.** Source-tree tests do not prove the NuGet package works. Test the actual package artifact from a clean environment.
8. **Evidence only.** Record real test counts and commands, screenshots/diffs, performance metrics, hashes, CI run links and commit IDs. Never invent results.
9. **Protect the user's repository.** Never discard user changes, reset a branch to force a clean state, force-push, rewrite public history, move a published tag, or publish a release using unverified artifacts.
10. **Release only after all gates.** If credentials, a UI-enabled runner, the low-spec reference machine, or another required capability is unavailable, report the blocker and leave the release un-published rather than pretending it passed.
11. **Write the completion report.** Map acceptance criteria to evidence, list unresolved items and owner-only steps, verify the pushed commit/tag/artifact identity, and update roadmap/backlog/README/changelog.
12. **Keep 2.0 compatibility changes deliberate.** Breaking API/template/resource changes belong only in the approved 2.0 preview/RC path; document and test migration instead of silently changing 1.x.

## 16. Out of scope unless separately approved

- Replacing WPF UI solely for novelty or combining overlapping theme libraries by default.
- Shipping an HLSL/Direct3D effect before prototype and low-resource gates are satisfied.
- Mandatory cloud services, phone-home telemetry, analytics, authentication, or crash reporting.
- Making the dashboard/gallery a dependency of generated Clean applications.
- Claiming the 8 GB performance target based on an unreported or different machine.
- A full rewrite of navigation, DI, or public APIs without task-level migration evidence.
- A paid, license-restrictive, or native-heavy dependency added without impact/terms review.
- Automatically accepting visual snapshots or silently skipping UI tests.
- Advertising unsupported installer, signing, Store, notification, or update combinations.
- Large code formatting/refactoring outside the target release's scope.

## 17. Research references

All links below were checked as research leads on 14 October 2026. Package versions and repository state must be refreshed at the next milestone that depends on them.

### Repository sources — current implementation and baseline

- [WPF 11 App Template repository](https://github.com/Fettah010/wpf-11-app-template)
- [Stable v1.0.0 release and package asset](https://github.com/Fettah010/wpf-11-app-template/releases/tag/v1.0.0)
- [README](https://github.com/Fettah010/wpf-11-app-template/blob/main/README.md)
- [Product roadmap (this document, 1.0.0 → 2.0.0)](https://github.com/Fettah010/wpf-11-app-template/blob/main/WPF-11-App-Template-Roadmap-1.0-to-2.0.md)
- [Agent execution protocol](https://github.com/Fettah010/wpf-11-app-template/blob/main/WPF-AGENT-EXECUTION-PROTOCOL.md)
- [Deferred backlog](https://github.com/Fettah010/wpf-11-app-template/blob/main/docs/DEFERRED.md)
- [1.0.0 completion report](https://github.com/Fettah010/wpf-11-app-template/blob/main/docs/releases/1.0.0-report.md)
- [CI workflow](https://github.com/Fettah010/wpf-11-app-template/blob/main/.github/workflows/ci.yml)
- [Template matrix script](https://github.com/Fettah010/wpf-11-app-template/blob/main/eng/test-templates.ps1)
- [UI automation tests](https://github.com/Fettah010/wpf-11-app-template/tree/main/tests/DevTemWpf.UiAutomation)
- [Current dependencies](https://github.com/Fettah010/wpf-11-app-template/blob/main/docs/DEPENDENCIES.md)

### Comparable implementations

- [ChainJG/WpfTemplate](https://github.com/ChainJG/WpfTemplate) — token-based visual system, animated custom controls, adaptive rail, effects and gallery.
- [Dane-ME/WPFTemplate](https://github.com/Dane-ME/WPFTemplate) — layered project structure and navigation lifecycle/cancellation patterns.
- [WPF UI](https://github.com/lepoco/wpfui) · [releases](https://github.com/lepoco/wpfui/releases) · [license](https://github.com/lepoco/wpfui/blob/main/LICENSE).
- [ModernWPF](https://github.com/Kinnara/ModernWpf) · [releases](https://github.com/Kinnara/ModernWpf/releases) · [roadmap](https://github.com/Kinnara/ModernWpf/blob/master/docs/roadmap-1.0.md).

### Testing, accessibility, and performance

- [FlaUI](https://github.com/FlaUI/FlaUI) · [releases](https://github.com/FlaUI/FlaUI/releases).
- [Accessibility Insights for Windows](https://github.com/microsoft/accessibility-insights-windows).
- [Verify snapshot testing tool](https://github.com/VerifyTests/Verify) · [Verify.ImageMagick](https://github.com/VerifyTests/Verify.ImageMagick) · [Verify maintenance fee](https://github.com/VerifyTests/Verify/blob/main/docs/maintenance-fee.md).
- [Microsoft WPF performance overview](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/performance).
- [Microsoft performance planning](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/planning-for-application-performance).
- [Microsoft WPF application performance optimization](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/optimizing-wpf-application-performance).
- [Microsoft WPF layout/design optimization](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/optimizing-performance-layout-and-design).

### Windows integration

- [Windows notifications overview](https://learn.microsoft.com/en-us/windows/apps/develop/notifications/).
- [App notifications quickstart](https://learn.microsoft.com/en-us/windows/apps/develop/notifications/app-notifications/app-notifications-quickstart).

---

## 18. Final roadmap rule

The intended destination is a **modular, premium Windows 11 WPF foundation with a tested template generator**, not an app that merely looks impressive in the gallery. Execute the releases in order: stabilize and make the current gates truthful; improve the UI; complete the controls and shell patterns; prototype and measure advanced effects; expand optional services and tooling; prove the modular architecture; then migrate and validate the public contract for 2.0.

Every milestone must earn its release through real evidence. When an idea fails a performance, accessibility, licensing, maintainability, or compatibility gate, the correct outcome is to improve it or defer it with a documented reason—not to ship it anyway.
