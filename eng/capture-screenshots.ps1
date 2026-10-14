#Requires -Version 5.1
<#
.SYNOPSIS
  Captures real README/docs screenshots of the reference app (dev-only).
.DESCRIPTION
  Launches the Release app, maximizes its window, captures the Home page and
  the Controls gallery page via window-rect screen capture, and saves PNGs to
  docs/images/. Prints the environment provenance (OS build, theme) that the
  README caption must record. Not part of any release gate.
.EXAMPLE
  powershell -ExecutionPolicy Bypass -File eng/capture-screenshots.ps1
#>

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$imgDir = Join-Path $root 'docs\images'
New-Item -ItemType Directory -Path $imgDir -Force | Out-Null

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

$sig = @'
[DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
[DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
[DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
[DllImport("user32.dll")] public static extern bool SetCursorPos(int X, int Y);
[DllImport("user32.dll")] public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, System.UIntPtr dwExtraInfo);
[DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
'@
Add-Type -MemberDefinition $sig -Name Win32 -Namespace DevTem -PassThru | Out-Null

$exe = Get-ChildItem -LiteralPath (Join-Path $root 'src') -Recurse -Filter '*.App.exe' |
  Where-Object { $_.FullName -like '*\Release\*' } | Select-Object -First 1 -ExpandProperty FullName
if (-not $exe) { Write-Host 'Build Release first.' -ForegroundColor Red; exit 1 }

function Capture-Window([string]$Path) {
  # Pin the app topmost so CopyFromScreen captures our pixels even if
  # another window tries to take focus on a busy agent machine.
  [void][DevTem.Win32]::SetForegroundWindow($proc.MainWindowHandle)
  [void][DevTem.Win32]::SetWindowPos($proc.MainWindowHandle, [IntPtr](-1), 0, 0, 0, 0, 0x0043)
  Start-Sleep -Milliseconds 700
  try {
    $rect = New-Object DevTem.Win32+RECT
    [void][DevTem.Win32]::GetWindowRect($proc.MainWindowHandle, [ref]$rect)
  $w = $rect.Right - $rect.Left
  $h = $rect.Bottom - $rect.Top
  if ($w -le 0 -or $h -le 0) { throw "Empty window rect ${w}x${h}." }
  $bmp = New-Object System.Drawing.Bitmap $w, $h
  try {
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    try { $g.CopyFromScreen($rect.Left, $rect.Top, 0, 0, $bmp.Size) }
    finally { $g.Dispose() }
    $bmp.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
  }
  finally { $bmp.Dispose() }
  Write-Host "saved $Path (${w}x${h})"
  }
  finally {
    [void][DevTem.Win32]::SetWindowPos($proc.MainWindowHandle, [IntPtr](-2), 0, 0, 0, 0, 0x0043)
  }
}

$proc = Start-Process -FilePath $exe -PassThru
try {
  $deadline = (Get-Date).AddSeconds(20)
  while ((Get-Date) -lt $deadline) {
    Start-Sleep -Milliseconds 500
    $proc.Refresh()
    if ($proc.HasExited) { throw "App exited early (code $($proc.ExitCode))." }
    if ($proc.MainWindowHandle -ne 0) { break }
  }
  if ($proc.MainWindowHandle -eq 0) { throw 'No main window within 20s.' }
  Start-Sleep -Seconds 2
  [void][DevTem.Win32]::ShowWindow($proc.MainWindowHandle, 3)  # SW_MAXIMIZE
  Start-Sleep -Seconds 1
  [void][DevTem.Win32]::SetForegroundWindow($proc.MainWindowHandle)
  Start-Sleep -Milliseconds 500

  Capture-Window (Join-Path $imgDir 'gallery-home.png')

  $root_el = [System.Windows.Automation.AutomationElement]::FromHandle($proc.MainWindowHandle)
  $typeCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
    [System.Windows.Automation.ControlType]::DataItem)
  $nameCond = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::NameProperty, 'Controls')
  $nav = $root_el.FindFirst([System.Windows.Automation.TreeScope]::Descendants,
    (New-Object System.Windows.Automation.AndCondition($typeCond, $nameCond)))
  if ($nav -eq $null) { throw 'Controls nav DataItem not found.' }
  $activated = $false
  foreach ($patternId in @(
    [System.Windows.Automation.InvokePattern]::Pattern,
    [System.Windows.Automation.SelectionItemPattern]::Pattern)) {
    try {
      $pattern = $nav.GetCurrentPattern($patternId)
      if ($patternId -eq [System.Windows.Automation.InvokePattern]::Pattern) { $pattern.Invoke() }
      else { $pattern.Select() }
      $activated = $true
      break
    } catch { }
  }
  if (-not $activated) {
    # Fallback: click the center of the item's bounding rectangle
    # (mirrors FlaUI Click()).
    $r = $nav.Current.BoundingRectangle
    if ($r.Width -le 0 -or $r.Height -le 0) { throw 'Controls nav item has an empty bounding rectangle.' }
    [void][DevTem.Win32]::SetCursorPos([int]($r.X + $r.Width / 2), [int]($r.Y + $r.Height / 2))
    Start-Sleep -Milliseconds 200
    [void][DevTem.Win32]::SetForegroundWindow($proc.MainWindowHandle)
    [DevTem.Win32]::mouse_event(0x0002, 0, 0, 0, [System.UIntPtr]::Zero)  # LEFTDOWN
    [DevTem.Win32]::mouse_event(0x0004, 0, 0, 0, [System.UIntPtr]::Zero)  # LEFTUP
    $activated = $true
  }
  Start-Sleep -Seconds 2
  Capture-Window (Join-Path $imgDir 'gallery-controls.png')
}
finally {
  if (-not $proc.HasExited) { $proc.Kill(); $proc.WaitForExit(5000) | Out-Null }
}

$os = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion'
Write-Host "PROVENANCE os=$($os.CurrentBuildNumber).$($os.UBR) display=$([System.Windows.Forms.Screen]::PrimaryScreen.Bounds.Size)"
