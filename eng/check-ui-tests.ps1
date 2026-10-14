#Requires -Version 5.1
<#
.SYNOPSIS
  Required UI-automation gate: runs the FlaUI suite and fails when required
  cases did not execute, so a skip can never report as green.
.DESCRIPTION
  Runs tests/DevTemWpf.UiAutomation (Release, no-build) in enabled mode and
  parses the Microsoft Testing Platform summary. Fails when the run itself
  fails, when zero tests ran, or when any test was skipped. Local convenience
  skips (DEVTEM_RUN_UI_TESTS=0 / headless) remain for ad-hoc runs, but this
  gate must execute on an interactive Windows session/CI runner.
.EXAMPLE
  powershell -ExecutionPolicy Bypass -File eng/check-ui-tests.ps1
#>

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

if ($env:DEVTEM_RUN_UI_TESTS -eq '0') {
  Write-Host 'FAILED: DEVTEM_RUN_UI_TESTS=0 forces UI skips; unset it for the required gate.' -ForegroundColor Red
  exit 1
}

$csproj = Join-Path $root 'tests\DevTemWpf.UiAutomation\DevTemWpf.UiAutomation.csproj'
$out = & dotnet test $csproj -c Release --no-build 2>&1
$code = $LASTEXITCODE
# Strip ANSI color escapes (CI logs carry them; local redirect usually not).
# Note: backtick-e is not an escape in Windows PowerShell 5.1, hence [char]27.
$text = (($out | Out-String) -replace "$([char]27)\[[0-9;]*m", "")

function Get-Count([string]$Name) {
  $m = [regex]::Match($text, "(?m)^\s*$Name" + ':\s*(\d+)\s*$')
  if ($m.Success) { return [int]$m.Groups[1].Value }
  return -1
}

$total = Get-Count 'total'
$failed = Get-Count 'failed'
$skipped = Get-Count 'skipped'
$succeeded = Get-Count 'succeeded'

Write-Host "UI_TESTS total=$total failed=$failed skipped=$skipped succeeded=$succeeded"
$out | Select-Object -Last 8 | ForEach-Object { Write-Host $_ }

if ($code -ne 0) {
  Write-Host "FAILED: UI test run exited with code $code." -ForegroundColor Red
  exit $code
}
if ($total -le 0) {
  Write-Host 'FAILED: zero UI tests ran (expected the 4 ShellFlows cases).' -ForegroundColor Red
  exit 1
}
if ($skipped -ne 0) {
  Write-Host "FAILED: $skipped UI test(s) skipped; required cases must execute (skipped=$skipped total=$total)." -ForegroundColor Red
  exit 1
}

Write-Host 'UI TESTS EXECUTED (no skips)' -ForegroundColor Green
