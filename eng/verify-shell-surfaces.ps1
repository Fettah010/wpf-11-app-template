#Requires -Version 5.1
<#
.SYNOPSIS
  1.4.1 shell-surface invariant check (local release gate).
.DESCRIPTION
  Launches the reference app in the Tabbed shell, captures the window with
  PrintWindow (deterministic; CopyFromScreen can catch unpainted frames), and
  asserts theme-surface invariants that a screenshot baseline cannot express
  robustly:

    * dark theme: essentially no pure-white panel. The shipped 1.4.0 Tabbed
      shell rendered 67.9% pure-white pixels because WPF-UI's implicit
      TabControl style is not applied to the derived ui:TabView; the built-in
      WPF template paints a white content border. Fail above -MaxWhitePercent.
    * no bright #F0F0F0 vertical band (the default GridSplitter paint, ~1188
      px tall in 1.4.0). Fail above -MaxBrightBand.

  Exits non-zero on failure, so it can gate a release locally. Local-only:
  it needs an interactive desktop; CI keeps the advisory visual policy (D09r)
  and relies on TabbedShellSurfaceTests for the enforced guard.
.EXAMPLE
  powershell -ExecutionPolicy Bypass -File eng/verify-shell-surfaces.ps1 -Build
.EXAMPLE
  powershell -ExecutionPolicy Bypass -File eng/verify-shell-surfaces.ps1 -Theme 1
#>
param(
  [int]$Shell = 3,
  [int]$Theme = 2,
  [switch]$Windowed,
  [int]$Width = 0,
  [int]$Height = 0,
  [double]$MaxWhitePercent = 1.0,
  [int]$MaxBrightBand = 200,
  [int]$SettleSeconds = 8,
  [string]$OutDir = (Join-Path ([IO.Path]::GetTempPath()) 'devtem-surfaces'),
  [string]$SaveAs = '',
  [switch]$Build
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Add-Type -AssemblyName System.Drawing

if ($Build) {
  Write-Host 'Building DevTemWpf.App (Release)...'
  & dotnet build (Join-Path $root 'src/DevTemWpf.App') -c Release -v m
  if ($LASTEXITCODE -ne 0) { throw 'build failed' }
}

$sig = @'
[DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
[DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
[DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
[DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
[DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);
public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
'@
Add-Type -MemberDefinition $sig -Name Win32 -Namespace DevTemSurfaces -PassThru | Out-Null

$analyzer = @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace DevTemSurfaces
{
    public sealed class SurfaceReport
    {
        public int Width;
        public int Height;
        public double WhitePercent;
        public int BrightBandMax;
        public string DocSurfaceHex = "";
        public double DocSurfacePercent;
    }

    public static class SurfaceAnalyzer
    {
        public static SurfaceReport Analyze(string path)
        {
            Bitmap bmp = new Bitmap(path);
            int w = bmp.Width;
            int h = bmp.Height;
            byte[] a;
            int strideUsed;
            try
            {
                BitmapData d = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try
                {
                    strideUsed = Math.Abs(d.Stride);
                    a = new byte[strideUsed * h];
                    Marshal.Copy(d.Scan0, a, 0, a.Length);
                }
                finally { bmp.UnlockBits(d); }
            }
            finally { bmp.Dispose(); }

            long white = 0;
            var colBright = new int[w];
            var hist = new System.Collections.Generic.Dictionary<int, long>();
            int cx0 = w / 4, cx1 = (3 * w) / 4, cy0 = (2 * h) / 5, cy1 = (4 * h) / 5;

            for (int y = 0; y < h; y++)
            {
                int row = y * strideUsed;
                for (int x = 0; x < w; x++)
                {
                    int i = row + (x * 4);
                    byte b = a[i], g = a[i + 1], r = a[i + 2];
                    if (r == 255 && g == 255 && b == 255) white++;
                    if (r == 240 && g == 240 && b == 240) colBright[x]++;
                    if (x >= cx0 && x < cx1 && y >= cy0 && y < cy1)
                    {
                        int key = (r << 16) | (g << 8) | b;
                        long c;
                        hist.TryGetValue(key, out c);
                        hist[key] = c + 1;
                    }
                }
            }

            int bandMax = 0;
            foreach (int c in colBright) if (c > bandMax) bandMax = c;

            int topKey = 0; long topCount = 0;
            foreach (var kv in hist) if (kv.Value > topCount) { topCount = kv.Value; topKey = kv.Key; }

            var rep = new SurfaceReport();
            rep.Width = w; rep.Height = h;
            rep.WhitePercent = 100.0 * white / ((double)w * h);
            rep.BrightBandMax = bandMax;
            rep.DocSurfaceHex = string.Format("{0:X2}{1:X2}{2:X2}", (topKey >> 16) & 0xFF, (topKey >> 8) & 0xFF, topKey & 0xFF);
            long regionTotal = (long)(cx1 - cx0) * (cy1 - cy0);
            rep.DocSurfacePercent = regionTotal > 0 ? 100.0 * topCount / regionTotal : 0.0;
            return rep;
        }
    }
}
'@
Add-Type -TypeDefinition $analyzer -ReferencedAssemblies 'System.Drawing'

New-Item -ItemType Directory -Path $OutDir -Force | Out-Null

$settingsPath = Join-Path $env:APPDATA 'Wpf11AppTemplate\settings.json'
$backup = if (Test-Path $settingsPath) { Get-Content $settingsPath -Raw } else { $null }
$json = @{
  Theme = $Theme; BackdropEnabled = $true; Shell = $Shell; Density = 0; AccentOverride = $null
  Motion = 0; EffectsEnabled = $true; UpdateChannel = 0; ExpandedDataNodes = @()
  Workspace = @{ OpenDocuments = @('home','controls','settings'); SelectedDocument = 'settings'; DetailWidth = 280; IsDetailOpen = $true }
  MinimizeToTray = $false; Culture = ''
} | ConvertTo-Json -Depth 6
New-Item -ItemType Directory -Path (Split-Path $settingsPath) -Force | Out-Null
[IO.File]::WriteAllText($settingsPath, $json)

$exe = Join-Path $root 'src\DevTemWpf.App\bin\Release\net10.0-windows10.0.26100\DevTemWpf.App.exe'
if (-not (Test-Path $exe)) { throw "exe not found (build first or pass -Build): $exe" }

$tag = "sh$Shell-t$Theme"
$out = Join-Path $OutDir "$tag.png"
$proc = Start-Process -FilePath $exe -PassThru
try {
  $deadline = (Get-Date).AddSeconds(30)
  while ((Get-Date) -lt $deadline) {
    Start-Sleep -Milliseconds 400
    $proc.Refresh()
    if ($proc.HasExited) { throw "exited early code $($proc.ExitCode)" }
    if ($proc.MainWindowHandle -ne 0) { break }
  }
  $hwnd = $proc.MainWindowHandle
  Start-Sleep -Seconds $SettleSeconds
  [void][DevTemSurfaces.Win32]::SetForegroundWindow($hwnd)
  if ($Width -gt 0 -and $Height -gt 0) {
    [void][DevTemSurfaces.Win32]::SetWindowPos($hwnd, [IntPtr]::Zero, 40, 40, $Width, $Height, 0x0040)
    Start-Sleep -Seconds 2
  } elseif (-not $Windowed) {
    [void][DevTemSurfaces.Win32]::ShowWindow($hwnd, 3)
    Start-Sleep -Seconds 2
  }
  [void][DevTemSurfaces.Win32]::SetForegroundWindow($hwnd)
  Start-Sleep -Seconds 2

  $rect = New-Object DevTemSurfaces.Win32+RECT
  [void][DevTemSurfaces.Win32]::GetWindowRect($hwnd, [ref]$rect)
  $w = $rect.Right - $rect.Left; $h = $rect.Bottom - $rect.Top
  $bmp = New-Object System.Drawing.Bitmap($w, $h)
  $g = [System.Drawing.Graphics]::FromImage($bmp)
  $hdc = $g.GetHdc()
  [void][DevTemSurfaces.Win32]::PrintWindow($hwnd, $hdc, 2)
  $g.ReleaseHdc($hdc)
  $g.Dispose()
  $bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
  $bmp.Dispose()
} finally {
  if (-not $proc.HasExited) { $proc.Kill(); $proc.WaitForExit(5000) | Out-Null }
  if ($backup) { Set-Content -LiteralPath $settingsPath -Value $backup -Encoding UTF8 -NoNewline }
  else { Remove-Item -LiteralPath $settingsPath -Force -ErrorAction SilentlyContinue }
}

$rep = [DevTemSurfaces.SurfaceAnalyzer]::Analyze($out)
if ($SaveAs) {
  $saveDir = Split-Path -Parent $SaveAs
  if ($saveDir) { New-Item -ItemType Directory -Path $saveDir -Force | Out-Null }
  Copy-Item -LiteralPath $out -Destination $SaveAs -Force
  Write-Host ("saved plate: {0}" -f $SaveAs)
}
Write-Host ("SURFACE $tag {0}x{1}  white={2:N3}%  brightBandMax={3}  docSurface=#{4} ({5:N1}% of centre)" -f `
  $rep.Width, $rep.Height, $rep.WhitePercent, $rep.BrightBandMax, $rep.DocSurfaceHex, $rep.DocSurfacePercent)
Write-Host ("capture: {0}" -f $out)

$failures = @()
if ($Theme -eq 2 -and $rep.WhitePercent -gt $MaxWhitePercent) {
  $failures += "white surface ${($rep.WhitePercent)}% exceeds $MaxWhitePercent% (built-in TabControl template leak?)"
}
if ($rep.BrightBandMax -gt $MaxBrightBand) {
  $failures += "bright #F0F0F0 vertical band ${($rep.BrightBandMax)}px exceeds $MaxBrightBand (default GridSplitter paint?)"
}

if ($failures.Count -gt 0) {
  Write-Host "SHELL SURFACE CHECK: FAIL"
  $failures | ForEach-Object { Write-Host "  - $_" }
  exit 1
}
Write-Host 'SHELL SURFACE CHECK: PASS'
