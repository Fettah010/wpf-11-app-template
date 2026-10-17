<#
.SYNOPSIS
  Produces a portable (framework-dependent) app artifact and its SHA-256,
  and reports the Authenticode state explicitly.

.DESCRIPTION
  This script is *verification*, not signing. It never produces a signed
  artifact: the output is unsigned unless a certificate pipeline (owner-owned
  secret) signs it afterwards. See docs/release/packaging.md.

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File eng/pack-portable.ps1
  powershell -ExecutionPolicy Bypass -File eng/pack-portable.ps1 -SelfContained -Runtime win-x64
#>
param(
  [string]$Configuration = 'Release',
  [string]$Runtime = 'win-x64',
  [string]$OutDir = '',
  [switch]$SelfContained
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
if (-not $OutDir) { $OutDir = Join-Path $root 'artifacts\portable' }
$appProject = Join-Path $root 'src\DevTemWpf.App\DevTemWpf.App.csproj'
if (-not (Test-Path -LiteralPath $appProject)) { throw "App project not found: $appProject" }

Write-Host "Portable pack: $Configuration / $Runtime (self-contained: $([bool]$SelfContained))" -ForegroundColor Cyan
if (Test-Path -LiteralPath $OutDir) { Remove-Item -LiteralPath $OutDir -Recurse -Force }

$publishArgs = @(
  'publish', $appProject, '-c', $Configuration, '-r', $Runtime
)
$publishArgs += if ($SelfContained) { '--self-contained' } else { '--self-contained', 'false' }
$publishArgs += @('-o', $OutDir)

& dotnet @publishArgs
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed ($LASTEXITCODE)" }

$exe = Get-ChildItem -LiteralPath $OutDir -Filter '*.exe' -File | Select-Object -First 1
if (-not $exe) { throw "No executable produced in $OutDir" }

# Explicit, honest signing state - script verification is NOT signing.
$sig = Get-AuthenticodeSignature $exe.FullName
Write-Host "Executable: $($exe.Name)" -ForegroundColor Gray
Write-Host "Authenticode: $($sig.Status) ($($sig.StatusMessage))" -ForegroundColor Yellow
if ($sig.Status -ne 'Valid') {
  Write-Host "UNSIGNED ARTIFACT - do not describe this as signed." -ForegroundColor Yellow
}

$hash = (Get-FileHash -Algorithm SHA256 $exe.FullName).Hash
$zip = Join-Path $root ("artifacts\DevTemWpf-{0}-{1}.zip" -f $Configuration, $Runtime)
if (-not (Test-Path -LiteralPath (Split-Path -Parent $zip))) {
  New-Item -ItemType Directory -Path (Split-Path -Parent $zip) -Force | Out-Null
}
if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip -Force }
Compress-Archive -Path (Join-Path $OutDir '*') -DestinationPath $zip

$sha = [System.Security.Cryptography.SHA256]::Create()
$zipStream = [System.IO.File]::OpenRead($zip)
try { $zipHash = ([BitConverter]::ToString($sha.ComputeHash($zipStream)) -replace '-', '').ToLowerInvariant() }
finally { $zipStream.Dispose(); $sha.Dispose() }

Set-Content -LiteralPath "$zip.sha256" -Value "$zipHash  $(Split-Path -Leaf $zip)" -Encoding ascii
Write-Host "Artifact: $zip" -ForegroundColor Green
Write-Host "SHA-256 : $zipHash" -ForegroundColor Green
Write-Host "Exe hash: $hash" -ForegroundColor Gray
