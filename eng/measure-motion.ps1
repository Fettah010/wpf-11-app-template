#Requires -Version 5.1
<#
.SYNOPSIS
  1.5.0 proxy performance profile (per motion profile).
.DESCRIPTION
  Launches the app once per run at each motion profile (Auto/Balanced/Rich/
  Reduced/Off) and records cold-start time, working set, and idle CPU. The
  persisted settings.json is backed up and restored, so the caller's real
  settings are untouched.

  All numbers come from the proxy machine (no 8 GB reference machine exists —
  D10/D27); report them as proxy/provisional (protocol §8.4).
.EXAMPLE
  powershell -ExecutionPolicy Bypass -File eng/measure-motion.ps1 -Runs 5 -IdleSeconds 10
#>
param([int]$Runs = 5, [int]$IdleSeconds = 10)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

$candidates = @(Get-ChildItem -LiteralPath (Join-Path $root 'src') -Recurse -Filter '*.App.exe' |
  Where-Object { $_.FullName -like '*\Release\*' } | Select-Object -ExpandProperty FullName)
if ($candidates.Count -eq 0) {
  Write-Host 'Build Release first (no *App.exe under src/).' -ForegroundColor Red
  exit 1
}
$exe = $candidates[0]

$settingsPath = Join-Path $env:APPDATA 'Wpf11AppTemplate\settings.json'
$settingsDir = Split-Path -Parent $settingsPath
$backup = if (Test-Path -LiteralPath $settingsPath) { Get-Content -LiteralPath $settingsPath -Raw } else { $null }

# MotionMode values: Auto=0, Rich(Full)=1, Reduced=2, Off=3, Balanced=4.
$profiles = [ordered]@{ Auto = 0; Balanced = 4; Rich = 1; Reduced = 2; Off = 3 }

function Median([double[]]$Values) {
  $s = $Values | Sort-Object
  $n = $s.Count
  if ($n % 2 -eq 1) { return $s[[math]::Floor($n / 2)] }
  return [math]::Round(($s[$n / 2 - 1] + $s[$n / 2]) / 2, 2)
}

try {
  foreach ($name in $profiles.Keys) {
    New-Item -ItemType Directory -Force -Path $settingsDir | Out-Null
    Set-Content -LiteralPath $settingsPath -Encoding UTF8 `
      -Value ("{{ `"Motion`": {0}, `"EffectsEnabled`": true }}" -f $profiles[$name])

    $starts = @(); $sets = @(); $cpus = @()
    for ($i = 1; $i -le $Runs; $i++) {
      $sw = [Diagnostics.Stopwatch]::StartNew()
      $proc = Start-Process -FilePath $exe -PassThru
      try {
        $deadline = (Get-Date).AddSeconds(30)
        while ((Get-Date) -lt $deadline) {
          Start-Sleep -Milliseconds 100
          $proc.Refresh()
          if ($proc.HasExited -or $proc.MainWindowHandle -ne 0) { break }
        }
        $sw.Stop()
        $proc.Refresh()
        if ($proc.HasExited -or $proc.MainWindowHandle -eq 0) {
          Write-Host "  $name run ${i}: FAILED (no window)" -ForegroundColor Red
          continue
        }

        Start-Sleep -Seconds 2
        $samples = (Get-Counter "\Process($($proc.ProcessName))\% Processor Time" `
          -MaxSamples 3 -SampleInterval 2 -ErrorAction SilentlyContinue).CounterSamples.CookedValue
        $proc.Refresh()
        $starts += [math]::Round($sw.Elapsed.TotalSeconds, 2)
        $sets += [math]::Round($proc.WorkingSet64 / 1MB, 1)
        $cpus += [math]::Round((($samples | Measure-Object -Average).Average) / $env:NUMBER_OF_PROCESSORS, 2)
      }
      finally {
        if (-not $proc.HasExited) { $proc.Kill(); $proc.WaitForExit(5000) | Out-Null }
      }
      Start-Sleep -Seconds 2
    }

    if ($starts.Count -gt 0) {
      Write-Host ("{0,-9} start median {1,5}s (max {2}s)  WS median {3,6} MB  idle {4,5}% one-core-equiv" -f `
        $name, (Median $starts), (($starts | Measure-Object -Maximum).Maximum), `
        (Median $sets), (Median $cpus))
    }
    else {
      Write-Host ("{0,-9} no successful runs" -f $name) -ForegroundColor Red
    }
  }
}
finally {
  if ($null -ne $backup) {
    Set-Content -LiteralPath $settingsPath -Value $backup -Encoding UTF8
  }
  elseif (Test-Path -LiteralPath $settingsPath) {
    Remove-Item -LiteralPath $settingsPath -Force
  }
}
