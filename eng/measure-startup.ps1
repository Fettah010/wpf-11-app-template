#Requires -Version 5.1
<#
.SYNOPSIS
  Measures cold-start time (launch to main window) and working set.
  Usage: powershell -ExecutionPolicy Bypass -File eng/measure-startup.ps1 [-Runs 5]
#>
param([int]$Runs = 5)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
# Resolve dynamically (like smoke.ps1) so renamed projects and future
# multi-project layouts keep working; fail clearly instead of measuring
# the wrong (or no) binary.
$candidates = @(Get-ChildItem -LiteralPath (Join-Path $root 'src') -Recurse -Filter '*.App.exe' |
  Where-Object { $_.FullName -like '*\Release\*' } | Select-Object -ExpandProperty FullName)
if ($candidates.Count -eq 0) {
  Write-Host 'Build Release first (no *App.exe under src/).' -ForegroundColor Red
  exit 1
}
$exe = $candidates[0]
if ($candidates.Count -gt 1) {
  Write-Host "Multiple Release app exes; measuring the first:" -ForegroundColor Yellow
  $candidates | ForEach-Object { Write-Host "  $_" }
}

$starts = @()
$sets = @()
for ($i = 1; $i -le $Runs; $i++) {
  $sw = [Diagnostics.Stopwatch]::StartNew()
  $proc = Start-Process -FilePath $exe -PassThru
  try {
    $deadline = (Get-Date).AddSeconds(30)
    while ((Get-Date) -lt $deadline) {
      Start-Sleep -Milliseconds 100
      $proc.Refresh()
      if ($proc.HasExited) { break }
      if ($proc.MainWindowHandle -ne 0) { break }
    }
    $sw.Stop()
    $proc.Refresh()
    if ($proc.HasExited -or $proc.MainWindowHandle -eq 0) {
      Write-Host "Run ${i}: FAILED (no window)" -ForegroundColor Red
      exit 1
    }
    Start-Sleep -Seconds 2
    $proc.Refresh()
    $wsMb = [math]::Round($proc.WorkingSet64 / 1MB, 1)
    $starts += [math]::Round($sw.Elapsed.TotalSeconds, 2)
    $sets += $wsMb
    Write-Host "Run ${i}: window in $($starts[-1])s, working set ${wsMb} MB"
  }
  finally {
    if (-not $proc.HasExited) { $proc.Kill(); $proc.WaitForExit(5000) | Out-Null }
  }
  Start-Sleep -Seconds 2
}

function Median([double[]]$Values) {
  $s = $Values | Sort-Object
  $n = $s.Count
  if ($n % 2 -eq 1) { return $s[[math]::Floor($n / 2)] }
  return [math]::Round(($s[$n / 2 - 1] + $s[$n / 2]) / 2, 2)
}

Write-Host ""
Write-Host "cold-start median: $(Median $starts)s, max: $(($starts | Measure-Object -Maximum).Maximum)s"
Write-Host "working-set median: $(Median $sets) MB, max: $(($sets | Measure-Object -Maximum).Maximum) MB"
