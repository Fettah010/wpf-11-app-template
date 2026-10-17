# ADR-0012: Single-tree dotnet-new templates

- Status: Accepted
- Date: 2026-10-14
- Milestone: 0.8.0

## Context

Roadmap 0.8.0 needs reproducible scaffolding with removable modules, page/
service generators, and verified output — without a parallel template tree
that rots.

## Decision

- One tree: root `.template.config/template.json` over `src/` (+ support
  files). `#if` directives are inert comments in repo builds; the engine
  processes them at instantiate. Conditional `exclude` drops module files;
  `sourceName` renames; `preferNameDirectory` nests.
- Symbols: `Preset` (Clean/Recommended/Full) + computed
  IncludeDashboard/Diagnostics/Services + `IncludeTests`. Shell/theme
  value-injection CUT after empirical disproof (choice keys substitute,
  custom values ignored; multi-line `#if` initializers invalid C#).
- Item templates (`devtemwpf-page`, `devtemwpf-service`) use `sourceName`
  for `-n` renaming (not `replaces`/`fileRename` symbols). No VSIX (CLI
  templates surface in VS Create-Project automatically; Add-Item dialog
  doesn't take item templates — documented).
- `eng/test-templates.ps1` verifies install→generate→residue→modules→
  restore/build/test/launch→cleanup outside the repo. CI runs Clean.

## Evidence

- Full matrix green (Clean/Recommended/Full/FullNoTests + items):
  restore/build/test/launch per combo; residue/module asserts.
- Repo suite unaffected (77/77; directives are comments).

## Consequences

- Per-service flags deferred (bundles proven). NuGet template packaging is
  a 1.0 distribution concern (mechanism documented in template docs).
- Template authors: repo builds must stay green WITH directives present.

## Revisit triggers

Per-service flags — **resolved in 1.6.0 by ADR-0017** (per-module flags + a
`Custom` preset). Template-package publishing (1.0) — done.
