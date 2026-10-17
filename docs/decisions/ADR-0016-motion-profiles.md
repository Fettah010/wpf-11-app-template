# ADR-0016: Motion profiles, primitives, and effects policy

- Status: Accepted
- Date: 2026-10-17
- Milestone: 1.5.0

## Context

1.5.0 (roadmap §8.5) expands the 0.5.0 motion system: property transitions,
enter/exit, selection, expansion/collapse, feedback, reduced-motion-aware page
transitions, and a bounded effects prototype. The earlier model had a single
"everything" tier (`Full`) and a `Full`-or-nothing ripple; there was no middle
tier, no per-effect control, and no stated shader policy. `MotionMode` is
persisted as an integer, so the profile model must stay backward compatible.

## Decision

- **Append, don't renumber.** `MotionMode` gains `Balanced = 4` (existing
  `Auto=0, Full=1, Reduced=2, Off=3` are untouched; old `Motion=1` files keep
  resolving to Rich). `EffectiveMotion` is derived (never persisted) and is
  ordered by richness: `Off < Reduced < Balanced < Full`.
- **Auto resolves to Balanced.** `MotionPolicy.Resolve` maps Auto → Reduced
  when OS animations are off or the render tier is ≤ 0, else Balanced. `Full`
  (labelled "Rich") stays an explicit opt-in, so decorative/looping effects
  never run by default. This is a deliberate, roadmap-authorized refinement of
  the 0.5.0 Auto→Full default.
- **Gating via pure helpers.** `AllowsMovement` (Balanced+) and
  `AllowsDecorative` (Rich only) live in `MotionPolicy`; call sites never
  compare enum values by hand.
- **Per-effect switches (additive settings).** `RippleEffectEnabled` (on),
  `GradientEffectEnabled` (on), `WarpEffectEnabled` (off) refine the master
  `EffectsEnabled`; `IMotionService.AllowsEffect` is the single gate.
- **Warp is a prototype, not a shader.** No HLSL: `ShaderEffect` requires a
  compiled `.ps` from `fxc.exe` (≤ `ps_3_0`) and any `Effect` forces an
  intermediate render target that disables ClearType for its subtree, and no
  low-spec reference machine (D10/D27) is available to satisfy the low-spec
  gate. The prototype is a backdrop-only simulated distortion with a static
  fallback, off by default.
- **No new packages.**

## Evidence

- Policy matrix + `AllowsMovement`/`AllowsDecorative` + effect-switch tests;
  settings round-trip and legacy `Motion` integer mapping tests.
- Effects page gates each demo through `AllowsEffect` (ripple, gradient).
- 1.5.0 plan go/no-go recorded the shader no-go with the toolchain/ClearType
  and reference-machine evidence above.

## Consequences

- `MotionMode.Balanced` is persisted as `4`; older 1.5.0 pre-release builds
  would not understand it, which is acceptable (unreleased).
- The "Rich" label is presentation only; the persisted value stays `Full`.
- Storyboards remain interruptible by policy; Off always snaps the final
  state.

## Revisit triggers

Low-spec reference hardware + pinned HLSL toolchain (shader unblock); a third
middle tier proves necessary.
