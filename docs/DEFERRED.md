# Deferred work backlog — 1.0 scope freeze (0.9.0)

Rule: 1.0-rc takes bug fixes + release mechanics only. Everything below is
post-1.0 unless marked **1.0**.

| ID | Item | Verdict |
|---|---|---|
| D01 | `Microsoft.Xaml.Behaviors.Wpf` adoption | Post-1.0 — no concrete consumer through 0.9.0. |
| D07r | Auth + crash-reporting samples | Post-1.0 — needs security review + config-burden approval. |
| D07t | System toast notifications | Post-1.0 — MSIX-gated. |
| D07s | Release signing execution (real cert) | Post-1.0 — app-level concern; guidance published. |
| D08r | VSIX / VS Add-Item integration | Post-1.0 — CLI templates surface in VS already. |
| D09r | Visual regression snapshots | Post-1.0 — no snapshot infra; review is manual. |
| D10 | Low-spec reference machine | Post-1.0 — no hardware; modern-HW budgets ratified. |
| D12 | `Microsoft.ApplicationInsights` test-only transitive | Watch each audit — still test-only, still harmless. |
| D13 | HLSL water-warp shader | Post-1.0 — needs toolchain + low-spec proof (D10). |
| D14 | Drag-and-drop workspace patterns | Post-1.0 — no consumer. |
| D15 | Top-nav + tabbed shells | Post-1.0 — needs real consumer + template surface. |
| D16 | Full-gallery localization | Post-1.0 — pattern + proof shipped; mechanical remainder. |
| D17 | Chart theming pass | Post-1.0 — defaults documented. |
| D18 | TreeView / full DataGrid / TimePicker sample | Post-1.0 — gaps catalogued. |
| D19 | Per-service template flags | Post-1.0 candidate — bundles proven. |
| D20 | Template NuGet package publishing | **1.0** — release mechanics (documented path exists). |

Done: D02 (0.2.0), D03 (0.3.0), D04 (0.4.0), D05 (0.5.0), D06 dashboard+charts (0.6.0),
D07 core services/CI (0.7.0), D08 templates/generators (0.8.0), D09 CI+FlaUI (0.7.0/0.9.0),
D11 remote/publishing (0.1.0–0.8.0).
