# Packaging, distribution, and updates

Three supported profiles. Each is a separate, tested configuration — never
mix update mechanisms across them.

"Script verification" (build, package, hash, signature *check*) is **not**
signing. This repo contains no certificate and produces no signed artifact;
only an owner-controlled run with a real certificate does. See §Signing.

## Profile matrix

| Profile | Updater | Signing | Store |
|---|---|---|---|
| Portable (zip) | none | optional (owner cert) | n/a |
| Installed (Velopack) | Velopack | recommended (owner cert) | n/a |
| MSIX / Store | Store | required (owner/Store cert) | required |

## Portable (zip)

```powershell
dotnet publish src/DevTemWpf.App -c Release -r win-x64 --self-contained `
  -p:PublishSingleFile=true -o publish/portable
```

Or via the repository script (publishes, zips, hashes, and *reports* the
Authenticode state honestly):

```powershell
powershell -ExecutionPolicy Bypass -File eng/pack-portable.ps1
powershell -ExecutionPolicy Bypass -File eng/pack-portable.ps1 -SelfContained
```

No updater runs (feed empty by default); the Diagnostics page reports updates
as not configured. Checksums: `Get-FileHash -Algorithm SHA256 publish/portable/*.exe`
published alongside the artifact. The script writes `<zip>.sha256` and prints
`UNSIGNED ARTIFACT` when no valid signature is present.

## Installed (Velopack)

Velopack (`Velopack` 1.2.161, MIT) takes compiler output and produces an
installer, delta updates, and portable packages (`vpk` CLI — see
[velopack.io](https://velopack.io)). Feed layout: a static file/HTTP host
serving `releases.{channel}.json` + `.nupkg` deltas.

- Feed URLs live in `appsettings.json` (`Update.StableFeedUrl`,
  `Update.BetaFeedUrl`); the channel (`Stable`/`Beta`) is a user setting.
- Flow (Diagnostics page): Check → Download (progress) → restart applies.
  Checks are explicit only — the app never phones home on its own.
- Dev/uninstalled runs: `VelopackApp.Build().Run()` is safe; checks without
  a feed report Disabled; checks with a feed but no install surface errors
  as Failed status text (never a crash).
- Manual test matrix (record results in the release report): fresh install,
  check-no-update, check-update → download → restart-applies version,
  channel switch, dev-run Disabled, corrupt-feed Failed.

## MSIX / Store

Path (not automated in this repo): package the published output with the
Windows App SDK packaging project / `makeappx`, sign (see below), submit.
MSIX apps must NOT use Velopack (Store handles updates) — guard by leaving
feeds empty in Store builds.

## Signing

No certificates or publisher secrets live in this repo, and none are produced
by any `eng` script. The following are **owner-owned secrets** that must be
supplied outside the tree (CI secret / local keystore), never committed:

- Code-signing certificate (PFX/PEM) + password, or an EV/HSM reference.
- Timestamp authority URL (recommended: a public RFC 3161 TSA).
- Store/partner-center publisher credentials.

For installed/MSIX artifacts: code-sign with the owner cert. Self-signed smoke
testing only:

```powershell
$cert = New-SelfSignedCertificate -Type CodeSigningCert -Subject "CN=DevTemWpf Test"
Export-PfxCertificate $cert -FilePath test-sign.pfx -Password (Read-Host -AsSecureString)
```

Document thumbprint + timestamp authority in release notes. Until a real
certificate run happens, every published claim must say **unsigned**, and
checksum-only verification must not be described as signing.

## Version and tag rules

- App releases follow the roadmap (`0.x` prerelease → `1.0.0-rc.1` →
  `1.0.0` stable). Tags `v<version>` point at the release commit.
- `CHANGELOG.md` entry per version; GitHub Release notes link the
  completion report (`docs/releases/<version>-report.md`).
- Checksums (SHA256) published for every downloadable artifact.
