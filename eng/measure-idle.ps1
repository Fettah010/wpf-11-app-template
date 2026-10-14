#Requires -Version 5.1
<#
.SYNOPSIS
  Measures idle behavior: launches the app, waits idle, samples CPU + memory.
  Usage: powershell -ExecutionPolicy Bypass -File eng/measure-idle.ps1 [-IdleSeconds 30]
#>
param([int]$IdleSeconds = 30)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$exe = Get-ChildItem -LiteralPath (Join-Path $root 'src') -Recurse -Filter '*.App.exe' |
  Where-Object { $_.FullName -like '*\Release\*' } | Select-Object -First 1 -ExpandProperty FullName
if (-not $exe) {
  Write-Host 'Build Release first (no *App.exe under src/).' -ForegroundColor Red
  exit 1
}

$proc = Start-Process -FilePath $exe -PassThru
try {
  $deadline = (Get-Date).AddSeconds(20)
  while ((Get-Date) -lt $deadline) {
    Start-Sleep -Milliseconds 500
    $proc.Refresh()
    if ($proc.HasExited -or $proc.MainWindowHandle -ne 0) { break }
  }
  if ($proc.HasExited) { Write-Host 'App exited during startup.' -ForegroundColor Red; exit 1 }

  $cpu = Get-Counter "\Process($($proc.ProcessName))\% Processor Time" -ErrorAction SilentlyContinue
  Start-Sleep -Seconds $IdleSeconds
  $proc.Refresh()
  $samples = (Get-Counter "\Process($($proc.ProcessName))\% Processor Time" -MaxSamples 3 -SampleInterval 2 -ErrorAction SilentlyContinue).CounterSamples.CookedValue
  $wsMb = [math]::Round($proc.WorkingSet64 / 1MB, 1)
  Write-Host "idle ${IdleSeconds}s: CPU avg $([math]::Round(($samples | Measure-Object -Average).Average / $env:NUMBER_OF_PROCESSORS, 2))% of one core equivalent (raw samples: $($samples -join ', '))"
  Write-Host "working set after idle: ${wsMb} MB"
}
finally {
  if (-not $proc.HasExited) { $proc.Kill(); $proc.WaitForExit(5000) | Out-Null }
}
