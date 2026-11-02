#Requires -Version 5.1
<#
.SYNOPSIS
  Measures large-list input and decorative-effect responsiveness on Windows.
.DESCRIPTION
  Uses the Release app and UI Automation on an interactive desktop. It loads
  the Data page's 10,000-row sample, records load and PageDown dispatch times,
  then samples process CPU for the gradient and warp effects individually.
  Settings are seeded for this run and restored byte-for-byte afterward.
.EXAMPLE
  powershell -ExecutionPolicy Bypass -File eng/measure-large-list-effects.ps1
#>
param(
  [int]$Runs = 3,
  [int]$EffectSampleSeconds = 5,
  [int]$PageDownEvents = 20
)

$ErrorActionPreference = 'Stop'
if ($Runs -lt 1 -or $EffectSampleSeconds -lt 1 -or $PageDownEvents -lt 1) {
  throw 'Runs, EffectSampleSeconds, and PageDownEvents must be positive.'
}

$root = Split-Path -Parent $PSScriptRoot
$appOutput = Join-Path $root 'src\DevTemWpf.App\bin\Release'
$exeCandidates = @(Get-ChildItem -LiteralPath $appOutput -Recurse -Filter 'DevTemWpf.App.exe' |
  Where-Object { $_.FullName -notmatch '\\win-[^\\]+\\' })
if ($exeCandidates.Count -ne 1) {
  throw "Expected one Release *App.exe under src; found $($exeCandidates.Count). Build Release first."
}
$exe = $exeCandidates[0].FullName

$existing = @(Get-CimInstance Win32_Process -Filter "Name='$($exeCandidates[0].Name)'" |
  Where-Object { $_.ExecutablePath -eq $exe })
if ($existing.Count -gt 0) {
  throw "The Release app is already running (PID $($existing[0].ProcessId)); close it before measuring."
}

Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class DevTemMeasureNative
{
    [DllImport("user32.dll")] public static extern uint GetDpiForWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
    [DllImport("user32.dll")] public static extern void mouse_event(int flags, int dx, int dy, int data, UIntPtr extra);
}
'@

$settingsPath = Join-Path $env:APPDATA 'Wpf11AppTemplate\settings.json'
$settingsExisted = Test-Path -LiteralPath $settingsPath
$settingsBackup = if ($settingsExisted) { [IO.File]::ReadAllBytes($settingsPath) } else { $null }
$settingsDir = Split-Path -Parent $settingsPath
$settingsSeed = @{
  Theme = 0; BackdropEnabled = $false; Shell = 1; Density = 0; AccentOverride = $null
  Motion = 1; EffectsEnabled = $true; RippleEffectEnabled = $true
  GradientEffectEnabled = $true; WarpEffectEnabled = $false; UpdateChannel = 0
  ExpandedDataNodes = @(); Workspace = @{ OpenDocuments = @('home'); SelectedDocument = 'home'; DetailWidth = 280; IsDetailOpen = $false }
  MinimizeToTray = $false; Culture = ''
} | ConvertTo-Json -Depth 6

function Find-Element([System.Windows.Automation.AutomationElement]$Scope, [string]$Name) {
  $condition = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::NameProperty, $Name)
  $Scope.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
}

function Wait-ForElement(
  [System.Windows.Automation.AutomationElement]$Scope,
  [string]$Name,
  [int]$TimeoutSeconds = 15
) {
  $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
  while ((Get-Date) -lt $deadline) {
    try {
      $element = Find-Element $Scope $Name
      if ($null -ne $element) { return $element }
    } catch { }
    Start-Sleep -Milliseconds 100
  }
  throw "Timed out waiting for UI Automation element '$Name'."
}

function Activate-Element([System.Windows.Automation.AutomationElement]$Element, [IntPtr]$WindowHandle) {
  [void][DevTemMeasureNative]::SetForegroundWindow($WindowHandle)
  foreach ($patternId in @(
    [System.Windows.Automation.InvokePattern]::Pattern,
    [System.Windows.Automation.SelectionItemPattern]::Pattern)) {
    try {
      $pattern = $Element.GetCurrentPattern($patternId)
      if ($patternId -eq [System.Windows.Automation.InvokePattern]::Pattern) { $pattern.Invoke() }
      else { $pattern.Select() }
      return
    } catch { }
  }

  $bounds = $Element.Current.BoundingRectangle
  if ($bounds.Width -le 0 -or $bounds.Height -le 0) {
    throw "Element '$($Element.Current.Name)' has no supported activation pattern or bounds."
  }
  [void][DevTemMeasureNative]::SetCursorPos(
    [int]($bounds.X + $bounds.Width / 2), [int]($bounds.Y + $bounds.Height / 2))
  [DevTemMeasureNative]::mouse_event(0x0002, 0, 0, 0, [UIntPtr]::Zero)
  [DevTemMeasureNative]::mouse_event(0x0004, 0, 0, 0, [UIntPtr]::Zero)
}

function Set-ToggleState(
  [System.Windows.Automation.AutomationElement]$Scope,
  [string]$Name,
  [System.Windows.Automation.ToggleState]$Desired
) {
  $element = Wait-ForElement $Scope $Name
  $pattern = $element.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
  if ($pattern.Current.ToggleState -eq $Desired) { return 0.0 }

  $watch = [Diagnostics.Stopwatch]::StartNew()
  $pattern.Toggle()
  $deadline = (Get-Date).AddSeconds(3)
  while ((Get-Date) -lt $deadline) {
    try {
      $element = Find-Element $Scope $Name
      $pattern = $element.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
      if ($pattern.Current.ToggleState -eq $Desired) {
        $watch.Stop()
        return [math]::Round($watch.Elapsed.TotalMilliseconds, 1)
      }
    } catch { }
    Start-Sleep -Milliseconds 20
  }
  throw "Toggle '$Name' did not reach $Desired."
}

function Measure-ProcessCpu([System.Diagnostics.Process]$Process, [int]$Seconds) {
  $Process.Refresh()
  $before = $Process.TotalProcessorTime.TotalSeconds
  Start-Sleep -Seconds $Seconds
  $Process.Refresh()
  [math]::Round((($Process.TotalProcessorTime.TotalSeconds - $before) / $Seconds) * 100, 2)
}

function Get-Median([double[]]$Values) {
  if ($Values.Count -eq 0) { throw 'Cannot calculate a median from an empty sample.' }
  $ordered = @($Values | Sort-Object)
  $middle = [int][math]::Floor($ordered.Count / 2)
  if ($ordered.Count % 2 -eq 1) { return [double]$ordered[$middle] }
  return ([double]$ordered[$middle - 1] + [double]$ordered[$middle]) / 2
}

$process = $null
try {
  New-Item -ItemType Directory -Path $settingsDir -Force | Out-Null
  [IO.File]::WriteAllText($settingsPath, $settingsSeed, [Text.UTF8Encoding]::new($false))
  $process = Start-Process -FilePath $exe -PassThru

  $launchDeadline = (Get-Date).AddSeconds(30)
  while ((Get-Date) -lt $launchDeadline) {
    Start-Sleep -Milliseconds 250
    $process.Refresh()
    if ($process.HasExited) { throw "App exited early with code $($process.ExitCode)." }
    if ($process.MainWindowHandle -ne 0) { break }
  }
  if ($process.MainWindowHandle -eq 0) { throw 'No main window appeared within 30 seconds.' }

  $windowHandle = $process.MainWindowHandle
  $window = [System.Windows.Automation.AutomationElement]::FromHandle($windowHandle)
  [void][DevTemMeasureNative]::SetForegroundWindow($windowHandle)
  $dpi = [DevTemMeasureNative]::GetDpiForWindow($windowHandle)
  $nav = Wait-ForElement $window 'Data'
  Activate-Element $nav $windowHandle
  $grid = Wait-ForElement $window 'Data grid'
  $loadButton = Wait-ForElement $window 'Load ten thousand rows'

  $watch = [Diagnostics.Stopwatch]::StartNew()
  Activate-Element $loadButton $windowHandle
  $loadDeadline = (Get-Date).AddSeconds(30)
  $gridRows = 0
  while ((Get-Date) -lt $loadDeadline) {
    try {
      $grid = Find-Element $window 'Data grid'
      $gridRows = $grid.GetCurrentPattern([System.Windows.Automation.GridPattern]::Pattern).Current.RowCount
      if ($gridRows -ge 10000) { break }
    } catch { }
    Start-Sleep -Milliseconds 100
  }
  $watch.Stop()
  $loadMs = $watch.ElapsedMilliseconds
  if ($gridRows -lt 10000) { throw "The 10,000-row grid did not load; UIA row count was $gridRows." }

  $grid.SetFocus()
  $pageDownTimes = @()
  for ($i = 0; $i -lt $PageDownEvents; $i++) {
    $watch.Restart()
    [System.Windows.Forms.SendKeys]::SendWait('{PGDN}')
    $watch.Stop()
    $pageDownTimes += $watch.Elapsed.TotalMilliseconds
  }
  $sortedPageDown = @($pageDownTimes | Sort-Object)
  $pageDownMedian = [math]::Round((Get-Median $pageDownTimes), 1)
  $pageDownMax = [math]::Round(($pageDownTimes | Measure-Object -Maximum).Maximum, 1)
  $process.Refresh()
  $largeListWorkingSetMb = [math]::Round($process.WorkingSet64 / 1MB, 1)

  $effectsNav = Wait-ForElement $window 'Effects'
  Activate-Element $effectsNav $windowHandle
  [void](Wait-ForElement $window 'Play standard preset')
  Start-Sleep -Seconds 2

  $effectCpu = @{}
  [void](Set-ToggleState $window 'Effects master switch' ([System.Windows.Automation.ToggleState]::On))
  [void](Set-ToggleState $window 'Gradient effect switch' ([System.Windows.Automation.ToggleState]::Off))
  [void](Set-ToggleState $window 'Warp effect switch' ([System.Windows.Automation.ToggleState]::Off))
  Start-Sleep -Seconds 1
  $baseline = @()
  for ($i = 0; $i -lt $Runs; $i++) {
    $baseline += Measure-ProcessCpu $process $EffectSampleSeconds
  }
  $effectCpu['Baseline'] = $baseline

  foreach ($effect in @(
    @{ Key = 'Gradient'; Name = 'Gradient effect switch' },
    @{ Key = 'Warp'; Name = 'Warp effect switch' }
  )) {
    [void](Set-ToggleState $window 'Gradient effect switch' ([System.Windows.Automation.ToggleState]::Off))
    [void](Set-ToggleState $window 'Warp effect switch' ([System.Windows.Automation.ToggleState]::Off))
    [void](Set-ToggleState $window $effect.Name ([System.Windows.Automation.ToggleState]::On))
    Start-Sleep -Seconds 2
    $samples = @()
    for ($i = 0; $i -lt $Runs; $i++) {
      $samples += Measure-ProcessCpu $process $EffectSampleSeconds
    }
    $effectCpu[$effect.Key] = $samples
  }

  $toggleTimings = @{}
  foreach ($name in @('Effects master switch', 'Ripple effect switch', 'Gradient effect switch', 'Warp effect switch')) {
    $values = @()
    for ($i = 0; $i -lt $Runs; $i++) {
      $element = Wait-ForElement $window $name
      $pattern = $element.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern)
      $original = $pattern.Current.ToggleState
      $target = if ($original -eq [System.Windows.Automation.ToggleState]::On) {
        [System.Windows.Automation.ToggleState]::Off
      } else {
        [System.Windows.Automation.ToggleState]::On
      }
      $values += Set-ToggleState $window $name $target
      $values += Set-ToggleState $window $name $original
    }
    $toggleTimings[$name] = $values
  }

  $computer = Get-CimInstance Win32_ComputerSystem
  $cpu = Get-CimInstance Win32_Processor | Select-Object -First 1
  $os = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion'
  $medianCpu = @{}
  foreach ($key in $effectCpu.Keys) {
    $medianCpu[$key] = [math]::Round((Get-Median $effectCpu[$key]), 2)
  }
  $medianToggle = @{}
  foreach ($key in $toggleTimings.Keys) {
    $medianToggle[$key] = [math]::Round((Get-Median $toggleTimings[$key]), 1)
  }

  [pscustomobject]@{
    OsBuild = "$($os.CurrentBuildNumber).$($os.UBR)"
    Cpu = $cpu.Name
    RamGb = [math]::Round($computer.TotalPhysicalMemory / 1GB, 1)
    Display = "$([System.Windows.Forms.Screen]::PrimaryScreen.Bounds.Width)x$([System.Windows.Forms.Screen]::PrimaryScreen.Bounds.Height)"
    WindowDpi = $dpi
    DataRows = $gridRows - 1
    GridAutomationRows = $gridRows
    LoadMs = $loadMs
    PageDownEvents = $PageDownEvents
    PageDownMedianMs = $pageDownMedian
    PageDownMaxMs = $pageDownMax
    WorkingSetAfterLargeListMb = $largeListWorkingSetMb
    EffectSampleSeconds = $EffectSampleSeconds
    EffectRuns = $Runs
    EffectCpuPercentOneCoreMedian = $medianCpu
    EffectToggleMedianMs = $medianToggle
  } | ConvertTo-Json -Depth 5
}
finally {
  try {
    if ($null -ne $process -and -not $process.HasExited) {
      $process.Kill()
      $process.WaitForExit(5000) | Out-Null
    }
  } catch {
    if ($null -eq $process -or -not $process.HasExited) { throw }
  } finally {
    if ($settingsExisted) {
      [IO.File]::WriteAllBytes($settingsPath, $settingsBackup)
    } elseif (Test-Path -LiteralPath $settingsPath) {
      Remove-Item -LiteralPath $settingsPath -Force
    }
  }
}
