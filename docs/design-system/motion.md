# Motion system

Single source: `MotionTokens` (durations, easings, distances, opacity) +
`MotionPolicy` (pure mapping) + `IMotionService`/`MotionService` (storyboards).
No XAML storyboards — everything is built in code so policy gating is
inescapable and interruptible.

## Tokens

| Token | Value | Character |
|---|---|---|
| Instant | 0ms | state jumps, no storyboard |
| Fast | 120ms | opacity-only micro-feedback (reduced-motion duration) |
| Standard | 220ms | standard transitions and page entrances |
| Deliberate | 380ms | ripple expand, expressive feedback |
| Stagger | 40ms | per-item cascade step for staggered content |
| MaxDuration | 380ms | hard ceiling — motion never delays actions |

Geometry/opacity: slide 12px, exit 6px, ripple 8→220px, enter from 0.0,
rest 1.0, exit to 0.0, feedback peak 0.85 (scale 1.03).
Easings: `EaseOutCubic` (movement), `EaseInOutCubic` (overlays).

## Profiles

Chosen in Settings; `MotionPolicy.Resolve` maps it to an `EffectiveMotion`
level, ordered by richness (Off < Reduced < Balanced < Full).

| Setting | Effective | Behavior |
|---|---|---|
| Auto (default) | Balanced / Reduced | OS animation setting off → Reduced; render tier 0 → Reduced; else Balanced |
| Rich (`Full`) | Full | everything, including decorative and looping effects |
| Balanced | Balanced | standard transitions + movement (slide/rise/scale); no decorative/looping effects |
| Reduced | Reduced | fast fades only — no movement, ripple, or looping surfaces |
| Off | Off | instant state changes, no storyboards |

`Full`/Rich is an explicit opt-in: the ambient Auto default resolves to
Balanced, so decorative effects never run unless requested. Existing
`Motion=1` files still resolve to Rich (enum values are append-only —
ADR-0016).

Policy helpers: `MotionPolicy.AllowsMovement` (Balanced and up) and
`MotionPolicy.AllowsDecorative` (Rich only) keep gating out of call sites.

OS signal: `SPI_GETCLIENTAREAANIMATION` (0x1042) via `OsMotionInfo`
(fail-open to enabled; user override always available). Render tier:
`RenderCapability.Tier` high word. Both injectable for tests.

## Effects and their switches

A decorative effect runs only when the master `EffectsEnabled` switch is on,
the effective level is Rich, and the effect's own switch is on. All three are
queried through `IMotionService.AllowsEffect`:

| Effect | Switch (persisted) | Default |
|---|---|---|
| Ripple | `RippleEffectEnabled` | on |
| Animated gradient | `GradientEffectEnabled` | on |
| Backdrop warp prototype | `WarpEffectEnabled` | off |

## Primitives

`IMotionService` exposes reusable primitives; every one is cancellable and
snaps to its final state on cancellation or failure, so callers may await
without blocking a view.

| Primitive | Movement-gated part | Notes |
|---|---|---|
| `PlayEntranceAsync` | 12px rise | fade always; accepts a duration override (presets) |
| `PlayExitAsync` | 6px fall | fade to opacity 0 |
| `PlaySelectionAsync` | — | indicator opacity for selected/deselected |
| `PlayExpandAsync` / `PlayCollapseAsync` | ±6px | reveal/hide detail content |
| `PlayFeedbackAsync` | 1.03 scale pulse | opacity flash always; transient acknowledgement |
| `PlayPageTransitionAsync` | ±12px slide | directional (`forward`), reduced-motion-aware |

Under Reduced the movement parts are dropped (fast fade only); under Off every
primitive snaps. Primitives own the element's `RenderTransform` (appending to
a `TransformGroup` when one already exists).

## Lifecycle

`MotionScope` (registered as a singleton) owns in-flight motion. Views register
the storyboards they start and pass `scope.Token` to awaited primitives; the
shell calls `StopAll()` on navigation-away, unload, theme change, window
deactivation, and shutdown, which cancels the token (primitives snap) and runs
registrations in reverse. `StopAll()` re-arms the scope for the next cycle;
`Dispose()` retires it. Motion never subscribes to
`CompositionTarget.Rendering` — a repo-only test enforces this.

## Where motion plays

- Page entrances: every page calls `PlayEntranceAsync` in `Loaded`
  (view-boundary orchestration). Off snaps opacity 1; failures snap too.
- Ripple: click-to-ripple on the Effects surface. Cleanup is idempotent and
  dual-pathed (storyboard completion + timed dispatcher fallback).
- Animated gradient: opt-in toggle on the Effects page; stops on toggle-off
  and page unload. The only looping animation in the app, never default.
- Never animated: WPF-UI internals, layout, essential state changes.

## Fallbacks

Reduced motion (OS or setting) skips movement/ripple/gradient; Off skips all.
`EffectsEnabled=false` (or a per-effect switch off) kills the matching
effect. Storyboard exceptions snap final state — motion never crashes a view.
High contrast is orthogonal.

## Backdrop warp: prototype, not a shader

A true water-warp/distortion shader is **not shipped**. It needs a pinned
HLSL toolchain (`fxc.exe`, ≤ `ps_3_0`) and low-spec GPU validation, and any
`Effect` forces an intermediate render target that disables ClearType for its
whole subtree. Neither the toolchain pin nor a low-spec reference machine
exists here (D10/D27). The bounded prototype is a backdrop-only *simulated*
distortion with a static fallback; it must never sit over text or interactive
content. Unblock conditions and evidence: `docs/DEFERRED.md` (D01/D10) and
ADR-0016.
