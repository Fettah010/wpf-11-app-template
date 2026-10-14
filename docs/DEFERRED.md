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
| D05 | Motion system + water-warp spike (optional, fallback-first) | Roadmap scope; WPF ≠ WinUI compositor | 0.5.0 feel | 0.5.0 | GPU/perf — measure |
| D06 | Dashboard/gallery content, charts (LiveCharts2 audit), drag-drop | Roadmap scope | 0.6.0 depth | 0.6.0 | — |
| D07 | Serilog, SQLite, HTTP, tray/notifications, backup, auth sample, Velopack, MSIX/Store, release automation | Each needs fresh license/compat audit | 0.7.0 production | 0.7.0 | License/compat per package |
| D08 | `dotnet new` templates, generators, VSIX, rebrand tooling | Roadmap scope | 0.8.0 DX | 0.8.0 | — |
| D09 | GitHub Actions CI, FlaUI automation, visual regression, signed artifacts | No remote/CI in 0.1.0 | 0.7.0–0.9.0 quality | 0.7.0–0.9.0 | Runner availability |
| D10 | Low-spec reference machine + ratified perf budgets | Single-machine baseline only | Budget trust | ≤0.4.0 | Hardware access |
| D11 | Remote repository creation + push | No remote configured; needs user remote/credentials | Collaboration/CI | When provided | Secrets handling |
| D12 | `Microsoft.ApplicationInsights` test-only transitive review | Via MTP telemetry ext. 2.5.1; harmless today | Supply-chain hygiene | 0.7.0 audit | Leakage into app output |
