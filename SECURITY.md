# Security policy

## Supported versions

| Version | Supported |
|---|---|
| 1.0.x | ✅ security fixes |
| 0.x prereleases | ❌ upgrade to 1.0.x |

## Reporting a vulnerability

**Do not open a public issue.** Use a [private security
advisory](https://github.com/Fettah010/wpf-11-app-template/security/advisories/new)
on this repository so details stay confidential until a fix ships.

## Response

- Triage within 7 days; severity assessed on exploitability × reach.
- Fix released as a patch version with a changelog entry; reporter credited
  (unless anonymity is requested).
- Transitive-dependency advisories are handled the same way (bump + release).

## Scope notes

- No telemetry or network calls ship by default; attack surface is local
  user data (settings, SQLite, logs) plus explicitly configured feeds.
- Signing keys, certificates, and feed credentials must never be committed —
  releases are cut from clean checkouts (verified per release report).
