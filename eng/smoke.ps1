#Requires -Version 5.1
<#
.SYNOPSIS
  0.1.0 smoke gate: restore, build (Release), test, launch check.
  Fails fast with a non-zero exit code; prints each step's outcome.
#>
$ErrorActionPreference = 'Stop'

function Invoke-Step([string]$Name, [scriptblock]$Cmd) {
  Write-Host "`n== $Name ==" -ForegroundColor Cyan
  & $Cmd
  if ($LASTEXITCODE -ne 0) {
    Write-Host "FAILED: $Name (exit $LASTEXITCODE)" -ForegroundColor Red
    exit $LASTEXITCODE
  }
  Write-Host "OK: $Name" -ForegroundColor Green
}

$root = Split-Path -Parent $PSScriptRoot
Set-Location -LiteralPath $root

Invoke-Step 'dotnet --version' { dotnet --version }
Invoke-Step 'restore' { dotnet restore }
# Release only (1.4.0): the repo contains no DEBUG-conditional code, and the
# required gates build Release with warnings as errors — a second Debug build
# doubles combo time for no additional signal.
Invoke-Step 'build Release' { dotnet build -c Release --no-restore }
Invoke-Step 'test Release' { dotnet test -c Release --no-build }

# Launch check: start the app, verify it stays alive with a window, then close it.
# The executable is located dynamically so renamed (generated) apps work too.
Write-Host "`n== launch check ==" -ForegroundColor Cyan
$exe = Get-ChildItem -LiteralPath (Join-Path $root 'src') -Recurse -Filter '*.App.exe' |
  Where-Object { $_.FullName -like '*\Release\*' } | Select-Object -First 1 -ExpandProperty FullName
if (-not $exe -or -not (Test-Path -LiteralPath $exe)) {
  Write-Host 'FAILED: Release *App.exe not found under src/' -ForegroundColor Red
  exit 1
}
$proc = Start-Process -FilePath $exe -PassThru
try {
  $deadline = (Get-Date).AddSeconds(20)
  $ready = $false
  while ((Get-Date) -lt $deadline) {
    Start-Sleep -Milliseconds 500
    $proc.Refresh()
    if ($proc.HasExited) { break }
    if ($proc.MainWindowHandle -ne 0) { $ready = $true; break }
  }
  $proc.Refresh()
  if ($proc.HasExited) {
    Write-Host "FAILED: app exited early with code $($proc.ExitCode)" -ForegroundColor Red
    exit 1
  }
  if (-not $ready) {
    Write-Host 'FAILED: no main window handle within 20s' -ForegroundColor Red
    exit 1
  }
  $wsMb = [math]::Round($proc.WorkingSet64 / 1MB, 1)
  Write-Host "App window up (PID $($proc.Id), working set ${wsMb} MB)" -ForegroundColor Green
}
finally {
  if (-not $proc.HasExited) {
    $proc.Kill()
    $proc.WaitForExit(5000) | Out-Null
  }
}

Write-Host "`nSMOKE PASSED" -ForegroundColor Green
