# Deferred work backlog — 1.0 scope freeze (0.9.0); 1.1.0 additions appended (D21–D24); 1.4.0 dispositions appended (D09r/D14/D15 closed, D25–D27 opened)

Rule: 1.0-rc takes bug fixes + release mechanics only. Everything below is
post-1.0 unless marked **1.0**.

| ID | Item | Verdict |
|---|---|---|
| D01 | `Microsoft.Xaml.Behaviors.Wpf` adoption | Post-1.0 — no concrete consumer through 0.9.0. |
| D07r | Auth + crash-reporting samples | Post-1.0 — needs security review + config-burden approval. |
| D07t | System toast notifications | Post-1.0 — MSIX-gated. |
| D07s | Release signing execution (real cert) | Post-1.0 — app-level concern; guidance published. |
| D08r | VSIX / VS Add-Item integration | Post-1.0 — CLI templates surface in VS already. |
| D09r | Visual regression snapshots | **1.2.0 (partial)** — dependency-free spike shipped (`eng/compare-screenshots.ps1` + baselines, advisory-only, 0.03% noise floor proven). Required-gate decision → 1.3.0 with this evidence. |
| D10 | Low-spec reference machine | Post-1.0 — no hardware; modern-HW budgets ratified. |
| D12 | `Microsoft.ApplicationInsights` test-only transitive | Watch each audit — still test-only, still harmless. |
| D13 | HLSL water-warp shader | Post-1.0 — needs toolchain + low-spec proof (D10). |
| D14 | Drag-and-drop workspace patterns | Post-1.0 — no consumer. |
| D15 | Top-nav + tabbed shells | Post-1.0 — needs real consumer + template surface. |
| D16 | Full-gallery localization | **1.3.0** — all gallery display copy in resx (en+de, parity-tested); automation names invariant; sample data + log text stay English by rule. |
| D17 | Chart theming pass | **1.3.0** — transparent chart, theme/accent-following paints, SR summary; light + dark verified. Legends/tooltips remain library-default. |
| D18 | TreeView / full DataGrid / TimePicker sample | **1.3.0** — native DataGrid + TreeView samples shipped with guides; WPF-UI TimePicker rejected after spike (broken popup reports + missing automation name on 4.3.0), gallery-scoped recipe instead. |
| D19 | Per-service template flags | Post-1.0 candidate — bundles proven. |
| D20 | Template NuGet package publishing | **1.0** — release mechanics (documented path exists). |
| D21 | UI-test skip visibility (ShellFlowsTests `return`-on-skip reported passes) | **1.1.0** — fixed by `RequireUiOrSkip` + `Assert.Skip` and `eng/check-ui-tests.ps1` required gate (T04). |
| D22 | Template-test isolation (global `dotnet new` hive, shared TEMP logs) | **1.1.0** — in progress (T06: per-run `DOTNET_CLI_HOME`, per-run logs, `finally` restore). |
| D23 | Fixed exe path in `eng/measure-startup.ps1` | **1.1.0** — in progress (T08: dynamic `*App.exe` resolution like `smoke.ps1`). |
| D24 | Stale README (screenshot placeholder, duplicate 1.0.0 row, legacy roadmap links) + stale in-app Home subline | **1.1.0** — fixed (T02/T03: real screenshots in `docs/images/`, version-agnostic copy). |
| D09r | Visual regression required gate | **1.4.0 decision: stays ADVISORY.** Evidence across 1.2 (0.03% same-UI noise), 1.3 (deliberate re-baseline, Dashboard excluded for volatility), 1.4 (deliberate re-baseline for palette button + shell note; Controls scroll noise 9%; DWM border strip masked with documented 12px mask). The CopyFromScreen harness is occlusion/scroll/animation-sensitive. Required-gate needs a deterministic capture harness (PrintWindow-based + scroll pinning); revisit then. |
| D14 | Drag-and-drop workspace patterns | **1.4.0 — feature shipped, gesture deferred.** Reorder/split ship via keyboard-operable Move buttons + GridSplitter + persisted width (unit + UI proven). The pointer-drag gesture (private format, Esc cancel, single-gesture guard) was prototyped and **deferred**: OLE modal gestures failed across automation runners including the quiet CI runner (traced sequential cancels; no product-side drop defect found, but the gate requires proven reliability). Revisit with a runner-proof gesture design. |
| D15 | Top-nav + tabbed shells | **1.4.0 — SHIPPED.** Four presets (Sidebar/Rail/Top/Tabbed) via shared route registry; `--Shell` scaffold default; runtime changes restart-apply (ADR-0015). Multi-window explicitly deferred (no testable consumer) — see D26. |
| D25 | Live shell hot-switch (runtime `PaneDisplayMode` swap) | Post-1.4 — WPF-UI 4.3.0 crashes intermittently (NaN `DoubleAnimation` during re-template; upstream PR #1012/#1015 fixed only the load path). Four mitigations exhausted with UI-run evidence (settle delay, transitionless close + animation defuse, control recreation with unbind-first). Revisit only with a passing hot-switch UI test on a fixed toolkit version; re-check every WPF-UI upgrade. |
| D26 | Window tear-out / cross-window or cross-process drag / multi-window | Post-1.4 — no testable consumer; drag stays in-workspace by construction (private format). |
| D27 | 8 GB low-spec reference validation | Still pending (unchanged, D10 standing). 1.4.0 numbers are proxy-hardware only; Tabbed + 3 documents measured 150.9 MB working set (accepted variance, per-document cost documented). |

Done: D02 (0.2.0), D03 (0.3.0), D04 (0.4.0), D05 (0.5.0), D06 dashboard+charts (0.6.0),
D07 core services/CI (0.7.0), D08 templates/generators (0.8.0), D09 CI+FlaUI (0.7.0/0.9.0),
D11 remote/publishing (0.1.0–0.8.0).
