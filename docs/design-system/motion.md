# Motion system

Single source: `MotionTokens` (durations, easings, distances) + `MotionPolicy`
+ `IMotionService`/`MotionService`. No XAML storyboards in 0.5.0 — everything
is built in code so policy gating is inescapable.

## Presets (tokens)

| Preset | Duration | Character |
|---|---|---|
| Instant | 0ms | state jumps, no storyboard |
| Subtle / Fast | 120ms | opacity-only micro-feedback |
| Standard | 220ms | page entrance: fade + 12px rise, EaseOutCubic |
| Expressive / Deliberate | 380ms | ripple expand + fade; maximum — motion never delays actions |

Easings: EaseOutCubic (standard movement), EaseInOutCubic (overlays).
Slide distance 12px; ripple 8→220px.

## Profiles

| Setting | Behavior |
|---|---|
| Auto (default) | OS animation setting off → Reduced; render tier 0 → Reduced; else Full |
| Full | everything (user-forced, even on weak GPUs) |
| Reduced | fast fades only — no slide, ripple, or looping surfaces |
| Off | instant state changes, no storyboards |

OS signal: `SPI_GETCLIENTAREAANIMATION` (0x1042) via `OsMotionInfo`
(fail-open to enabled; user override always available). Render tier:
`RenderCapability.Tier` high word. Both injectable for tests.

## Where motion plays

- Page entrances: every page calls `PlayEntranceAsync` in `Loaded`
  (view-boundary orchestration). Off snaps opacity 1; failures snap too.
- Ripple: click-to-ripple on the Effects surface. Full policy + effects
  switch required. Decorative only; cleanup is idempotent and dual-pathed
  (storyboard completion + timed dispatcher fallback).
- Animated gradient: opt-in toggle on the Effects page; stops on toggle-off
  and page unload. The only looping animation in the app, never default.
- Never animated: WPF-UI internals, layout, essential state changes.

## Fallbacks

Reduced motion (OS or setting) skips slide/ripple/gradient; Off skips all.
`EffectsEnabled=false` kills ripple + gradient. Storyboard exceptions snap
final state — motion never crashes a view. High contrast is orthogonal.

## Warp shader: deliberately not shipped

A true water-warp/distortion shader needs a pinned HLSL toolchain and
low-spec GPU validation — neither exists in this environment (no low-spec
reference machine, D10). The storyboard ripple is the bounded showcase.
Unblock conditions: low-spec validation passing + fallback proof + toolchain
pin. See `docs/DEFERRED.md`.
