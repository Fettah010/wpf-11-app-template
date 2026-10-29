#Requires -Version 5.1
<#
.SYNOPSIS
  Generator gate for the DevTemWPF item templates (1.7.0).

.DESCRIPTION
  Installs the templates from this repo into an isolated CLI home, generates a
  sample application, adds every item template with a custom name/namespace,
  asserts that no template token survived, then builds with --warnaserror and
  runs the generated unit tests.

  Nothing is written to the caller's global template hive.

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File eng/test-generators.ps1

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File eng/test-generators.ps1 -Preset Full

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File eng/test-generators.ps1 -NoBuild
#>
[CmdletBinding()]
param(
  [ValidateSet('Clean', 'Recommended', 'Full')]
  [string]$Preset = 'Clean',
  [string]$AppName = 'GenApp',
  [string]$PackagePath = '',
  [switch]$NoBuild,
  [switch]$NoTest,
  [switch]$KeepWork
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$work = Join-Path ([IO.Path]::GetTempPath()) ('devtem-gen-' + [Guid]::NewGuid().ToString('N'))
$cliHome = Join-Path $work 'cli-home'
$appDir = Join-Path $work $AppName
$appProject = Join-Path $appDir ('src\' + $AppName + '.App')
$unitProject = Join-Path $appDir ('tests\' + $AppName + '.Unit')
$logOut = Join-Path $work 'invoke.out.log'
$logErr = Join-Path $work 'invoke.err.log'
$failures = 0

# Item templates, the name each is generated with, and the project it targets.
# Test scaffolds intentionally reuse the ViewModel/service names generated above
# so the derived "type under test" symbol is exercised end to end.
$items = @(
  @{ Template = 'devtemwpf-page'; Name = 'MyPage'; Project = 'app' }
  @{ Template = 'devtemwpf-settings'; Name = 'MySettingsPage'; Project = 'app' }
  @{ Template = 'devtemwpf-listdetail'; Name = 'MyListDetailPage'; Project = 'app' }
  @{ Template = 'devtemwpf-datagrid'; Name = 'MyDataGridPage'; Project = 'app' }
  @{ Template = 'devtemwpf-treeview'; Name = 'MyTreeViewPage'; Project = 'app' }
  @{ Template = 'devtemwpf-tabpage'; Name = 'MyTabPage'; Project = 'app' }
  @{ Template = 'devtemwpf-control'; Name = 'MyValueEditor'; Project = 'app' }
  @{ Template = 'devtemwpf-viewmodel'; Name = 'MyViewModel'; Project = 'app' }
  @{ Template = 'devtemwpf-service'; Name = 'MyService'; Project = 'app' }
  @{ Template = 'devtemwpf-test-vm'; Name = 'MyViewModelTests'; Project = 'tests' }
  @{ Template = 'devtemwpf-test-service'; Name = 'MyServiceTests'; Project = 'tests' }
  # Second pass over an existing app: distinct names must not collide, and a
  # name without the Page suffix must still produce a valid companion type.
  @{ Template = 'devtemwpf-page'; Name = 'SecondPage'; Project = 'app' }
  @{ Template = 'devtemwpf-datagrid'; Name = 'InventoryGrid'; Project = 'app' }
)

# Any of these appearing in generated output means a symbol/replacement is broken.
$residueTokens = @(
  'DevTemWpf'
  'SamplePage'
  'SampleSettingsPage'
  'SampleListDetailPage'
  'SampleDataGridPage'
  'SampleTreeViewPage'
  'SampleTabPage'
  'SampleValueEditor'
  'SampleViewModel'
  'SampleService'
  'SampleListItem'
  'SampleDataGridRow'
  'SampleTreeNode'
  'AppBodyStrongText'
)

$oldCliHome = $env:DOTNET_CLI_HOME

function Assert-True([bool]$Condition, [string]$Message) {
  if (-not $Condition) {
    Write-Host "ASSERT FAILED: $Message" -ForegroundColor Red
    $script:failures++
  }
  else {
    Write-Host "  ok: $Message" -ForegroundColor DarkGray
  }
}

function Invoke-Native([string]$Exe, [string[]]$Arguments, [string]$WorkDir) {
  if (-not (Test-Path -LiteralPath $WorkDir)) { New-Item -ItemType Directory -Path $WorkDir -Force | Out-Null }
  $sw = [System.Diagnostics.Stopwatch]::StartNew()
  $p = Start-Process -FilePath $Exe -ArgumentList $Arguments -WorkingDirectory $WorkDir `
    -NoNewWindow -Wait -PassThru -RedirectStandardOutput $logOut -RedirectStandardError $logErr
  $secs = [math]::Round($sw.Elapsed.TotalSeconds, 1)
  if ($p.ExitCode -ne 0) {
    Write-Host "COMMAND FAILED ($($p.ExitCode)) after ${secs}s: $Exe $($Arguments -join ' ')" -ForegroundColor Red
    Write-Host '--- stderr (tail) ---' -ForegroundColor DarkYellow
    Get-Content $logErr -Tail 25 -ErrorAction SilentlyContinue
    Write-Host '--- stdout (tail) ---' -ForegroundColor DarkYellow
    Get-Content $logOut -Tail 40 -ErrorAction SilentlyContinue
    $script:failures++
    return $false
  }
  Write-Host "  $($Arguments -join ' ') (${secs}s)" -ForegroundColor DarkGray
  return $true
}

try {
  New-Item -ItemType Directory -Path $cliHome -Force | Out-Null
  $env:DOTNET_CLI_HOME = $cliHome

  # --- Install app + item templates (from a package when given) ---
  Write-Host "`n== install templates ==" -ForegroundColor Cyan
  $installSource = $root
  if ($PackagePath) {
    $resolved = Resolve-Path -LiteralPath $PackagePath -ErrorAction SilentlyContinue
    if (-not $resolved) { throw "package not found: $PackagePath" }
    $installSource = $resolved.Path
  }
  Write-Host "  source: $installSource" -ForegroundColor DarkGray
  if (-not (Invoke-Native 'dotnet' @('new', 'install', $installSource, '--force') $work)) { exit 1 }

  $listed = & dotnet new list devtemwpf 2>&1 | Out-String
  Assert-True ($listed -match 'devtemwpf-app') 'app template registered'
  foreach ($item in $items) {
    Assert-True ($listed -match [regex]::Escape($item.Template)) "template registered: $($item.Template)"
  }

  # --- Generate the application ---
  Write-Host "`n== generate app ($Preset preset) ==" -ForegroundColor Cyan
  if (-not (Invoke-Native 'dotnet' @('new', 'devtemwpf-app', '-n', $AppName, '-o', $appDir, '--Preset', $Preset) $work)) { exit 1 }
  Assert-True (Test-Path -LiteralPath $appProject) "app project generated: $appProject"

  # --- Add every item template ---
  Write-Host "`n== add item templates ==" -ForegroundColor Cyan
  foreach ($item in $items) {
    $target = if ($item.Project -eq 'tests') { $unitProject } else { $appProject }
    Write-Host "  $($item.Template) -> $($item.Name)" -ForegroundColor Cyan
    $arguments = @('new', $item.Template, '-n', $item.Name, '-o', $target, '--RootNamespace', $AppName)
    if (-not (Invoke-Native 'dotnet' $arguments $work)) { continue }
    $expected = Join-Path $target ($item.Name + '.cs')
    $expectedXaml = Join-Path $target ($item.Name + '.xaml')
    Assert-True ((Test-Path -LiteralPath $expected) -or (Test-Path -LiteralPath $expectedXaml)) "files generated for $($item.Template)"
  }

  # --- No template residue ---
  Write-Host "`n== assert no template tokens survived ==" -ForegroundColor Cyan
  $generatedFiles = Get-ChildItem -LiteralPath $appDir -Recurse -File -Include *.xaml, *.cs, *.csproj, *.props, *.md |
    Where-Object { $_.FullName -notlike '*\bin\*' -and $_.FullName -notlike '*\obj\*' }
  foreach ($token in $residueTokens) {
    $hits = @($generatedFiles | Select-String -Pattern $token -SimpleMatch -ErrorAction SilentlyContinue)
    if ($hits.Count -gt 0) {
      Write-Host "  RESIDUE '$token' in:" -ForegroundColor Red
      $hits | Select-Object -First 5 | ForEach-Object { Write-Host "    $($_.Path):$($_.LineNumber)" -ForegroundColor Red }
      $script:failures++
    }
    else {
      Write-Host "  ok: no '$token'" -ForegroundColor DarkGray
    }
  }

  # --- No scaffold leaks (tool configs, build output, absolute paths) ---
  Write-Host "`n== assert no scaffold leaks ==" -ForegroundColor Cyan
  foreach ($leak in @('.kilo', '.github', 'TestResults')) {
    $path = Join-Path $appDir $leak
    Assert-True (-not (Test-Path -LiteralPath $path)) "no '$leak' directory in generated app"
  }
  $absoluteHits = @($generatedFiles | Select-String -Pattern 'C:\Code\projects' -SimpleMatch -ErrorAction SilentlyContinue)
  Assert-True ($absoluteHits.Count -eq 0) 'no absolute developer paths in generated files'

  # --- Build ---
  if (-not $NoBuild) {
    Write-Host "`n== build Release --warnaserror ==" -ForegroundColor Cyan
    if (-not (Invoke-Native 'dotnet' @('build', $appDir, '-c', 'Release', '--warnaserror') $appDir)) { exit 1 }

    if (-not $NoTest) {
      Write-Host "`n== run generated unit tests ==" -ForegroundColor Cyan
      $unitCsproj = Join-Path $unitProject ($AppName + '.Unit.csproj')
      Assert-True (Test-Path -LiteralPath $unitCsproj) 'unit test project present'
      if (-not (Invoke-Native 'dotnet' @('run', '--project', $unitCsproj, '-c', 'Release', '--no-build') $appDir)) { exit 1 }
    }
  }

  Write-Host "`n== GENERATOR GATE SUMMARY ==" -ForegroundColor Cyan
  if ($failures -eq 0) {
    Write-Host 'GENERATOR GATE PASSED' -ForegroundColor Green
    exit 0
  }
  Write-Host "GENERATOR GATE FAILED ($failures assertion(s))" -ForegroundColor Red
  exit 1
}
finally {
  $env:DOTNET_CLI_HOME = $oldCliHome
  if ($KeepWork -or $failures -gt 0) {
    Write-Host "Work dir kept: $work" -ForegroundColor Yellow
  }
  elseif (Test-Path -LiteralPath $work) {
    foreach ($target in @($work, "\\?\$work")) {
      try { Remove-Item -LiteralPath $target -Recurse -Force -ErrorAction Stop; break } catch { }
    }
  }
}
