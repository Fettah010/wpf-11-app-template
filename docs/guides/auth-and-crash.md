# Optional samples: authentication and crash reporting

Both samples are **strictly opt-in**. They are reachable only through the
`Custom` preset (`--CustomAuth`, `--CustomCrash`) and never appear in the
Clean/Recommended/Full presets. Neither ships telemetry by default.

## Auth sample (MSAL / Entra ID)

Adds `Microsoft.Identity.Client` (MIT) and an `IAuthService` over a public
client. There is **no client secret** — a public desktop app must not embed
one.

Configuration (empty by default; supply out of tree):

```jsonc
// appsettings.json or environment (Auth__ClientId, Auth__TenantId, ...)
"Auth": {
  "ClientId": "",          // Entra ID application (client) id — not a secret
  "TenantId": "",          // tenant id or verified domain — not a secret
  "Scopes": [ "User.Read" ]
}
```

- Unconfigured ⇒ `IsConfigured == false`, `SignInAsync` returns a
  "not configured" result, and **the token provider is never called** (no
  network). On startup a silent, cache-only restore runs; it also does nothing
  when unconfigured.
- Wire a sign-in button to `IAuthService.SignInAsync` / `SignOutAsync` where it
  fits your shell; the sample deliberately does not add UI.

## Crash sample (local-first)

Adds an `ICrashReporter` that writes redacted reports to
`%LOCALAPPDATA%\Wpf11AppTemplate\crashes`. No vendor, no upload.

- On startup, `Install()` attaches handlers for
  `AppDomain.UnhandledException`, WPF `DispatcherUnhandledException`, and
  `TaskScheduler.UnobservedTaskException` — only when `Crash:Enabled` is true.
- Reports are redacted with the same `LogPrivacy.Redact` used by the log
  (secret-shaped keys, bearer tokens), so payloads never carry obvious secrets.
- The **adapter seam** `ICrashSink` lets you plug a user-chosen reporter
  (e.g. an opt-in exporter) while keeping the redacted payload; replace the
  default `LocalFileCrashSink` in DI. Do not add default telemetry.
- Capture never throws to the caller; a failing sink is swallowed after a
  warning.

```jsonc
"Crash": { "Enabled": true }
```

## Security / privacy posture

- No secrets in source, config defaults, or logs; environment-based config.
- Disabled without configuration; no network unless a user acts on configured
  auth.
- `ICrashSink` receives already-redacted content.
- Evidence: `AuthServiceTests` and `CrashReporterServiceTests` cover
  "disabled without configuration" and redaction; the release report records
  the secret-leak scan and license review.
