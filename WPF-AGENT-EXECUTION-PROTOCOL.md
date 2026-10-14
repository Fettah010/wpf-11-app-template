# WPF Template — Agent Execution Protocol

> **Purpose:** Permanent operating rules for AI coding agents implementing this repository, from `0.1.0` through `1.0.0`.
>
> **Execution mode:** Autonomous within the approved roadmap; strict version scope; deep, evidence-based research; continuous implementation and testing; commit, push, merge, and release only when the applicable gates pass and repository permissions allow it.
>
> **This is an execution contract, not the product roadmap.** The roadmap defines *what* the project should become. This protocol defines *how* every version must be researched, planned, implemented, verified, and delivered.

---

## 1. Mission and governing documents

Build a premium, Windows 11-first WPF application template and reusable UI foundation. Follow the existing product roadmap: WPF UI as the primary Fluent foundation; CommunityToolkit.Mvvm; Microsoft.Extensions.Hosting/DI where appropriate; reusable shell, design-token, control, and motion layers; optional premium effects; clean/recommended/full/gallery presets; modular production services; tested scaffolding and distribution; and measurable performance and accessibility.

Before doing any work, locate and read:

1. This protocol.
2. The existing roadmap, expected to be `WPF-Template-Roadmap.md` or an explicitly designated equivalent.
3. `README.md`, `CHANGELOG.md`, `Directory.Build.*`, `Directory.Packages.props`, solution/project files, CI workflows, relevant tests, and all applicable architecture decision records (ADRs).
4. The previous version's plan and completion report, if they exist.
5. The actual current Git state, existing project conventions, and latest passing CI results where available.

Do not create duplicate roadmaps or silently move, rename, or replace existing project documents. If the repository uses different paths, follow its established structure and record the resolved paths in the version plan.

### Authority order

Apply these rules in this order:

1. Explicit user instructions for the current task.
2. This execution protocol for process, scope discipline, validation, and delivery.
3. The existing product roadmap for goals, release boundaries, and planned capabilities.
4. Accepted ADRs and documented repository conventions.
5. Versioned implementation plans for the current milestone's agreed scope.
6. Existing code and third-party examples, which must be checked rather than presumed correct.

An implementation plan may refine a milestone but may not silently rewrite the roadmap. If a roadmap change becomes necessary, document the reason and impact as a proposal; do not implement unrelated future work to accommodate it.

## 2. Non-negotiable operating rules

1. **Plan before implementation.** Every version gets a separate, substantive Markdown plan containing research, analysis, scope, tasks, tests, and acceptance criteria before production code or implementation tests for that version are changed.
2. **Autonomous execution within bounds.** Do not pause for routine choices or ask for approval for work already covered by the roadmap. Use the established stack and documented defaults. Never treat autonomy as permission to expand scope.
3. **Strict version boundary.** Implement only the selected version's in-scope work and the smallest necessary prerequisites. Log worthwhile extras in the deferred-work backlog for a later version.
4. **Research must change decisions.** Research concrete technical questions, compare real alternatives, cite current evidence, and state the consequence for this repository. Do not fill documents with generic summaries or unverified claims.
5. **Test as you build.** Add or update tests alongside each feature or fix. Run focused checks immediately, then broader checks at milestones and before release.
6. **Evidence, not assertion.** A checkbox is complete only with a command result, test report, CI result, visual evidence, artifact check, or an explicitly recorded manual verification. Never claim a command was run if it was not.
7. **No false-green release.** Known failing required tests, unexplained build warnings, missing required evidence, or unverified release artifacts block a release. Fix the cause or mark the version blocked; do not weaken a test simply to obtain a pass.
8. **Keep the repository recoverable.** Preserve user changes and history. Never run destructive Git commands to make the tree appear clean; never force-push, rewrite shared history, delete a remote tag/release, or bypass branch protections.
9. **Optional means removable.** Clean scaffolds must not inherit the dashboard, gallery, database, updater, tray, telemetry, or other optional service just because those features exist in the reference app.
10. **Prefer the smallest justified design.** Avoid speculative abstraction, duplicate libraries, unneeded dependencies, hidden network behaviour, and public APIs unsupported by a real use case.
11. **Performance and accessibility are product requirements.** Rich motion and effects must have usable fallback paths, respect reduced-motion/accessibility settings, and be measured on representative hardware.
12. **Delivery must be truthful.** Report exact commits, test outcomes, skipped checks, limitations, push/merge/release status, and links where available. A local commit is not a push; a pushed tag is not proof that a GitHub Release was published.

## 3. Release sequence and scope control

Follow the sequence in the roadmap. Do not skip, combine, or reorder milestones without an explicit roadmap change. The expected sequence is:

- `0.1.0` — repository foundation and architecture decisions
- `0.2.0` — Windows 11 window experience and shell foundations
- `0.3.0` — design tokens, themes, component standards
- `0.4.0` — essential controls and forms
- `0.5.0` — motion system and optional effects
- `0.6.0` — advanced layouts, data controls, dashboard/gallery
- `0.7.0` — modular production services and distribution workflows
- `0.8.0` — template presets, CLI scaffolding, generators, developer tooling
- `0.9.0` — feature-complete beta, accessibility/performance hardening
- `1.0.0-rc.1` — API and template surface freeze, release-candidate validation
- `1.0.0` — stable release

The roadmap controls the precise milestone requirements. This list is a navigation aid, not permission to start work on later milestones early.

### What counts as in scope?

A task is in scope only when it directly advances the current milestone's roadmap objective or is required to make that in-scope work correct, secure, testable, or maintainable. Every implementation task must map to a plan task ID and one or more acceptance criteria.

When an unrelated improvement is discovered:

1. Do not implement it during the current release.
2. Add a concise entry to `docs/DEFERRED.md` (or the repository's established equivalent): problem, evidence, proposed value, likely milestone, and any dependency or risk.
3. Continue current work.

If an in-scope implementation exposes a genuine blocker in an earlier foundation, make only the minimal prerequisite change needed. Explain it in the plan/report and add regression coverage. Do not use a prerequisite as an excuse to refactor nearby unrelated code.

If the roadmap and the current milestone cannot be reconciled safely, finish independent in-scope work where possible, record the blocker and options, and do not make the disputed architectural or scope change silently. Ask the user only if the unresolved choice makes safe progress impossible or would change an explicitly approved product decision.

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

For example: `docs/releases/0.1.0-plan.md`.

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

### 8.2 Failures and environment limitations

A failing test is a real result, not a reason to remove the test. Reproduce and diagnose it; determine whether it is pre-existing or introduced; fix in-scope causes; add a regression test where appropriate.

If a required test cannot run in the current environment (for example, WPF UI automation requires Windows), state precisely why and use a configured Windows CI runner if available. Confirm its actual result. A planned CI run or an unobserved workflow is not a pass. If required verification remains unavailable, do not publish the release; report it as blocked and preserve the reproducible work.

Do not hide warnings, add broad test skips, loosen thresholds without evidence, substitute mock tests for real generated builds, or rely on screenshots alone for functional correctness.

### 8.3 Generated applications are products

Whenever templates, presets, feature flags, page/control generators, solution files, or packaging options change, test the actual generated files. A successful build of the template source repository does not prove generation works.

For every preset/combination changed or claimed as supported, verify appropriate parts of this chain:

1. Scaffold from a clean temporary directory outside the template source tree.
2. Check generated names, namespaces, paths, configuration, and feature manifest.
3. Search for unresolved placeholders, absolute developer-machine paths, missing assets, duplicate/unused references, broken project links, and disabled-feature residue.
4. Restore and build the generated application with documented settings.
5. Run smoke tests and launch checks on Windows when required.
6. Verify optional dependency removal when the preset promises it.
7. Preserve logs/evidence and clean temporary artifacts without touching user files.

Test all supported presets and feature combinations that can be affected by the change. Avoid testing only the happy-path configuration.

### 8.4 Performance policy

- Start or maintain reproducible baselines for startup to window/interactive readiness, memory after startup, navigation latency, scrolling/virtualization, idle CPU, and animation/effect cost as applicable.
- Identify modern and low-spec Windows 11 reference environments. The roadmap's low-spec target is representative of a lower-power CPU, 8 GB RAM, integrated graphics, and SSD; use actual hardware details when available.
- Compare measurements under stated conditions and report medians plus meaningful worst-case observations. Separate measured data from estimates.
- Prefer no animation or lightweight transforms for common interaction. Avoid unnecessary perpetual animations, repeated layout invalidation, huge blurred/translucent surfaces, and expensive render work on hidden content.
- Keep a sensible `Auto` mode and user-accessible `Balanced`/`Rich` behaviour as the motion/effects system develops. Effects must not delay an essential action.
- Before performance budgets are formally ratified, capture an initial baseline and provisional targets in `0.1.0`; ratify measurable budgets by `0.4.0` per roadmap. Once ratified, do not relax them silently.
- A water-warp or similar experiment must be compared with its static fallback, can be independently disabled, and must not be required for legibility or navigation.

### 8.5 Accessibility and visual verification

Where relevant, verify:

- Keyboard-only operation, logical tab order, visible focus, automation names, and accessible state.
- Readable contrast and usable high-contrast behaviour.
- Reduced-motion behaviour and settings persistence where applicable.
- Light/dark/system theme changes and correct fallback when a material or effect is unavailable.
- DPI scaling, resizing, window restore/maximize, narrow/collapsed navigation, text scaling where supported, and prevention of clipped content.
- Visual regression captures for important shells/components as a review aid, never as the sole test.

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

Maintain the existing roadmap as the long-term product plan. Update its status or milestone checkboxes only when supported by real evidence; do not rewrite future scope just because an implementation preference changed.

Recommended documentation structure (adapt to established repository conventions; do not duplicate equivalent files):

```text
AGENT-EXECUTION-PROTOCOL.md        # this permanent process contract
WPF-Template-Roadmap.md            # long-term product roadmap
README.md                          # human-facing quick start and product overview
CHANGELOG.md                       # user-visible changes per version
Directory.Packages.props           # central package versions, if adopted

docs/
  releases/
    0.1.0-plan.md                  # written before implementation
    0.1.0-report.md                # completion/release evidence
    0.2.0-plan.md
    0.2.0-report.md
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

- Follow established repository branch conventions when present. Otherwise use a dedicated version branch such as `release/0.1.0` or `work/0.1.0`.
- Do not start release implementation directly on the default branch when branch-based work is supported by the repository.
- Make small, meaningful commits that represent coherent, verified slices. Include relevant tests and documentation in the associated changes.
- Use a consistent, descriptive style (Conventional Commits if already adopted; otherwise `type(scope): summary` is acceptable). Do not manufacture commit messages that claim unverified outcomes.
- Before staging, inspect `git status` and the diff. Stage explicit project files rather than blindly adding every untracked file. Exclude secrets, local environment files, temporary benchmarks, generated build output, and unrelated user work.
- Before every commit, review `git diff --check`, staged diffs, and relevant tests. Do not commit knowingly broken or untested work except where the commit is explicitly a documentation-only planning commit and clearly labelled.

### 11.2 Push and integration

After the version implementation and local gates pass:

1. Re-run the release-candidate checks from a clean or controlled checkout state.
2. Commit all intended changes and verify commit contents/history.
3. Push the version branch to the configured remote if credentials and permissions allow.
4. Use the repository's normal PR/merge mechanism. If a PR is required or standard, create/update it and verify required checks. The agent may merge automatically only after all required checks pass and repository policy permits it.
5. Never bypass a protected branch, disable a required check, force-push, or rewrite shared history to make integration easier.
6. If direct integration is the established documented workflow and is permitted, follow it only after the same checks pass.

If authentication, remote access, branch protection, mandatory review, or CI prevents an authorized step, stop at that boundary, preserve commits and results, and report exactly what remains. Do not claim a push or merge happened unless the remote confirms it.

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

1. Create the version tag (typically `v0.1.0`) at the verified release commit.
2. Push the tag without altering existing tags.
3. Publish the GitHub Release with accurate notes and only the appropriate verified artifacts/checksums.
4. Mark `0.x` releases as prereleases where appropriate; treat `1.0.0-rc.1` as a release candidate/prerelease; publish `1.0.0` as stable only after the complete stable release gate passes.
5. Verify the resulting remote tag, release page, and assets after publication.

Only publish NuGet packages, installers, MSIX/Store artifacts, or other distribution channels when that channel is part of the roadmap/release plan, credentials are configured, and its specific verification passes. Do not create fake success placeholders for a channel that is not configured.

### 11.4 Release blockers

Do **not** tag/publish if any of the following apply:

- A required restore/build/test/CI gate fails or is unverified.
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

For example: `docs/releases/0.1.0-report.md`.

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

## 14. Required version-plan template

Use this outline when creating `docs/releases/<version>-plan.md`. Expand sections enough to make the plan actionable; do not fill them with generic text.

```markdown
# <VERSION> — <Milestone Name> Plan

- Status: Draft
- Prepared: YYYY-MM-DD
- Roadmap section: <heading/link>
- Starting commit/branch: <verified value>
- Previous version: <version/commit>

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

## 9. Test and CI plan
## 10. Performance plan and budgets
## 11. Accessibility, resilience, and fallback plan
## 12. Dependency, license, and security impact
## 13. Documentation and examples
## 14. Risks and mitigations
## 15. Git, integration, and release plan
## 16. Deferred work
## 17. Plan-readiness checklist
```

## 15. Required completion-report template

Use this outline for `docs/releases/<version>-report.md`.

```markdown
# <VERSION> — <Milestone Name> Completion Report

- Status: Complete / Blocked
- Completed: YYYY-MM-DD
- Plan: <link>
- Final commit: <hash>
- Release tag/URL: <verified value, or Not published + reason>

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
## 10. Git, merge, and release evidence
## 11. Reproduction and recovery instructions
```

## 16. Release gate checklist

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

## 17. Standard final status message

At the end of each run, present a concise report with concrete evidence:

1. **Version and scope:** target version; what was delivered; what was deliberately deferred.
2. **Plan/report:** repository paths to the version plan and completion report.
3. **Verification:** exact important build/test/CI results and any skipped or unavailable gates.
4. **Performance/accessibility:** measured results or explicit statement of what was not measured.
5. **Git:** branch, commit hashes, push/PR/merge status.
6. **Release:** published tag/URL and artifacts, or the exact reason publication did not occur.
7. **Blockers/next step:** only actionable unresolved issues; do not begin the next version automatically in the same scope.

Never announce completion solely because code was written. Completion means evidence-backed acceptance. If a release gate fails, say so directly, preserve the work, and report the recovery path.

---

## 18. Initial run instructions

When first adopting this protocol in the repository:

1. Read the existing `WPF-Template-Roadmap.md` and verify its location/name.
2. Add this file at the repository root as `AGENT-EXECUTION-PROTOCOL.md`.
3. Ensure the repository's agent entry point (`AGENTS.md`, or equivalent instructions file used by the chosen coding agent) explicitly says: **“Before any implementation work, read `AGENT-EXECUTION-PROTOCOL.md` and the WPF template roadmap. Follow the per-version planning, research, testing, and release gates.”** If no such entry point exists, create the smallest suitable one; do not overwrite unrelated instructions.
4. Start with `0.1.0` only. Inspect the real repository and environment, establish the baseline, perform current research, and write `docs/releases/0.1.0-plan.md` before implementation.
5. Do not assume the initial repository is empty or matches this roadmap. Reconcile the plan with actual files and report material discrepancies without silently broadening scope.

**Operating principle:** Research until the important decisions are evidence-based; plan the version before coding; implement only that version; test continuously; verify every release claim; then commit, push, integrate, and publish only when all applicable gates pass.
