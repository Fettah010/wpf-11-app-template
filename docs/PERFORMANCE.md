# Performance methodology and baselines

Method: `eng/measure-startup.ps1` (Release build, N=5 launches,
launch → main-window handle; working set sampled 2s after window appears;
median + max reported). Budgets are **provisional** until ratified ≤0.4.0
per roadmap. Navigation/scroll/animation metrics: N/A (no such surface in
0.1.0) — will be added with the milestones that ship them.

## Reference hardware (0.1.0)

- CPU: AMD Ryzen 7 8700G w/ Radeon 780M Graphics
- RAM: 31.1 GB; GPU: AMD Radeon 780M (iGPU); Disk: WD_BLACK SN850X 1 TB (NVMe)
- OS: Windows 11 Pro build 26300 (x64); SDK 10.0.401
- Note: **modern machine, warm-cache runs** (repeated launches, no reboot).
  A low-spec reference environment (lower-power CPU, 8 GB, iGPU, SSD) is
  required from 0.2.0 onward (see `docs/DEFERRED.md` D10).

## 0.1.0 baseline (2026-10-14, `release/0.1.0`)

| Metric | Median | Max | How |
|---|---|---|---|
| Cold start (launch → window) | 0.61s | 0.69s | `measure-startup.ps1 -Runs 5` (0.69/0.61/0.66/0.61/0.57) |
| Working set after start | 116.3 MB | 116.6 MB | same runs, +2s idle sample |

Provisional hypotheses (to confirm/ratify by 0.4.0, not gates yet):
cold start < 2s on modern HW; working set < 150 MB at idle.

## 0.2.0 baseline (2026-10-14, `release/0.2.0`)

| Metric | Median | Max | How |
|---|---|---|---|
| Cold start (launch → window) | 0.71s | 0.77s | `measure-startup.ps1 -Runs 5` (0.73/0.69/0.77/0.67/0.71) |
| Working set after start | 130.4 MB | 130.5 MB | same runs, +2s idle sample |

Delta vs 0.1.0: +0.10s start, +14 MB working set — the FluentWindow +
NavigationView + DI pages cost. Inside provisional bounds; low-spec
reference environment still outstanding (D10).
