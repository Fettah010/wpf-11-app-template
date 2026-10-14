# Deferred work backlog

Out-of-scope discoveries go here instead of leaking into the current
milestone (protocol §3). Each entry: problem, evidence, value, likely
milestone, risk.

| ID | Item | Evidence / reason | Value | Milestone | Risk |
|---|---|---|---|---|---|
| D01 | `Microsoft.Xaml.Behaviors.Wpf 1.1.161` adoption | Latest stable 2026-09-15; 0.1.0 app needs no behaviors | Interaction helpers without code-behind | 0.4.0/0.5.0 | Low |
| D02 | ~~Shells, title-bar chrome, backdrop adapters~~ — done in 0.2.0 | — | — | 0.2.0 ✅ | — |
| D03 | ~~Tokens/themes/density~~ — done in 0.3.0 | — | — | 0.3.0 ✅ | — |
| D04 | ~~Gallery/forms/dialogs~~ — done in 0.4.0 | — | — | 0.4.0 ✅ | — |
| D05 | ~~Motion system~~ — done in 0.5.0 (stagger/layout animation stay out) | — | — | 0.5.0 ✅ | — |
| D13 | HLSL water-warp/distortion shader | Needs pinned shader toolchain + low-spec GPU validation + fallback proof (none available) | Signature backdrop effect | 0.5.0+ (unblocked by D10) | GPU/perf — measure before ship |
| D14 | Drag-and-drop workspace patterns | No 0.6.0 consumer; lower priority than table/charts | Rich workspace UX | 0.6.0+ / 0.9.0 scope decision | API/design time |
| D15 | Top-navigation + tabbed-workspace shells | 0.6.0 ships patterns (master/detail, sections); full shells are template surface | Shell choice for generated apps | 0.8.0 (contracts then) | Scope — needs real consumer |
| D06 | ~~Dashboard/gallery content, charts~~ — dashboard + charts done in 0.6.0; drag-drop remainder → D14 | — | — | 0.6.0 ✅ (partial) | — |
| D07 | ~~Serilog, SQLite, HTTP, tray/notifications, backup, auth sample, Velopack, MSIX/Store, release automation~~ — done in 0.7.0 except: auth/reporting samples (security review), system toasts (MSIX-gated), signing execution | — | — | 0.7.0 ✅ (partial) | — |
| D09 | ~~GitHub Actions CI, FlaUI automation, visual regression, signed artifacts~~ — CI done in 0.7.0; automation/regression/signing remain | — | — | 0.7.0 ✅ (partial) | Runner availability |
| D11 | ~~Remote repository creation + push~~ — done (origin set, releases published) | — | — | 0.1.0–0.6.0 ✅ | — |
| D08 | `dotnet new` templates, generators, VSIX, rebrand tooling | Roadmap scope | 0.8.0 DX | 0.8.0 | — |
| D09 | ~~GitHub Actions CI~~ — CI done in 0.7.0; FlaUI automation, visual regression, signed artifacts remain | — | — | 0.7.0 ✅ (partial) | Runner availability |
| D10 | Low-spec reference machine + ratified perf budgets | Single-machine baseline only | Budget trust | ≤0.4.0 | Hardware access |
| D11 | Remote repository creation + push | No remote configured; needs user remote/credentials | Collaboration/CI | When provided | Secrets handling |
| D12 | `Microsoft.ApplicationInsights` test-only transitive review | Via MTP telemetry ext. 2.5.1; harmless today | Supply-chain hygiene | 0.7.0 audit | Leakage into app output |
