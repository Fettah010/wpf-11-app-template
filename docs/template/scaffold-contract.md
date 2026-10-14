# Scaffold contract (0.1.0 baseline)

Defines what each preset promises so later milestones cannot silently
redefine them. The generator itself is built in 0.8.0; until then this is
the contract to implement against.

- **Clean:** minimal WPF app, chosen shell only, basic theme, MVVM.
  No dashboard, gallery, database, updater, tray, diagnostics UI, telemetry,
  or release infrastructure unless explicitly selected. Disabling a feature
  removes its package references and registrations — not just hides UI.
- **Recommended:** clean + polished shell, theme/settings foundation,
  logging, diagnostics, standard state patterns, tests.
- **Full:** recommended + demo dashboard, UI gallery, optional service
  modules, distribution options, release automation.
- **Gallery/demo:** reference showcase of the complete UI system and optional
  effects. Never a dependency of product presets.

The 0.1.0 minimal app (`src/DevTemWpf.App`) is the embryo of the **clean**
preset. Rule: new features must not turn the clean scaffold into an all-on
application (verified per milestone by building the clean output).
