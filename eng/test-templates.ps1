#Requires -Version 5.1
<#
.SYNOPSIS
  Template matrix gate: installs the repo template, generates each supported
  preset outside the repo tree, and verifies restore/build/test/launch plus
  module presence/absence. See docs/releases/0.8.0-plan.md.
.EXAMPLE
  powershell -ExecutionPolicy Bypass -File eng/test-templates.ps1
  powershell -ExecutionPolicy Bypass -File eng/test-templates.ps1 -Presets Clean
#>
param(
  [string[]]$Presets = @('Clean', 'Recommended', 'Full'),
  [string]$PackagePath = ''
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$work = Join-Path ([IO.Path]::GetTempPath()) ('devtem-tmpl-' + [Guid]::NewGuid().ToString('N'))
$failures = 0

function Assert-True([bool]$Condition, [string]$Message) {
  if (-not $Condition) {
    Write-Host "ASSERT FAILED: $Message" -ForegroundColor Red
    $script:failures++
  }
}

function Invoke-Native([string]$Exe, [string[]]$Arguments, [string]$WorkDir) {
  $p = Start-Process -FilePath $Exe -ArgumentList $Arguments -WorkingDirectory $WorkDir `
    -NoNewWindow -Wait -PassThru -RedirectStandardOutput "$env:TEMP\devtem-tmpl.out" -RedirectStandardError "$env:TEMP\devtem-tmpl.err"
  if ($p.ExitCode -ne 0) {
    Write-Host "COMMAND FAILED ($($p.ExitCode)): $Exe $($Arguments -join ' ')" -ForegroundColor Red
    Get-Content "$env:TEMP\devtem-tmpl.err" | Select-Object -First 15
    $script:failures++
    return $false
  }
  return $true
}

function Test-Combo([string]$Preset, [string]$ExtraArgs, [bool]$WithTests, [string]$DirName) {
  $name = "Tmpl$Preset"
  $dir = Join-Path $work $DirName
  Write-Host "`n===== $Preset ($ExtraArgs) =====" -ForegroundColor Cyan

  $genArgs = @('new', 'devtemwpf-app', '-n', $name, '-o', $dir, '--Preset', $Preset) + ($ExtraArgs -split ' ' | Where-Object { $_ })
  if (-not (Invoke-Native 'dotnet' $genArgs $work)) { return }

  $appDir = $dir
  if (-not (Test-Path (Join-Path $dir 'src'))) {
    $sub = Get-ChildItem -LiteralPath $dir -Directory | Select-Object -First 1
    if ($sub) { $appDir = $sub.FullName }
  }
  $srcDir = Join-Path $appDir "src\$name.App"

  # Residue checks (before build creates bin/obj).
  $nameHits = @(Get-ChildItem -LiteralPath $appDir -Recurse -Filter '*DevTemWpf*' -ErrorAction SilentlyContinue)
  Assert-True ($nameHits.Count -eq 0) "DevTemWpf residue in file names: $($nameHits -join ', ')"
  $contentHits = @(Get-ChildItem -LiteralPath $appDir -Recurse -File -Include *.cs, *.xaml, *.csproj, *.slnx, *.json, *.md, *.ps1 -ErrorAction SilentlyContinue |
    Select-String -Pattern 'DevTemWpf' -ErrorAction SilentlyContinue)
  Assert-True ($contentHits.Count -eq 0) "DevTemWpf residue in content"
  $ifHits = @(Get-ChildItem -LiteralPath (Join-Path $appDir 'src') -Recurse -File -Include *.cs, *.xaml, *.csproj, *.slnx -ErrorAction SilentlyContinue |
    Select-String -Pattern '#if \(|#endif' -ErrorAction SilentlyContinue)
  Assert-True ($ifHits.Count -eq 0) "Template directive residue"

  # Module presence/absence.
  $hasDashboard = Test-Path (Join-Path $srcDir 'Views\DashboardPage.xaml')
  $hasDiagnostics = Test-Path (Join-Path $srcDir 'Views\DiagnosticsPage.xaml')
  $csproj = Get-Content (Join-Path $srcDir "$name.App.csproj") -Raw
  Assert-True (($hasDashboard -eq ($Preset -eq 'Full'))) "$Preset dashboard presence: $hasDashboard"
  Assert-True (($hasDiagnostics -eq ($Preset -ne 'Clean'))) "$Preset diagnostics presence: $hasDiagnostics"
  Assert-True (($csproj -match 'LiveChartsCore') -eq ($Preset -eq 'Full')) "$Preset LiveCharts ref"
  Assert-True (($csproj -match 'Velopack') -eq ($Preset -ne 'Clean')) "$Preset Velopack ref"
  Assert-True (($csproj -match 'Serilog') -eq ($Preset -ne 'Clean')) "$Preset Serilog ref"
  Assert-True (Test-Path (Join-Path $appDir 'FEATURES.md')) "$Preset FEATURES.md"
  Assert-True (Test-Path (Join-Path $appDir 'README.md')) "$Preset README.md"
  $features = Get-Content (Join-Path $appDir 'FEATURES.md') -Raw
  $dashChecked = $features -match '- \[x\] Dashboard'
  Assert-True ($dashChecked -eq ($Preset -eq 'Full')) "$Preset FEATURES dashboard row"

  if ($WithTests) {
    if (-not (Invoke-Native 'powershell' @('-ExecutionPolicy', 'Bypass', '-File', (Join-Path $appDir 'eng\smoke.ps1')) $appDir)) { return }
    Write-Host "OK: $Preset smoke (restore/build/test/launch)" -ForegroundColor Green
  }
  else {
    if (-not (Invoke-Native 'dotnet' @('restore') $appDir)) { return }
    if (-not (Invoke-Native 'dotnet' @('build', '-c', 'Release', '--no-restore', '--warnaserror') $appDir)) { return }
    Write-Host "OK: $Preset build (no tests)" -ForegroundColor Green
  }
}

try {
  New-Item -ItemType Directory -Path $work -Force | Out-Null
  $installSource = if ($PackagePath) { $PackagePath } else { $root }
  $uninstallId = if ($PackagePath) { 'DevTemWPF.Templates' } else { $root }
  try { & dotnet new uninstall $uninstallId 2>&1 | Out-Null } catch { }
  if (-not (Invoke-Native 'dotnet' @('new', 'install', $installSource) $root)) { throw 'template install failed' }

  if ($Presets -contains 'Clean') {
    Test-Combo 'Clean' '' $true 'Clean'
  }
  if ($Presets -contains 'Recommended') {
    Test-Combo 'Recommended' '' $true 'Recommended'
  }
  if ($Presets -contains 'Full') {
    Test-Combo 'Full' '' $true 'Full'
    Test-Combo 'Full' '--IncludeTests false' $false 'FullNoTests'
  }

  # Item templates (into the Full app).
  if ($Presets -contains 'Full') {
  Write-Host "`n===== item templates =====" -ForegroundColor Cyan
  $fullApp = Join-Path $work 'Full'
  $pageDir = Join-Path $root 'templates\items\devtemwpf-page'
  $svcDir = Join-Path $root 'templates\items\devtemwpf-service'
  try { & dotnet new uninstall $pageDir 2>&1 | Out-Null } catch { }
  try { & dotnet new uninstall $svcDir 2>&1 | Out-Null } catch { }
  if (-not (Invoke-Native 'dotnet' @('new', 'install', $pageDir) $root)) { throw 'page template install failed' }
  if (-not (Invoke-Native 'dotnet' @('new', 'install', $svcDir) $root)) { throw 'service template install failed' }
  $target = Join-Path $fullApp 'src\TmplFull.App'
  if (-not (Invoke-Native 'dotnet' @('new', 'devtemwpf-page', '-n', 'OrdersPage', '--Namespace', 'TmplFull.App') $target)) { throw 'page gen failed' }
  if (-not (Invoke-Native 'dotnet' @('new', 'devtemwpf-service', '-n', 'AuditService', '--Namespace', 'TmplFull.App') $target)) { throw 'service gen failed' }
  Assert-True (Test-Path (Join-Path $target 'OrdersPage.xaml')) 'OrdersPage.xaml generated'
  Assert-True (Test-Path (Join-Path $target 'AuditService.cs')) 'AuditService.cs generated'
  $nsHits = @(Select-String -LiteralPath (Join-Path $target 'OrdersPage.xaml.cs') -Pattern 'namespace TmplFull.App')
  Assert-True ($nsHits.Count -gt 0) 'Namespace substitution in generated page'
  if (-not (Invoke-Native 'dotnet' @('build', '-c', 'Release', '--no-restore') $fullApp)) { throw 'Full app rebuild with items failed' }
  Write-Host 'OK: item templates' -ForegroundColor Green
  }
}
finally {
  $uninstallId = if ($PackagePath) { 'DevTemWPF.Templates' } else { $root }
  try { & dotnet new uninstall $uninstallId 2>&1 | Out-Null } catch { }
  try { & dotnet new uninstall (Join-Path $root 'templates\items\devtemwpf-page') 2>&1 | Out-Null } catch { }
  try { & dotnet new uninstall (Join-Path $root 'templates\items\devtemwpf-service') 2>&1 | Out-Null } catch { }
  if (Test-Path $work) { Remove-Item -Recurse -Force $work }
}

if ($failures -gt 0) { Write-Host "`nTEMPLATE MATRIX FAILED ($failures)" -ForegroundColor Red; exit 1 }
Write-Host "`nTEMPLATE MATRIX PASSED" -ForegroundColor Green
