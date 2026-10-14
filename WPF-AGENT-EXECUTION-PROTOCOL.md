# WPF 11 App Template — Agent Execution Protocol

> **Purpose:** Permanent operating rules for AI coding agents maintaining the stable WPF 11 App Template from `1.0.0` through the `2.0.0` major release and beyond.
>
> **Execution mode:** Autonomous within the approved roadmap; deep, evidence-based research; strict version boundaries; incremental implementation and testing; and automatic commit, pull-request, integration, and release operations only when every applicable gate passes and repository permissions allow them.
>
> **Document role:** This file defines *how* work is planned, implemented, tested, and delivered. The canonical product roadmap defines *what* should ship and in which order.

---

## 1. Mission and governing documents

Build a premium, Windows 11-first WPF application template and reusable UI foundation. Follow the existing product roadmap: WPF UI as the primary Fluent foundation; CommunityToolkit.Mvvm; Microsoft.Extensions.Hosting/DI where appropriate; reusable shell, design-token, control, and motion layers; optional premium effects; clean/recommended/full/gallery presets; modular production services; tested scaffolding and distribution; and measurable performance and accessibility.

Before doing any work, locate and read:

1. This protocol.
2. The canonical roadmap: `WPF-11-App-Template-Roadmap-1.0-to-2.0.md`.
3. The repository's agent entry point (`AGENTS.md` or equivalent), `README.md`, `CHANGELOG.md`, `Directory.Build.*`, `Directory.Packages.props`, solution/project files, CI workflows, relevant tests, dependency/license inventories, and applicable architecture decision records (ADRs).
4. The immediately previous version's plan and completion report, plus the target version's plan/report, tag, release, and artifacts if they already exist.
5. `docs/DEFERRED.md` (or its established equivalent), the relevant roadmap milestone, the current source/tests, actual Git state, and latest *verified* CI results.
6. The legacy `WPF-Template-Roadmap.md` only for historical context or unresolved legacy decisions. It is not a second source of release order once the canonical 1.0-to-2.0 roadmap is present.

Do not create duplicate roadmaps or silently move, rename, or replace existing project documents. If the repository uses different paths, follow its established structure and record the resolved paths in the version plan.

### Authority order

Apply these rules in this order:

1. Explicit user instructions for the current task.
2. This protocol for process, scope discipline, safety, validation, and delivery.
3. `WPF-11-App-Template-Roadmap-1.0-to-2.0.md` as the **single source of truth for release order, milestone scope, and intended product outcomes**.
4. Accepted ADRs and established repository conventions, provided they do not conflict with the current user instruction or canonical roadmap.
5. The current version's plan, which operationalizes the relevant roadmap milestone but may not broaden it.
6. Existing code, test results, package metadata, and third-party examples, all of which must be checked rather than presumed correct.

Do not duplicate the release sequence into this protocol or create a competing version roadmap. If the canonical roadmap is missing, unreadable, or materially inconsistent with repository state, do not guess the next milestone: inspect releases/tags/reports, record the discrepancy, and block implementation until the authoritative sequence is available or the user explicitly resolves the conflict. The historical `WPF-Template-Roadmap.md` may inform context but must not silently override the canonical roadmap.

A version plan may refine tasks and verification details but may not silently rewrite roadmap scope. Propose material roadmap changes with evidence, impact, and migration consequences; do not implement out-of-scope work while waiting for that decision.

## 2. Non-negotiable operating rules

1. **Plan before implementation.** Every version has a separate substantive plan, saved and readiness-checked before production code or new implementation tests for that version are changed.
2. **Autonomous within approved scope.** Make routine decisions using current evidence and repository conventions. Do not ask for approval for work already authorized by the roadmap; do not treat autonomy as permission to broaden scope.
3. **The roadmap is the release authority.** Use the canonical roadmap for the next version, order, and boundaries. Do not skip, combine, reorder, or begin future milestones early.
4. **Research must affect a decision.** Compare real alternatives, record current sources/dates and repository-specific consequences, and validate material uncertainty with code inspection, a prototype, tests, or a benchmark.
5. **Test as you build.** Add regression coverage with fixes and tests with features. Run focused checks immediately and complete the relevant full gate before release.
6. **Evidence, not assertion.** Record exact commands, environment, outputs, CI links, visual evidence, artifact hashes, and measured values. A planned run is not a pass.
7. **No false-green release.** A skipped required UI test, unverified required check, unexplained warning, broken supported preset, missing required evidence, or invalid artifact blocks the affected release gate.
8. **Keep the repository recoverable.** Preserve user changes and history; never force-push, rewrite shared history, move/delete published tags/releases, expose secrets, or bypass branch protections.
9. **Optional means removable.** Clean scaffolds must not inherit gallery/dashboard content or unselected services, integrations, native dependencies, source files, registrations, or runtime behaviour.
10. **Prefer the smallest justified design.** Avoid speculative abstractions, duplicate theme libraries, unnecessary dependencies, hidden network behaviour, and public APIs unsupported by a demonstrated use case.
11. **Performance and accessibility are product requirements.** Motion/effects need fallback paths and reduced-motion support; measure relevant changes; never present proxy-machine results as proof of the 8 GB target.
12. **Visual evidence is controlled.** Critical visual-regression cases are blocking after the baseline and comparison environment are established; never accept changed snapshots mechanically.
13. **Breaking changes are deliberate.** Preserve the 1.x contract through 1.x releases. Put approved breaking public API/template/resource/project-layout changes inside the 2.0 preview/RC path and supply tested migration guidance.
14. **Delivery must be truthful.** Distinguish local commit, pushed branch, merged PR, tag, GitHub Release, GitHub artifact, and NuGet publication. Report each separately.

## 3. Release sequence and scope control

The canonical release order and milestone scope live only in `WPF-11-App-Template-Roadmap-1.0-to-2.0.md`. Do not maintain a copied version list in this protocol. Before acting, identify the next unfinished milestone from the roadmap and reconcile it with the actual default branch, tags, published releases, plans, reports, and CI. The current repository baseline is expected to be stable `1.0.0`, but that must be verified at execution time rather than assumed from this text.

### 3.1 In-scope rule

A task is in scope only when it directly advances the current roadmap milestone or is the smallest necessary prerequisite to make that in-scope work correct, secure, testable, or maintainable. Every implementation task maps to a stable task ID in the version plan and one or more acceptance criteria.

When unrelated improvement is discovered:

1. Do not implement it in the current release.
2. Add or update a concise entry in `docs/DEFERRED.md` (or the established equivalent): problem, evidence, user/developer value, likely roadmap milestone, dependencies, and risks.
3. Continue the authorized work.

If an in-scope change exposes a genuine earlier-foundation blocker, make only the minimum prerequisite change, record the reason, and add regression coverage. Do not use a prerequisite as a reason for unrelated refactoring.

If the roadmap and current state cannot be reconciled safely, complete independent in-scope tasks where possible, document options/evidence, and stop the disputed change. Ask the user only when safe progress is impossible or an explicit product decision would change.

### 3.2 Milestone-sensitive controls

The following are **process gates, not a duplicate roadmap**. The canonical roadmap retains authority over order, outcomes, and complete milestone scope.

- **Early 1.x stabilization:** prioritize demonstrable correctness, honest UI-test execution, template-test isolation, documentation consistency, dependency/license checks, and trustworthy baselines before layering features on false-green checks.
- **Visual-system and component milestones:** require before/after captures of the affected critical surfaces, theme and resizing checks, keyboard/focus validation, and documentation for reusable controls. Do not treat decorative richness as sufficient acceptance.
- **Workspace and navigation milestones:** verify focus, history, cancellation, disposal, repeated/rapid navigation, drag/drop alternatives, resizing, and shell generation where applicable.
- **Motion/effects milestones:** use a written prototype plan with a go/no-go decision. Compare against a static fallback, measure the actual effect, test reduced motion/high contrast, and do not make an experimental shader a Clean-preset or application-startup dependency.
- **Service and distribution milestones:** validate feature presence *and absence* in generated source, project references, package closure, DI registration, configuration, and runtime behaviour. Keep channel claims separate: an unsigned script is not a signed installer, and an unpushed package is not a published package.
- **Generator and IDE-tooling milestones:** test repeatable generation into a temporary consumer app, naming/namespace permutations, existing-target collisions, accurate help text, and every supported generated result. Do not ship a Visual Studio extension unless evidence justifies its long-term maintenance cost.
- **Architecture preparation before 2.0:** map APIs/resources/options, prototype library boundaries in isolation, build downstream consumer canaries from real package artifacts, measure the result, and record an ADR before migrating the main product.
- **1.x freeze/migration preparation:** no major API or template breakage is allowed to slip into the 1.x maintenance path. Establish migration mappings and reproducible 1.x canaries before 2.0 previews.
- **2.0 preview and RC:** require separate independently consumable packages, clean generated-app proof, migration examples/tests, package identity/content checks, and public-surface freeze at RC. The RC accepts only release blockers and necessary corrective changes, not feature additions.
- **2.0 stable:** verify migration documentation against actual canary conversions, downstream package consumption, stable artifact identity, and the exact release tag/commit relationship.

### 3.3 Roadmap updates

Update milestone status only after evidence exists. Do not alter future milestone intent during implementation. If real research proves a milestone outcome invalid, write a proposal describing the evidence, alternatives, scope/version impact, API compatibility, and release consequences; do not silently edit the canonical roadmap as an implementation convenience.

## 4. Repository state and safety preflight

Before each version:

- Inspect `git status --short --branch`, current branch, recent commits, remotes, tags, and the previous release/plan/report.
- Identify uncommitted and untracked files. Treat them as user work unless proven otherwise. Never discard, overwrite, stage, or commit unrelated user changes.
- Check whether the target version already has commits, a tag, a published release, or a plan. Do not duplicate or move existing release tags. Resume from verified state when appropriate.
- Identify available SDKs, OS, build tools, test runners, signing/publishing tools, remote permissions, and CI workflows. Record relevant environment constraints.
- Confirm baseline build/test status before implementation. Separate pre-existing failures from failures introduced by this milestone.
- Check the default branch and repository contribution/release conventions. Respect protected branches, required checks, and existing automation.
- Do not expose tokens, certificates, connection strings, private paths, or other secrets in logs, plans, commits, screenshots, or release notes.

If the repository has unrelated dirty changes, work around them safely on a separate branch when possible. Do not claim a clean baseline unless the Git status proves it.

## 5. Mandatory research phase for every version

Research starts with the current codebase and tests, then expands to current external evidence for questions that materially affect the milestone. Reuse previous research only after verifying that it is still current and applicable.

### 5.1 Inspect before proposing

At minimum, inspect the relevant:

- Project structure, public APIs, dependency graph, build configuration, packaging, and template files.
- Existing controls, resource dictionaries, ViewModels, services, tests, and architecture decisions.
- Issues and technical debt that directly affect the milestone.
- Baselines for build time, startup, memory, rendering, navigation, large lists, or other relevant performance concerns.

Do not recommend a large rewrite without evidence from the current implementation.

### 5.2 External research expectations

When a milestone changes or relies on a package, framework API, Windows integration, animation/effect technique, installer, test tool, or template-engine capability, check appropriate current primary sources. Prefer:

1. Official package documentation and supported target-framework information.
2. Official release notes, tags, changelogs, source code, and license files.
3. Maintainer documentation and relevant open/closed issues or discussions.
4. Reproducible benchmark data or a small local prototype when performance/compatibility is uncertain.
5. Secondary articles only to supplement primary evidence, never as the sole support for a significant dependency or architecture decision.

For package candidates, review only as relevant but explicitly consider: exact approved version, release/stability status, framework compatibility, maintenance/release cadence, license, direct and transitive dependencies, native components, security notices, issue health, footprint/startup implications, API fit, alternatives, and removal/migration cost. Prefer stable, permissively licensed dependencies. A preview package requires a documented, material benefit and a risk/fallback plan.

For a new animation or rendering effect, study WPF's real rendering and layout behaviour. Prototype and profile significant uncertainties. Do not assume that WinUI 3 composition/shader capabilities map directly to WPF. Water-warp/distortion and other expensive visual effects must remain optional and have a static fallback.

### 5.3 Research quality: actionable findings only

For each significant question, record:

- **Question:** the decision the project must make.
- **Options considered:** viable candidates, not strawmen.
- **Evidence:** direct URLs, relevant version/tag or documentation section, and access/verification date.
- **Finding:** what the evidence establishes and what remains uncertain.
- **Impact:** compatibility, maintainability, performance, visual quality, accessibility, package footprint, or developer experience.
- **Decision/recommendation:** what the agent will do in this version and why.
- **Validation:** test, prototype, benchmark, build, or source inspection used to confirm it.

A list of links without conclusions is not research. A recommendation with no evidence for a material technical claim is not research. Keep research focused on the version's real decisions; do not repeat the entire ecosystem survey every milestone unless the landscape or project needs have changed.

For external claims, include links and dates in the plan, research note, ADR, or report. Do not invent benchmark numbers, package status, license details, or CI results. Distinguish measured values from estimates and unverified expectations.

### 5.4 When to create or update an ADR

Create an ADR for material choices that influence future milestones or public surfaces, including changes to the UI foundation, app architecture, public control APIs, animation engine, template-engine strategy, distribution architecture, or dependency suite. Each ADR should state context, options, decision, evidence, trade-offs, consequences, and revisit triggers.

Do not silently reverse an accepted ADR. Research the case for change, document the alternative and migration impact, and stop the disputed change if it falls outside existing approval. Small implementation choices do not need an ADR.

## 6. Mandatory per-version Markdown plan

**The version plan must be written and reviewed before implementation begins.** Research, repository inspection, baseline verification, and planning documents are allowed during this stage. Do not begin production-code implementation or implement new feature tests until the plan is saved and internally checked against the roadmap.

Default location:

`docs/releases/<version>-plan.md`

For example: `docs/releases/1.1.0-plan.md` when repository state confirms it is the next unfinished milestone. Always verify the actual target against the canonical roadmap and live release/tag/report state.

Use existing repository conventions when equivalent folders already exist; do not create duplicate documentation systems.

### 6.1 Required plan contents

Every version plan must contain the following sections, omitting one only with an explicit reason:

1. **Metadata and status:** version, milestone name, date prepared, source branch/commit, roadmap section, plan owner/agent, status (`Draft`, `Ready`, `In Progress`, `Blocked`, or `Complete`).
2. **Current-state summary:** relevant code and architecture as actually found; previous version outcome; baseline build/test/performance status; known blockers.
3. **Research and analysis:** specific questions, options, evidence URLs and dates, findings, recommendation, and validation approach.
4. **Version outcome:** concise user/developer-facing outcome and measurable success definition.
5. **Scope:** explicit in-scope work and explicit non-goals. Include features that must *not* be pulled forward from later versions.
6. **Design/architecture:** affected components, dependency boundaries, API/resource impact, lifecycle/async behaviour, configuration and fallback paths; ADR references when required.
7. **Task breakdown:** ordered, small, verifiable tasks with stable IDs such as `T01`, dependencies, likely paths/files, and a definition of done. Keep tasks at useful implementation level, not line-by-line instructions.
8. **Acceptance criteria:** observable, measurable criteria. No vague items such as “make the UI polished” without concrete evidence.
9. **Test plan:** mapping from each feature/acceptance criterion to unit, integration, UI, template, accessibility, packaging, or manual verification as appropriate. Include actual commands or establish exactly how they will be selected.
10. **Performance plan:** which metrics matter, reference hardware/configuration, measurement procedure, baseline/comparison, budgets or a stated reason why the budget is not yet ratified, and fallback expectations.
11. **Accessibility and resilience:** keyboard/focus, scaling/resizing, theme and reduced-motion implications, error handling, fallback, and optional-feature behaviour relevant to this version.
12. **Dependency/security/license impact:** new/updated/removed packages, compatibility, license evidence, transitive/native footprint, and required approval/ADR.
13. **Documentation and examples:** README, component docs, CLI help, changelog, screenshots, migration/configuration instructions, and release notes to update.
14. **Risks and mitigations:** technical uncertainty, dependency risk, regression surface, performance risk, packaging or CI uncertainty.
15. **Git and release approach:** intended branch, sensible commit boundaries, tag/release artifacts, workflows, and any external permissions required.
16. **Exit gate:** the exact checks that must pass before the version can be considered complete.
17. **Deferred work:** explicitly identified items that are not part of this version.

### 6.2 Plan readiness checklist

Before changing implementation code, confirm:

- [ ] The correct roadmap milestone and previous version state were verified.
- [ ] The repo state and baseline test results are recorded.
- [ ] Significant technical decisions have current evidence and reasoned recommendations.
- [ ] Every implementation task maps to current-version scope.
- [ ] Every acceptance criterion has a means of verification.
- [ ] Test, performance, accessibility, security/license, and documentation impacts have been considered.
- [ ] Risks and rollback/recovery options are defined.
- [ ] The plan does not silently change architecture, roadmap, public scope, or dependency policy.

Once ready, mark the plan `Ready`, then begin implementation. Update status and checkboxes as work proceeds; preserve the original intent and explain material factual adjustments. Do not quietly loosen acceptance criteria to fit the implementation.

## 7. Implementation discipline

Implement one coherent slice at a time. For each task ID:

1. Re-read the relevant plan/acceptance criterion.
2. Make the smallest maintainable change that satisfies it.
3. Add or update tests in the same slice. A bug fix normally includes a regression test.
4. Run focused tests/builds immediately.
5. Inspect the code diff and test output, then record the evidence in the plan or working notes.
6. Mark the task complete only when its code and verification are complete.

After each meaningful slice, keep the repository buildable where practical. Do not defer all tests, architecture validation, or integration work until the end. Do not commit known-broken intermediate states merely to create activity; use small, coherent, passing commits when possible.

### Engineering standards

- Follow existing conventions first. If conventions are missing, define them once and document them.
- Use CommunityToolkit.Mvvm and the selected host/DI architecture as defined in the roadmap; avoid competing MVVM/application frameworks without an approved ADR.
- Keep business logic out of XAML code-behind. Code-behind is acceptable for isolated window lifecycle, WPF event routing, animation orchestration, or native interop that genuinely belongs at the view boundary.
- Keep UI-library code distinct from sample/dashboard content, app services, template-engine code, and packaging scripts.
- Prefer composition, resource dictionaries, tokens, small adapters, and documented extension points over forking the primary UI toolkit.
- Avoid duplicated competing themes/control libraries. A new overlapping suite must be supported by a comparison, proof-of-concept, conflict review, license review, and ADR.
- Keep effects opt-in and independent. Every enhanced effect must support a static/non-effect fallback; essential controls must still work when the effect fails or is disabled.
- Honour cancellation, async errors, app shutdown, resource disposal, and UI-thread rules.
- Avoid hard-coded UI text in reusable control templates when localization is required.
- Do not add telemetry, authentication, remote calls, scheduled tasks, or hidden background services without explicit feature selection and clear documentation.
- Do not introduce a large abstraction, package, or public API solely for a hypothetical future use case.

### Mid-version plan changes

A plan can be clarified when repository discoveries invalidate an assumption. For any meaningful adjustment:

1. Record the new evidence.
2. Explain the impact on tasks, APIs, tests, risks, and acceptance criteria.
3. Keep changes inside the same milestone.
4. Preserve the original requirement unless the roadmap itself proves it incorrect.

If the change effectively adds a feature, changes a public contract, replaces a chosen library, changes a distribution strategy, or expands supported scope, do not implement it as an incidental adjustment. Defer it or document a roadmap-change proposal for user decision. Fixing a defect needed to satisfy existing acceptance criteria is not scope expansion.

## 8. Build, test, performance, and accessibility gates

Testing is continuous, and the gate is strict for every release. The *applicable* verification grows with the milestone; do not claim that a category passed when it is not yet implemented or could not be run. The version plan must identify what applies and why.

### 8.1 Required general checks

For each version, run the relevant repository-defined commands and record the exact command, environment, exit code, and a concise result. At minimum, consider:

- Restore using the documented SDK and package configuration.
- Build affected projects and then the full solution in Debug and Release configurations as applicable.
- Unit tests for affected logic and ViewModels.
- Integration tests for affected DI, configuration, persistence, update state, lifecycle, and packaging behaviour.
- Static analysis, formatting, analyzers, and warnings review.
- Dependency, license, and security checks appropriate to the repository.
- UI launch/smoke tests and manual visual review for UI changes.
- Template generation/build/run checks when template or generator code changes.
- Accessibility and reduced-motion checks when shell, controls, themes, motion, or effects change.
- Performance measurements when startup, rendering, animation, layout, data volume, or service initialization changes.
- Packaging/install/update/uninstall/recovery checks when distribution code changes.

Do not invent universal command names. Discover the commands and scripts actually defined by the repository, improve them in scope when necessary, and document canonical commands in `docs/TESTING.md` or the established equivalent.

### 8.1.1 Required UI automation policy

- The required UI automation tier must run on a supported Windows CI runner for pull requests and final release validation. The workflow may use a lighter gate on unrelated branch pushes, but a successful lightweight run is not a substitute for required PR/release UI coverage.
- The current repository's `ShellFlowsTests.cs` uses opt-in configuration and can report UI cases as skipped. A passing generic `dotnet test` is therefore insufficient until the workflow explicitly enables the UI tier and verifies that required cases executed.
- Each required UI test must have an unambiguous result: pass, fail, or not-run with a reason. In the required CI job, unexpected skip/not-run is a failure. Environment-based opt-in may remain for local convenience, but it must not silently bypass the PR/release gate.
- Use stable automation properties and semantic UIA patterns such as Invoke/Selection/Value where reliable. Avoid coordinate-only clicks and fixed sleeps where state polling or accessible patterns can make the test more robust. Preserve diagnostic UI-tree dumps and logs on failure without exposing secrets.
- If UI automation cannot run on the current local machine, use the configured Windows CI runner and verify the completed run. An unobserved/queued CI job is not a pass. If the required Windows job is unavailable or fails, block the release and report the specific limitation.

### 8.2 Failures and environment limitations

A failing test is a real result, not a reason to remove the test. Reproduce and diagnose it; determine whether it is pre-existing or introduced; fix in-scope causes; add a regression test where appropriate.

If a required test cannot run in the current environment (for example, WPF UI automation requires Windows), state precisely why and use a configured Windows CI runner if available. Confirm its actual result. A planned CI run or an unobserved workflow is not a pass. If required verification remains unavailable, do not publish the release; report it as blocked and preserve the reproducible work.

Do not hide warnings, add broad test skips, loosen thresholds without evidence, substitute mock tests for real generated builds, or rely on screenshots alone for functional correctness.

### 8.3 Generated applications are products

A successful build of the template source repository does not prove that a generated project or published template package works. Test the actual generated files; when a NuGet template package is part of the release, generate from that exact `.nupkg` as a separate gate from generating from the source folder.

Use a **hybrid feature-matrix policy**:

1. Fully validate every named preset (`Clean`, `Recommended`, `Full`, and any later documented preset) whenever it is claimed to be supported.
2. Validate every advertised independent feature flag in both its enabled and disabled state, including dependency/source/registration absence when disabled.
3. Run pairwise coverage over supported feature-flag interactions plus targeted high-risk combinations (UI foundation, navigation shell, tests, dashboard/charts, logging, SQLite, updater, tray, notifications, localization, packaging, and optional auth/crash reporting as applicable).
4. Exhaustively test all combinations when the matrix remains small enough to be practical. If it grows too large, document the computed matrix, pairwise method, high-risk cases, and residual risk in the plan.
5. Test each changed item template/generator on a real generated consumer and verify that repeated invocation, name/namespace substitution, collisions, and additions to an existing app are handled correctly.

For each tested combination:

- Scaffold into a clean temporary directory outside the repository, using an isolated .NET CLI home/template-registration environment where feasible. Do not permanently modify the agent's global `dotnet new` registrations.
- Check generated project names, namespaces, paths, SDK/framework versions, configuration, feature manifest, references, and documentation.
- Search for unresolved tokens, `DevTemWpf` residue where renaming is expected, absolute local paths, missing resources/pack URIs, duplicate/unused references, broken links, preprocessor/template residue, and disabled-feature files or dependencies.
- Restore/build with warnings treated as errors, run included tests if selected, and execute Windows launch/smoke/UI checks where applicable.
- Inspect package/output contents to prove that excluded modules are actually absent when the preset contract promises absence.
- Preserve per-run logs/results and clean only the temporary artifacts created by the test.

Do not claim “all combinations passed” if only presets were tested. Report the exact matrix and coverage method.

### 8.4 Performance policy

- Start or maintain reproducible baselines for startup to window/interactive readiness, memory after startup, navigation latency, scrolling/virtualization, idle CPU, and animation/effect cost as applicable.
- Identify modern and low-spec Windows 11 reference environments. The target low-spec reference is a modest CPU, 8 GB RAM, integrated graphics, and SSD; report actual CPU/GPU/RAM/storage, OS/build, power mode, resolution/DPI, display refresh rate, and test configuration when available.
- Be explicit about hardware evidence. A real 8 GB reference-machine run is the only direct validation for that target. A faster machine, virtual machine, runner, synthetic profile, or estimate may provide useful proxy evidence but must be labeled **proxy/provisional**, never represented as equivalent proof.
- If the reference machine is temporarily unavailable, continue unrelated work and report the limitation. A milestone may ship only if its approved acceptance criteria do not require that missing proof and no claim is made that the 8 GB gate passed. If a release's blocking acceptance criteria require an actual 8 GB result (especially a rendering/effect go/no-go), defer the affected feature or mark that gate/version blocked rather than substituting proxy evidence.
- Compare measurements under stated conditions and report medians, sample count, variance/range, and meaningful worst-case observations where practical. Separate measured data from estimates.
- Prefer no animation or lightweight transforms for common interaction. Avoid unnecessary perpetual animations, repeated layout invalidation, huge blurred/translucent surfaces, and expensive render work on hidden content.
- Keep a sensible `Auto` mode and user-accessible `Balanced`/`Rich` behaviour as the motion/effects system develops. Effects must not delay an essential action.
- The `1.0.0` report's approximately 0.81-second startup median and 146.6 MB working set are historical measurements from the available machine, not proof of the 8 GB target. Reproduce them with the current code and methodology, record hardware/configuration, and establish or revise budgets through an evidence-backed decision in the stabilization milestone specified by the canonical roadmap. Never reuse an old number as a fresh result. Once a budget is ratified, do not relax it silently.
- A water-warp or similar experiment must be compared with its static fallback, can be independently disabled, and must not be required for legibility or navigation.

### 8.5 Accessibility and visual verification

Where relevant, verify:

- Keyboard-only operation, logical tab order, visible focus, automation names, accessible state, access keys, and focus restoration after dialogs/navigation.
- Readable contrast and usable high-contrast behaviour; do not rely on accent colors, blur, translucency, or motion alone to convey state.
- Reduced-motion behaviour and settings persistence; system animation/accessibility preferences must be handled correctly.
- Light/dark/system theme changes and safe fallback when a material or effect is unavailable.
- DPI scaling, resizing, window restore/maximize/snap, narrow/collapsed navigation, text scaling where supported, and prevention of clipped/overlapping content.
- Critical visual-regression snapshots for agreed reference surfaces, themes, shell/layout states, and high-risk components once a stable capture environment exists. Review every changed baseline intentionally; never accept snapshots mechanically.

**Visual regression policy:** establish deterministic Windows CI capture conditions (OS/runner, DPI, window size, theme/accent, font availability, animation state, test data, and wait-for-idle strategy). Blocking snapshots apply to the defined critical set, not automatically to every UI surface. A baseline mismatch must be inspected and classified as a real regression, an intentional approved design change, or environment noise. Intentional changes require a reviewed diff and updated expected image; do not simply regenerate all baselines to turn CI green. Keep functional/UIA tests because screenshots cannot prove input, focus, semantics, or state transitions.

A visual flourish must not obscure content, focus, hit targets, or state. Use motion to communicate cause and result, not to force users to wait.

## 9. Dependencies, licensing, security, and supply chain

Maintain centralized package versions where practical and an auditable dependency inventory. Use stable, maintained, permissively licensed open-source packages by default.

For every added/updated nontrivial direct dependency, record its purpose, exact version, upstream URL, license source, framework compatibility, transitive/native footprint, maintenance evidence, alternatives, test evidence, and removal cost. For transitive or native dependencies, record material licensing/security/size risks. Recheck any version or license statement against primary sources at implementation time; previous roadmap research is a starting point, not permanent proof.

Rules:

- No dependency without a demonstrated need or clear reduction in maintenance burden.
- No duplicate UI/theme framework added without the evidence described in Section 7.
- No silently changing licenses, vendoring source without license/notice handling, or copying substantial code without compliance checks.
- Do not claim a security audit is complete if tooling or package metadata was unavailable. Record the exact limitation.
- No secrets in Git, templates, generated README files, test output, or releases. Review changed files before staging.
- No runtime network request or telemetry by default in clean/recommended projects unless the preset explicitly includes and explains it.

## 10. Documentation and repository hygiene

Maintain `WPF-11-App-Template-Roadmap-1.0-to-2.0.md` as the canonical long-term product plan. Update its status only when supported by real evidence; do not rewrite future scope just because an implementation preference changed.

Recommended documentation structure (adapt to established repository conventions; do not duplicate equivalent files):

```text
WPF-AGENT-EXECUTION-PROTOCOL.md  # this permanent process contract
WPF-11-App-Template-Roadmap-1.0-to-2.0.md # canonical product roadmap
README.md                          # human-facing quick start and product overview
CHANGELOG.md                       # user-visible changes per version
Directory.Packages.props           # central package versions, if adopted

docs/
  releases/
    <version>-plan.md              # written before each version implementation
    <version>-report.md            # evidence-backed completion/release report
  decisions/
    ADR-0001-....md                # material architecture decisions
  research/                        # focused research notes if useful
  DEFERRED.md                      # out-of-scope improvement backlog
  TESTING.md                       # canonical test/verification commands
  PERFORMANCE.md                  # methodology, environments, baselines, budgets
  DEPENDENCIES.md                  # dependency/license inventory, if useful
```

Keep documentation proportional and decision-oriented. The goal is traceability, not paperwork for its own sake. The per-version plan and report are mandatory; create separate research notes only when the analysis is valuable beyond the version plan.

Update documentation in the same version as behaviour changes. Public controls need examples, parameters/behaviour, accessibility details where relevant, theme/token usage, and extension/removal guidance. New template options need help text and generated-project evidence. Feature removal/deprecation needs migration instructions when user projects could be affected.

The premium dashboard and gallery are disposable examples. They must never become required dependencies of the clean shell.

## 11. Git, commit, push, merge, and release automation

The user has authorized autonomous Git and release operations **when all applicable checks pass**. This permission does not authorize destructive history operations, bypassing repository protections, exposing secrets, or ignoring release gates.

### 11.1 Branch and commit policy

- Default to a dedicated version branch such as `work/1.1.0` or `release/1.1.0`, named according to repository convention and the actual target milestone.
- Do not implement a release directly on the default branch when branch-based work and pull requests are supported.
- Make small, meaningful commits that represent coherent, verified slices. Include relevant tests and documentation in the associated changes.
- Use a consistent, descriptive style (Conventional Commits if already adopted; otherwise `type(scope): summary` is acceptable). Do not manufacture commit messages that claim unverified outcomes.
- Before staging, inspect `git status` and the diff. Stage explicit project files rather than blindly adding every untracked file. Exclude secrets, local environment files, temporary benchmarks, generated build output, and unrelated user work.
- Before every commit, review `git diff --check`, staged diffs, and relevant tests. Do not commit knowingly broken or untested work except where the commit is explicitly a documentation-only planning commit and clearly labelled.

### 11.2 Push and integration

After the version implementation and local gates pass:

1. Re-run release-candidate checks from a clean or controlled checkout state and inspect the complete final diff.
2. Commit intended changes in coherent, reviewable slices and verify commit contents/history.
3. Push the version branch to the configured remote if credentials and permissions allow.
4. Create or update a pull request by default. State the milestone, summarize scope, link the version plan/report as appropriate, identify risks/breaking changes, and provide test/performance evidence. Keep unrelated changes out of the PR.
5. Verify required PR checks have completed successfully, including required Windows UI automation and critical visual-regression jobs when applicable. A green status with required tests skipped is not sufficient.
6. Merge automatically only after required checks pass and repository policy, branch protection, permissions, and any mandatory approvals allow it. If policy requires a human review, stop at that boundary rather than bypassing it.
7. Never bypass branch protection, disable a required check, force-push, or rewrite shared history. If a repository explicitly documents a different integration model, follow it only when allowed and document why it differs from the PR default.

If authentication, remote access, branch protection, mandatory review, or CI prevents an authorized step, stop at that boundary, preserve commits and results, and report exactly what remains. Do not claim a push, PR update, merge, or check passed unless the remote confirms it.

### 11.3 Release versioning and artifacts

Use the roadmap's version sequence and repository's established version source. Keep package versions, template metadata, app versions, changelog headings, tags, and release artifacts consistent. Never silently reuse or move an existing tag.

Before publishing:

- Verify the target version is correct and has not already been published differently.
- Confirm the working tree and staged diff contain only intended files.
- Verify all required local and remote CI checks are green.
- Run the complete applicable acceptance gate from the final candidate commit.
- Review/update `CHANGELOG.md`, README/install docs, release notes, package metadata, and migration notes as applicable.
- Build artifacts using documented, reproducible commands. Check artifact existence, names, versions, signatures if used, checksums, and install/run smoke behaviour where applicable.
- Confirm package licenses/notices and release assets are correct. Do not upload debug symbols, credentials, internal logs, or personal machine data inadvertently.

Then, if the remote configuration and permissions allow:

1. Create the target tag (typically `v<version>`) at the exact verified release commit. Confirm it does not already exist; never move, replace, or silently reuse a published tag.
2. Push the tag without altering existing tags.
3. Publish the GitHub Release with accurate notes and only the appropriate verified artifacts/checksums. Use prerelease status consistent with the roadmap and SemVer phase (`preview`, `rc`, stable).
4. Verify the remote tag, release page, assets, artifact version metadata, and SHA-256 checksums after publication.
5. Confirm that the release tag points to the same source commit from which the attached artifacts were built. Record proof in the completion report.

### Independent publication channels and NuGet

Treat GitHub Releases, NuGet, installers/Velopack, MSIX/Store, signing, and any other distribution channel as separate channel gates with explicit status. Publish a channel only when it is included in the roadmap/version plan, its exact artifact has been built and verified, required credentials are configured, and its channel-specific tests pass.

If NuGet credentials or owner authorization are unavailable, do not fabricate a push or report that NuGet publication succeeded. Where the roadmap permits independent delivery and the GitHub artifact/release gates pass, publish the valid GitHub Release and report NuGet as **not published / owner action required**, including the exact package, intended version, verification state, and remaining command/action. Do not label the overall milestone “fully published” if NuGet was a required channel; report partial channel completion honestly. If NuGet publication is a blocking acceptance criterion for that milestone, mark that criterion/version blocked until the owner action is complete and remotely verified.

Never push an unverified package merely because an authentication token exists. Verify the exact `.nupkg`, metadata, package contents, clean-install generation matrix, SHA-256, and target NuGet identity before publishing. Do not include local paths or credentials in package metadata.

### 11.4 Release blockers

Do **not** tag/publish if any of the following apply:

- A required restore/build/test/CI gate fails or is unverified.
- A required PR/release UI automation job did not execute its required cases, reports unexpected skips, or has no observed successful Windows CI result.
- A blocking critical visual-regression capture fails without an inspected, documented disposition; intentional baseline changes have not been reviewed.
- A required named template preset, advertised flag state, required pairwise/high-risk combination, or changed package-based generation path is broken or unverified.
- A feature or release claim requiring direct 8 GB reference validation has only proxy/higher-spec evidence.
- A required generated preset or distribution combination is broken.
- An in-scope critical/high-severity defect is unresolved.
- An architectural or public API change lacks its required decision record.
- Required package/license/security evidence is missing or unacceptable.
- Required performance/accessibility evidence is missing or breaches ratified limits.
- Release artifacts are missing, inconsistent, unverified, or contaminated by local-only dependencies.
- The repository's release protections or permissions do not allow the operation.

Fix the blocker if in scope and feasible. Otherwise mark the version `Blocked`, record evidence and a concrete recovery path, and do not ship it. Do not reduce the gate to obtain a release.

## 12. Per-version completion report

Required location:

`docs/releases/<version>-report.md`

For example: `docs/releases/1.1.0-report.md`.

Create/update the report once implementation has been validated; finish it before release publication where possible, then append the verified remote release URL and final identifiers after publication.

The report must include:

- **Outcome:** what actually shipped compared with the plan.
- **Scope audit:** each task/acceptance criterion as passed, failed, deferred, or blocked, with evidence.
- **Research decisions:** final decisions and ADR/dependency links; note material decisions rejected and why.
- **Tests and quality gates:** exact commands, environment, exit code/status, CI links/run identifiers where available, key results, and all skipped/unavailable checks with reasons.
- **Performance:** machine/specs, methodology, before/after values where relevant, budget status, and effect fallback observations. Do not report fabricated or incomparable numbers.
- **Accessibility/visual review:** key scenarios verified, screenshot references if useful, defects found/resolved, outstanding limitations.
- **Dependencies/security/license:** changes and audit results, including unresolved caveats.
- **Generated-project and packaging validation:** configurations actually exercised and outcomes.
- **Known issues and deferred work:** honest list, backlog references, and severity.
- **Git evidence:** branch, commit hashes, pushed commit, PR/merge reference where applicable.
- **Release evidence:** tag, remote release URL, artifact list/checksums, publication status, or exact blocker.
- **Recovery/reproduction:** commands and steps another developer/agent can follow to reproduce the result.

Use concise tables and evidence links. Avoid vague reports such as “all tests passed” without naming the checks.

## 13. End-of-version decision and next-version handoff

A version is complete only when its required acceptance criteria are verified, the completion report is written, in-scope docs are current, and the Git/release status is accurately recorded.

At the end of a successful version:

1. Update the roadmap's status for the completed milestone, based on report evidence.
2. Confirm the repository has no accidental uncommitted implementation changes. Do not erase legitimate user changes to force cleanliness.
3. Verify that the published tag/release refers to the intended commit, if publishing was possible.
4. Leave the next milestone unimplemented.
5. On the next version proceeding, repeat the **full research → plan → plan-readiness review → implementation → incremental testing → final audit → report → commit/push/integration/release** cycle.

Do not pre-implement the next version merely because the current version finished early. It is acceptable to record useful preparation or future decisions in the deferred backlog, research notes, or ADRs, but not to silently deliver future-version functionality.

## 14. Public API, resource, and 2.0 migration controls

### 14.1 Preserve the 1.x contract

For every `1.x` milestone, assume existing consumers may rely on documented or observable public types, XAML resource keys, theme tokens, control properties, DI registration methods, template CLI names/parameters, generated file layout, package IDs, settings/configuration shape, update/install behaviour, and extension points. Before changing any of them:

- Determine whether the surface is documented, generated, referenced by templates, or likely consumed downstream.
- Prefer additive or backwards-compatible changes within 1.x.
- If a breaking change is unavoidable for correctness/security, record an ADR, quantify the impact, provide a compatibility shim or migration note where viable, and obtain explicit roadmap/user authorization if the change exceeds the current milestone.
- Do not quietly insert 2.0 architecture changes into a 1.x release.

### 14.2 Make breaking changes explicit at the 2.0 boundary

The 2.0 migration may deliberately change API and package boundaries, but it must not be an undocumented rewrite. Before and during 2.0 previews:

1. Maintain an inventory of public APIs, resource keys, template parameters, generator output, package references, DI conventions, configuration/settings formats, and supported distribution paths.
2. Create a mapping of each 1.x surface to its 2.0 state: preserved, renamed, moved, replaced, deprecated, or removed. Explain rationale and consumer impact.
3. Provide migration guidance with concrete before/after examples for project references, `App.xaml` resources, control styling, navigation, service registration, template commands/options, and package identities as relevant.
4. Build migration-canary applications from known 1.x starting points. Exercise the documented changes, compile and launch migrated consumers, and test the resulting package from a clean temporary feed/path rather than relying exclusively on project references.
5. Freeze public API, resource, template, and package identity decisions by the roadmap's RC milestone. After that point, only release blockers or necessary corrective changes are allowed; a correction that materially changes the frozen surface requires a new RC and renewed migration/gate review.
6. Do not promise automated migration unless it is implemented, tested on representative canaries, and documented. A migration guide is mandatory even if an upgrade tool is explored.

### 14.3 Library boundaries require downstream proof

Do not split projects solely to create more assemblies. Every new library must have a clear responsibility, acceptable dependency closure, intentional public API, coherent resource-loading strategy, and an independently built and tested consumer. At minimum, prove the Clean/UI-only consumer, a recommended application, a full/gallery/services consumer, and relevant generated page/control samples. Validate XAML resource resolution and pack URIs in built/package-based apps and verify that the gallery is not required at runtime.

## 15. Required version-plan template

Use this outline when creating `docs/releases/<version>-plan.md`. Expand sections enough to make the plan actionable; do not fill them with generic text.

```markdown
# <VERSION> — <Milestone Name> Plan

- Status: Draft
- Prepared: YYYY-MM-DD
- Roadmap section: <heading/link>
- Starting commit/branch: <verified value>
- Previous version/release: <verified version, tag, report, commit>

## 1. Outcome and success definition
## 2. Current repository state and baseline
## 3. Research questions, evidence, and analysis
| Question | Options | Evidence and checked date | Finding | Decision and impact | Validation |
|---|---|---|---|---|---|

## 4. In scope
## 5. Explicitly out of scope
## 6. Design and architecture
## 7. Implementation tasks
| ID | Task | Dependencies | Likely areas/files | Definition of done | Verification |
|---|---|---|---|---|---|

## 8. Acceptance criteria
| ID | Observable criterion | Evidence required | Gate |
|---|---|---|---|

## 9. Test and CI plan (explicit UI execution / visual / template matrix gates)
## 10. Performance plan and budgets (measured vs proxy hardware evidence)
## 11. Accessibility, resilience, and fallback plan
## 12. Dependency, license, and security impact
## 13. Documentation and examples
## 14. Risks and mitigations
## 15. Git, integration, and release plan
## 16. Deferred work
## 17. Plan-readiness checklist
```

## 16. Required completion-report template

Use this outline for `docs/releases/<version>-report.md`.

```markdown
# <VERSION> — <Milestone Name> Completion Report

- Status: Complete / Blocked
- Completed: YYYY-MM-DD
- Plan: <link>
- Final commit: <hash>
- Pull request / merge: <verified URL and status, or exact blocker>
- GitHub Release: <URL/status or not published + reason>
- NuGet: <package/version and remote status, or not published + owner action required>
- Other channels (Velopack/MSIX/Store/signing): <each status or N/A with reason>

## 1. Delivered outcome
## 2. Acceptance criteria and task audit
| ID | Expected result | Outcome | Evidence |
|---|---|---|---|

## 3. Research and architecture decisions
## 4. Tests, CI, and exact commands
| Check | Environment | Command/run | Result | Evidence / limitation |
|---|---|---|---|---|

## 5. Performance results
## 6. Accessibility and visual verification
## 7. Generated template / package / installer verification
## 8. Dependency, license, and security audit
## 9. Known issues and deferred work
## 10. Git, merge, and independent release-channel evidence
| Channel | Required for this version? | Artifact/version/hash | Verification | Remote status / URL | Blocker or owner action |
|---|---|---|---|---|---|
## 11. Reproduction and recovery instructions
```

## 17. Release gate checklist

Complete this checklist for every version, marking items `N/A` only with a reason documented in the plan/report. A `N/A` must not be used to disguise missing verification.

### Plan and scope

- [ ] The roadmap and prior version report were read; current milestone was confirmed.
- [ ] A version-specific plan existed before implementation and contains concrete research/analysis.
- [ ] Every code change maps to an in-scope task or a documented prerequisite.
- [ ] Out-of-scope discoveries were deferred rather than implemented.

### Implementation and maintainability

- [ ] Code follows documented architecture and extension conventions.
- [ ] Tests were added/updated alongside relevant implementation.
- [ ] Public API, template options, and dependency changes are deliberate and documented.
- [ ] Errors, cancellation, optional features, and fallback paths are handled honestly.

### Verification

- [ ] Restore/build/test gates relevant to this version pass.
- [ ] Required CI checks are confirmed green, not merely queued or assumed.
- [ ] Required UI automation actually executed in Windows CI; required cases were not skipped or silently disabled.
- [ ] Blocking critical visual-regression cases are reviewed and pass, or an intentional baseline change has explicit evidence/review.
- [ ] The documented hybrid template matrix (named presets, changed flags, pairwise and high-risk combinations) is recorded and green.
- [ ] UI/accessibility/reduced-motion checks relevant to the change pass.
- [ ] Performance evidence is recorded and ratified budgets pass when applicable.
- [ ] Generated apps, package combinations, and distribution paths affected by the change are verified.
- [ ] Dependency, license, and security checks are documented.
- [ ] No known critical or release-blocking issue remains.

### Documentation and delivery

- [ ] README, changelog, testing guide, relevant component docs, and roadmap status are accurate.
- [ ] Completion report contains concrete evidence and lists limitations.
- [ ] Staged diff was reviewed; secrets, unrelated files, and accidental build artifacts are absent.
- [ ] Commits are meaningful and correspond to the intended version.
- [ ] Push/integration respected repository policy and required checks.
- [ ] Tag/release/artifacts were created only after gates passed and were verified remotely.
- [ ] If any delivery step was blocked, status is marked `Blocked` and no false release claim was made.

---

## 18. Standard final status message

At the end of each run, present a concise report with concrete evidence:

1. **Version and scope:** target version; what was delivered; what was deliberately deferred.
2. **Plan/report:** repository paths to the version plan and completion report.
3. **Verification:** exact important build/test/CI results and any skipped or unavailable gates.
4. **Performance/accessibility:** measured results or explicit statement of what was not measured.
5. **Git:** branch, commit hashes, push/PR/merge status.
6. **Release channels:** tag/URL and artifact list/checksums; GitHub Release status; NuGet status; installer/MSIX/Store/signing status where applicable. Mark each as published, not applicable with reason, or blocked/owner action required.
7. **Blockers/next step:** only actionable unresolved issues; do not begin the next version automatically in the same scope.

Never announce completion solely because code was written. Completion means evidence-backed acceptance. If a release gate fails, say so directly, preserve the work, and report the recovery path.

---

## 19. Initial adoption and next-run instructions

When placing this updated protocol in the repository:

1. Place it at the repository root as `WPF-AGENT-EXECUTION-PROTOCOL.md` (the path referenced by the existing repository is acceptable; ensure `AGENTS.md` names the same canonical path).
2. Place `WPF-11-App-Template-Roadmap-1.0-to-2.0.md` at the repository root. It is the single source of truth for release sequence and milestone scope. Keep the old `WPF-Template-Roadmap.md` only as historical reference unless a deliberate, documented migration removes it; do not treat both as equal authorities.
3. Ensure `AGENTS.md` (or the chosen coding agent's equivalent entry point) tells the agent to read this protocol and the canonical roadmap before work, then read the current plan/report and relevant ADRs/tests.
4. Inspect the real repository, branch, tags, published releases, current CI and local changes. The published `v1.0.0` release is the expected baseline, but verify it. Select the next unfinished milestone from the canonical roadmap; the first planned post-1.0 milestone is `1.1.0` only if the live repository and release evidence confirm that it has not already been started or released.
5. Write `docs/releases/<actual-next-version>-plan.md` after research and before implementation. Do not start implementation or feature-specific test changes until the plan passes the readiness checklist.
6. If the canonical roadmap is absent, do not silently fall back to the legacy 0.1.0–1.0.0 roadmap. Report the blocker, record what has been verified, and do not implement a guessed milestone.
7. Preserve legitimate uncommitted user changes. Do not overwrite unrelated `AGENTS.md` instructions; merge only the minimal required instruction if needed.

**Operating principle:** Inspect reality; research until material decisions are evidence-based; plan the complete current version before coding; implement only its scope; test continuously; validate the exact generated and released artifacts; report limitations honestly; and commit, push, merge, and publish only when the applicable gates pass and repository policy permits them.
