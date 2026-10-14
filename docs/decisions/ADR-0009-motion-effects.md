# ADR-0009: Motion system and effects policy

- Status: Accepted
- Date: 2026-10-14
- Milestone: 0.5.0

## Context

Roadmap 0.5.0 needs a coherent motion system with performance fallbacks and
optional premium effects — never animation for its own sake, never blocking.

## Decision

- Tokens in code (`MotionTokens`); policy pure (`MotionPolicy`); OS signal
  `SPI_GETCLIENTAREAANIMATION` 0x1042 (menu flag 0x1002 is NOT the signal —
  corrected from memory against Microsoft Learn); render tier high word.
- `MotionService` storyboards (fade + rise entrances, ripple, gradient
  helper owned by the page); Off snaps; all failures snap; 380ms max.
- Page entrances via `Loaded` (no nav-event coupling); ripple + gradient on
  a dedicated Effects page behind `EffectsEnabled`; warp shader deferred.
- Zero new packages.

## Evidence

- 45/45 tests (policy matrix, instant path, ripple lifecycle, round-trip).
- Launches: default, Motion=Off + effects off; 0.77s / 131.8 MB (budgets hold).
- Storyboard `Completed` proven unreliable off-visual-tree → dual-path
  removal (idempotent), documented in code.

## Consequences

- Command-supplied CancellationTokens arrive pre-cancelled under MTP (0.4.0
  finding, encore) — motion APIs take optional explicit tokens only.
- Stagger/layout animation stay out until measured need.

## Revisit triggers

Low-spec validation (warp unblock), dashboard motion needs (0.6.0).
