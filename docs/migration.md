# Migration & upgrade policy

## 0.x prereleases → 1.0.0

`0.1.0`–`0.9.0` were construction scaffolding, not stable APIs. To adopt
1.0.0 from any prerelease:

1. **Regenerate** (recommended): `dotnet new devtemwpf-app -n MyApp
   --Preset <matching>` and port your pages/services over. The per-version
   reports (`docs/releases/`) are the changelog to port against.
2. **Hand-upgrade**: follow `CHANGELOG.md` top-down; pay attention to
   renamed contracts (frozen at rc.1 — see `docs/frozen-surface.md`).

No database migration is needed: `ShellSettings` JSON ignores unknown
fields and defaults missing ones; SQLite uses `user_version` upgrades.

## After 1.0.0 (semantic versioning)

- **Patch** (1.0.x): bug fixes only; drop-in safe.
- **Minor** (1.x.0): additive features (new pages, services, tokens);
  existing contracts untouched.
- **Major** (2.0.0): may break frozen surface; ships a migration guide and
  codemods where automatable.
- Deprecations: one minor of warnings before removal, documented in the
  changelog with the replacement.

## Supported extension points (stable)

- Pages: `INavigableView<T>` + transient registration + nav item.
- Services: implement the `I*` contracts, register in `AppComposition`.
- Tokens: add `App*` aliases (never redefine upstream keys).
- Settings: additive fields on `ShellSettings` (old files keep loading).
- Scaffolding: `devtemwpf-page` / `devtemwpf-service` item templates.
- Template options: additive symbols only; renames cut a new rc/major.
