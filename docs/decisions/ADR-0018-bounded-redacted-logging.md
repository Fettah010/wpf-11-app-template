# ADR-0018: Bounded, redacted file logging

- Status: Accepted
- Date: 2026-10-18
- Milestone: 1.6.0

## Context

`Logging.cs` (ADR-0011) wrote rolling daily files with 14-day retention, no
size cap, and no redaction. Roadmap 8.6 requires logs to be bounded and
private: documented retention, no secrets/tokens/passwords, safe exception
rendering, per-user paths. A crash sample (`ICrashReporter`, ADR-0017) also
needs a redaction policy it can share rather than duplicate.

## Decision

- **Redaction** is applied to every rendered line via a Serilog
  `ITextFormatter` (`RedactingTextFormatter`) wrapping the message-template
  formatter, delegating to `LogPrivacy.Redact`. Two `GeneratedRegex` patterns
  mask secret-shaped key/value pairs (`password`, `token`, `apiKey`,
  `authorization`, `connectionString`, `dsn`, `client_secret`, ...) and
  `Bearer` credentials. The replacement is `***`.
- **The formatter never throws**: any formatting failure writes a safe fallback
  line so a logging fault cannot crash the app.
- **Boundedness**: `FileSizeLimitBytes` = 5 MB with `rollOnFileSizeLimit` and
  `RetainedFileCountLimit` = 14, in addition to daily rolling. All files live
  under `%APPDATA%\Wpf11AppTemplate\logs` (`AppPaths`).
- Redaction is a **defence-in-depth scrub, not a guarantee**: call sites must
  still avoid logging raw secrets (see `docs/guides/`).
- The crash sample reuses `LogPrivacy.Redact`, so there is exactly one
  redaction implementation.

## Consequences

- Logs are bounded in both time and size, and secret-shaped values are masked
  before they reach disk.
- A regex scrub can over- or under-match; it is intentionally conservative and
  unit-tested (`LogPrivacyTests`), and never a substitute for not logging a
  secret.
- The logging module now ships three files (`Logging.cs`, `LogPrivacy.cs`,
  `RedactingTextFormatter.cs`), all excluded together when logging is off.

## Revisit triggers

A structured/JSON sink (where redaction would move to the property level) or a
secret-detection library with fewer false negatives.
