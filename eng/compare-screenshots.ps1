#Requires -Version 5.1
<#
.SYNOPSIS
  Advisory visual-regression spike (1.2.0): tolerant screenshot comparison.
.DESCRIPTION
  Captures the reference app (via eng/capture-screenshots.ps1) and compares
  each page against committed baselines in docs/images/baselines/. Reports
  changed-pixel fractions; writes red-overlay diff PNGs for failures.

  ADVISORY in 1.2.0 (never required-red): rendering can vary with fonts and
  GPU anti-aliasing across machines. The required-gate decision is deferred
  to 1.3.0 with this spike as evidence. Baselines are NEVER auto-updated:
  copy a reviewed capture into docs/images/baselines/ deliberately.
  Zero new packages (System.Drawing is inbox on net10.0-windows).
.EXAMPLE
  powershell -ExecutionPolicy Bypass -File eng/compare-screenshots.ps1
#>
param(
  [string]$BaselineDir = 'docs/images/baselines',
  [string]$Pages = 'Home,Controls,Settings',
  [double]$TolerancePercent = 0.5,
  [int]$ChannelTolerance = 8,
  # Default mask: top window-border strip (1.4.0). Those pixels belong to DWM
  # chrome, not product UI (proven by inspected diffs: maximized-border paint
  # timing only); the app title text starts below. Documented, not hidden.
  [string]$Mask = '0,0,2576,12'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
if ([IO.Path]::IsPathRooted($BaselineDir)) { $baseDir = $BaselineDir }
else { $baseDir = Join-Path $root $BaselineDir }
$diffDir = Join-Path ([IO.Path]::GetTempPath()) ('devtem-vr-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $diffDir -Force | Out-Null
$freshDir = Join-Path $diffDir 'fresh'
New-Item -ItemType Directory -Path $freshDir -Force | Out-Null

Add-Type -AssemblyName System.Drawing

$comparerCode = @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public static class VrCompare
{
    public static double Compare(string basePath, string freshPath, int channelTolerance, string maskSpec, string diffPath)
    {
        Rectangle[] masks = ParseMasks(maskSpec);
        Bitmap bb = new Bitmap(basePath);
        Bitmap fb = new Bitmap(freshPath);
        try
        {
            int w = bb.Width;
            int h = bb.Height;
            if (fb.Width != w || fb.Height != h)
            {
                throw new InvalidOperationException("Size mismatch: baseline " + w + "x" + h +
                    " vs fresh " + fb.Width + "x" + fb.Height +
                    ". Re-baseline deliberately on the pinned environment; never auto-approve.");
            }
            Bitmap b = bb.Clone(new Rectangle(0, 0, w, h), PixelFormat.Format32bppArgb);
            Bitmap f = fb.Clone(new Rectangle(0, 0, w, h), PixelFormat.Format32bppArgb);
            Bitmap diff = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            try
            {
                BitmapData bd = b.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                BitmapData fd = f.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                BitmapData dd = diff.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                try
                {
                    int stride = Math.Abs(bd.Stride);
                    byte[] ba = new byte[stride * h];
                    byte[] fa = new byte[stride * h];
                    byte[] da = new byte[stride * h];
                    Marshal.Copy(bd.Scan0, ba, 0, ba.Length);
                    Marshal.Copy(fd.Scan0, fa, 0, fa.Length);
                    long changed = 0;
                    long total = 0;
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            bool masked = false;
                            for (int m = 0; m < masks.Length; m++)
                            {
                                if (masks[m].Contains(x, y)) { masked = true; break; }
                            }
                            int i = (y * stride) + (x * 4);
                            da[i] = ba[i];
                            da[i + 1] = ba[i + 1];
                            da[i + 2] = ba[i + 2];
                            da[i + 3] = 255;
                            if (masked) { continue; }
                            total = total + 1;
                            bool same = true;
                            for (int c = 0; c < 3; c++)
                            {
                                if (Math.Abs((int)ba[i + c] - (int)fa[i + c]) > channelTolerance) { same = false; break; }
                            }
                            if (same) { continue; }
                            changed = changed + 1;
                            da[i] = 0;
                            da[i + 1] = 0;
                            da[i + 2] = 255;
                            da[i + 3] = 255;
                        }
                    }
                    Marshal.Copy(da, 0, dd.Scan0, da.Length);
                    diff.Save(diffPath, ImageFormat.Png);
                    if (total == 0) { return 0.0; }
                    return 100.0 * (double)changed / (double)total;
                }
                finally
                {
                    b.UnlockBits(bd);
                    f.UnlockBits(fd);
                    diff.UnlockBits(dd);
                }
            }
            finally
            {
                b.Dispose();
                f.Dispose();
                diff.Dispose();
            }
        }
        finally
        {
            bb.Dispose();
            fb.Dispose();
        }
    }

    private static Rectangle[] ParseMasks(string spec)
    {
        if (spec == null || spec.Trim().Length == 0) { return new Rectangle[0]; }
        string[] parts = spec.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        Rectangle[] rects = new Rectangle[parts.Length];
        for (int k = 0; k < parts.Length; k++)
        {
            string[] n = parts[k].Split(',');
            rects[k] = new Rectangle(int.Parse(n[0]), int.Parse(n[1]), int.Parse(n[2]), int.Parse(n[3]));
        }
        return rects;
    }
}
'@

Add-Type -TypeDefinition $comparerCode -ReferencedAssemblies 'System.Drawing'

Write-Host "Capturing fresh screenshots for: $Pages"
& (Join-Path $root 'eng/capture-screenshots.ps1') -Pages $Pages -OutDir $freshDir | Out-Null

$failures = 0
$pageNames = $Pages.Split(',') | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne '' }
foreach ($page in $pageNames)
{
  $slug = $page.ToLowerInvariant()
  $base = Join-Path $baseDir "gallery-$slug.png"
  $fresh = Join-Path $freshDir "gallery-$slug.png"
  if (-not (Test-Path -LiteralPath $base))
  {
    Write-Host "VR $page : NO BASELINE - capture and human-review first, then commit deliberately."
    $failures = $failures + 1
    continue
  }
  $diffPath = Join-Path $diffDir "diff-$slug.png"
  $pct = [math]::Round([VrCompare]::Compare($base, $fresh, $ChannelTolerance, $Mask, $diffPath), 3)
  if ($pct -le $TolerancePercent)
  {
    Write-Host "VR $page : changed=$pct% tolerance=$TolerancePercent% PASS"
    Remove-Item -LiteralPath $diffPath -Force -ErrorAction SilentlyContinue
  }
  else
  {
    Write-Host "VR $page : changed=$pct% tolerance=$TolerancePercent% FAIL (diff: $diffPath)"
    $failures = $failures + 1
  }
}

if ($failures -gt 0)
{
  Write-Host "VISUAL REGRESSION: $failures page(s) differ - see diffs under $diffDir (advisory in 1.2.0; never auto-approve)."
  exit 1
}

Write-Host 'VISUAL REGRESSION CLEAN (advisory)'
Remove-Item -Recurse -Force $diffDir
