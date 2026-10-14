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

## 0.3.0 baseline (2026-10-14, `release/0.3.0`)

| Metric | Median | Max | How |
|---|---|---|---|
| Cold start (launch → window) | 0.73s | 0.80s | `measure-startup.ps1 -Runs 5` (0.73/0.80/0.77/0.69/0.67) |
| Working set after start | 131.4 MB | 149 MB | same runs, +2s idle sample |

Delta vs 0.2.0: +0.02s start, +1 MB working set — token dictionaries are
noise. Max working set (149 MB, one run) touches the provisional 150 MB
hypothesis; treated as GC-timing noise on a warm-cache machine, watched in
0.4.0. Low-spec reference still outstanding (D10).

## Ratified budgets (0.4.0, modern reference hardware)

Data: start medians 0.61 / 0.61 / 0.71 / 0.73s; working-set medians
116 / 116 / 130 / 131 MB (max 149). Growth is explained (shell + tokens);
headroom is comfortable. Low-spec numbers remain outstanding (D10) — these
budgets are modern-HW-only until measured there.

| Budget | Gate |
|---|---|
| Cold start median ≤ 1.0s (Release, warm-cache) | Blocking |
| Working set median ≤ 150 MB after start | Blocking |
| Builds with 0 warnings; tests 100% passing | Blocking |

## 0.4.0 baseline (2026-10-14, `release/0.4.0`)

| Metric | Median | Max | How |
|---|---|---|---|
| Cold start (launch → window) | 0.68s | 0.72s | `measure-startup.ps1 -Runs 5` (0.72/0.68/0.68/0.68/0.67) |
| Working set after start | 131.7 MB | 132.2 MB | same runs, +2s idle sample |

Inside ratified budgets. Gallery pages resolve on navigation — startup is
unaffected by gallery size (design validated by measurement).

## 0.5.0 baseline (2026-10-14, `release/0.5.0`)

| Metric | Median | Max | How |
|---|---|---|---|
| Cold start (launch → window) | 0.77s | 0.81s | `measure-startup.ps1 -Runs 5` (0.81/0.76/0.77/0.71/0.77) |
| Working set after start | 131.8 MB | 132.2 MB | same runs, +2s idle sample |

Inside ratified budgets (≤1.0s, ≤150 MB). Entrance storyboards play
post-load and do not move startup; no perpetual animation by default.

## 0.6.0 baseline (2026-10-14, `release/0.6.0`)

| Metric | Median | Max | How |
|---|---|---|---|
| Cold start (launch → window) | 0.77s | 0.81s | `measure-startup.ps1 -Runs 5` (0.81/0.71/0.79/0.77/0.70) |
| Working set after start | 132.2 MB | 132.3 MB | same runs, +2s idle sample |

Inside ratified budgets. Dashboard (incl. Skia natives + 1500-row source)
resolves on first navigation — startup unaffected (design re-validated).

## 0.7.0 baseline (2026-10-14, `release/0.7.0`)

| Metric | Median | Max | How |
|---|---|---|---|
| Cold start (launch → window) | 0.80s | 0.84s | `measure-startup.ps1 -Runs 5` (0.84/0.80/0.78/0.80/0.79) |
| Working set after start | 135.7 MB | 137 MB | same runs, +2s idle sample |

Inside ratified budgets (≤1.0s, ≤150 MB). Serilog init + SQLite open add
~0.03s / ~3 MB — lazy-init backstop documented if budgets ever tighten.

## 0.9.0 baseline (2026-10-14, `release/0.9.0`)

| Metric | Median | Max | How |
|---|---|---|---|
| Cold start (launch → window) | 0.80s | 0.82s | `measure-startup.ps1 -Runs 5` (0.82/0.79/0.78/0.81/0.80) |
| Working set after start | 142.5 MB | 146.3 MB | same runs, +2s idle sample |
| Idle 20s CPU | 0% | 0% | `measure-idle.ps1 -IdleSeconds 20` (observational) |
| Working set after idle | 142.1 MB | — | same run (GC settles higher; under budget) |

Inside ratified budgets (≤1.0s, ≤150 MB) with shrinking headroom on memory:
0.7.0→0.9.0 set grew 135.7→142.5 MB (Serilog + SQLite + Velopack natives).
1.0-rc must not add resident weight without a matching cut.
