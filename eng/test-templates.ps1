#Requires -Version 5.1
<#
.SYNOPSIS
  Template matrix gate: installs the repo template, generates each supported
  preset/shell outside the repo tree, and verifies restore/build/test/launch
  plus module presence/absence. See docs/releases/0.8.0-plan.md (1.4.0 adds
  the Shell dimension: every shell generates + builds + launches; presets
  keep full smoke; Full+Tabbed is the high-risk full smoke).
  CI splits the gate into parallel jobs (presets vs shells, source vs
  package) to bound wall time; defaults run everything (the full gate).
.EXAMPLE
  powershell -ExecutionPolicy Bypass -File eng/test-templates.ps1
  powershell -ExecutionPolicy Bypass -File eng/test-templates.ps1 -Presets Clean
  powershell -ExecutionPolicy Bypass -File eng/test-templates.ps1 -Shells Top, Tabbed
  powershell -ExecutionPolicy Bypass -File eng/test-templates.ps1 -Shells Sidebar
  powershell -ExecutionPolicy Bypass -File eng/test-templates.ps1 -NoPresets
#>
param(
  [string[]]$Presets = @('Clean', 'Recommended', 'Full'),
  [string[]]$Shells = @('Sidebar', 'Rail', 'Top', 'Tabbed'),
  [switch]$NoPresets,
  [string]$PackagePath = ''
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$work = Join-Path ([IO.Path]::GetTempPath()) ('devtem-tmpl-' + [Guid]::NewGuid().ToString('N'))
$failures = 0

# Isolation (1.1.0): per-run CLI home (template hive) + per-run logs, so
# concurrent runs never share state and the developer's global template
# registrations are untouched. Restored/removed in the finally block below.
# NUGET_PACKAGES keeps pointing at the user's cache (read-through reuse:
# no per-run package re-download, and nothing hive-specific to clean).
$cliHome = Join-Path $work 'cli-home'
$logOut = Join-Path $work 'invoke.out.log'
$logErr = Join-Path $work 'invoke.err.log'
$oldCliHome = $env:DOTNET_CLI_HOME
$oldNugetPackages = $env:NUGET_PACKAGES

function Assert-True([bool]$Condition, [string]$Message) {
  if (-not $Condition) {
    Write-Host "ASSERT FAILED: $Message" -ForegroundColor Red
    $script:failures++
  }
}

function Invoke-Native([string]$Exe, [string[]]$Arguments, [string]$WorkDir) {
  $p = Start-Process -FilePath $Exe -ArgumentList $Arguments -WorkingDirectory $WorkDir `
    -NoNewWindow -Wait -PassThru -RedirectStandardOutput $logOut -RedirectStandardError $logErr
  if ($p.ExitCode -ne 0) {
    Write-Host "COMMAND FAILED ($($p.ExitCode)): $Exe $($Arguments -join ' ')" -ForegroundColor Red
    Get-Content $logErr | Select-Object -First 15
    $script:failures++
    return $false
  }
  return $true
}

function Test-Combo([string]$Preset, [string]$ExtraArgs, [bool]$WithTests, [string]$DirName, [string]$Shell = 'Sidebar', [bool]$LaunchOnly = $false) {
  $name = "Tmpl$Preset$Shell"
  if ($Shell -eq 'Sidebar') { $name = "Tmpl$Preset" }
  $dir = Join-Path $work $DirName
  Write-Host "`n===== $Preset / $Shell ($ExtraArgs) =====" -ForegroundColor Cyan

  $genArgs = @('new', 'devtemwpf-app', '-n', $name, '-o', $dir, '--Preset', $Preset, '--Shell', $Shell) + ($ExtraArgs -split ' ' | Where-Object { $_ })
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
  # Stray conditional tokens in comments (1.4.0 T08: a `//#if` mention inside
  # a comment silently truncates the generated file from that line on). Real
  # directives are removed by the engine, so ANY remaining token is a defect.
  $strayIf = @(Get-ChildItem -LiteralPath (Join-Path $appDir 'src') -Recurse -File -Include *.cs -ErrorAction SilentlyContinue |
    Select-String -Pattern '//#(if|else|endif)' -ErrorAction SilentlyContinue)
  Assert-True ($strayIf.Count -eq 0) "Stray template directive tokens: $($strayIf -join '; ')"

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

  # Shell seed (1.4.0): the generated default must carry the chosen shell and
  # no unreplaced template token.
  $settingsCs = Get-Content (Join-Path $srcDir 'Services\ShellSettings.cs') -Raw
  Assert-True ($settingsCs -match "Enum\.TryParse<ShellMode>\(`"$Shell`"") "$Preset/$Shell default seed"
  Assert-True ($settingsCs -notmatch 'DevTemWpfShellDefault') "$Preset/$Shell no unreplaced token"

  if ($LaunchOnly) {
    if (-not (Invoke-Native 'dotnet' @('restore') $appDir)) { return }
    if (-not (Invoke-Native 'dotnet' @('build', '-c', 'Release', '--no-restore', '--warnaserror') $appDir)) { return }
    $exe = Get-ChildItem -LiteralPath $appDir -Recurse -Filter "$name.App.exe" -ErrorAction SilentlyContinue |
      Where-Object { $_.FullName -match 'Release' } | Select-Object -First 1
    Assert-True ($null -ne $exe) "$Preset/$Shell exe built"
    if ($null -eq $exe) { return }
    $p = Start-Process -FilePath $exe.FullName -PassThru
    Start-Sleep 20
    $p.Refresh()
    $hwnd = 0
    try { $hwnd = $p.MainWindowHandle } catch { }
    Assert-True ((-not $p.HasExited) -and ($hwnd -ne 0)) "$Preset/$Shell launched with window"
    try { Stop-Process -Id $p.Id -Force -ErrorAction Stop } catch { }
    if ($script:failures -eq 0) { Write-Host "OK: $Preset/$Shell build+launch" -ForegroundColor Green }
  }
  elseif ($WithTests) {
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
  New-Item -ItemType Directory -Path $cliHome -Force | Out-Null
  $env:DOTNET_CLI_HOME = $cliHome
  $userPackages = Join-Path $env:USERPROFILE '.nuget\packages'
  if (Test-Path -LiteralPath $userPackages) { $env:NUGET_PACKAGES = $userPackages }
  $installSource = if ($PackagePath) { $PackagePath } else { $root }
  $uninstallId = if ($PackagePath) { 'DevTemWPF.Templates' } else { $root }
  try { & dotnet new uninstall $uninstallId 2>&1 | Out-Null } catch { }
  if (-not (Invoke-Native 'dotnet' @('new', 'install', $installSource) $root)) { throw 'template install failed' }

  if (-not $NoPresets) {
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
  }

  # Shell dimension (1.4.0): every non-default shell generates + builds +
  # launches (preset matrix above already smokes the Sidebar default), plus
  # the high-risk Full+Tabbed full smoke (dashboard/charts inside tabs).
  foreach ($shell in @($Shells | Where-Object { $_ -ne 'Sidebar' })) {
    Test-Combo 'Recommended' '' $false "Rec$shell" $shell $true
  }
  if ($Shells -contains 'Tabbed') {
    Test-Combo 'Full' '' $true 'FullTabbed' 'Tabbed' $false
  }

  # Item templates (into the Full app).
  if ((-not $NoPresets) -and ($Presets -contains 'Full')) {
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
  # Everything (template hives, generated apps) lives under the per-run
  # $work dir, so wholesale deletion is the cleanup — per-ID uninstalls are
  # redundant inside a throwaway hive and only cost minutes on slow runners.
  # Restore the caller's environment first; global state was never touched.
  if ($null -eq $oldCliHome) { [Environment]::SetEnvironmentVariable('DOTNET_CLI_HOME', $null, 'Process') }
  else { $env:DOTNET_CLI_HOME = $oldCliHome }
  if ($null -eq $oldNugetPackages) { [Environment]::SetEnvironmentVariable('NUGET_PACKAGES', $null, 'Process') }
  else { $env:NUGET_PACKAGES = $oldNugetPackages }
  if (($script:failures -eq 0) -and (Test-Path $work)) {
    # Long-path-safe removal (generated bin/obj trees nest deeply); warn —
    # never fail the product gate — with the cause when removal struggles.
    $removed = $false
    foreach ($target in @($work, "\\?\$work")) {
      try { Remove-Item -LiteralPath $target -Recurse -Force -ErrorAction Stop; $removed = $true; break }
      catch { Write-Host "cleanup retry ($target): $($_.Exception.Message)" -ForegroundColor Yellow }
    }
    if (-not $removed) { Write-Host "WARNING: work-dir cleanup failed, kept for manual removal: $work" -ForegroundColor Yellow }
  }
  elseif (Test-Path $work) { Write-Host "Work dir kept for diagnosis: $work" -ForegroundColor Yellow }
}

if ($failures -gt 0) { Write-Host "`nTEMPLATE MATRIX FAILED ($failures)" -ForegroundColor Red; exit 1 }
Write-Host "`nTEMPLATE MATRIX PASSED" -ForegroundColor Green
