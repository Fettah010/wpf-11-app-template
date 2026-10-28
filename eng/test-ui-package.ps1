$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$library = Join-Path $repoRoot "src\DevTemWpf.UI\DevTemWpf.UI.csproj"
$canarySource = Join-Path $repoRoot "experiments\1.8.0\ui-package-consumer"
$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "DevTemWpf-UiPackage-$([guid]::NewGuid().ToString('N'))"
$packageRoot = Join-Path $tempRoot "packages"
$consumerRoot = Join-Path $tempRoot "consumer"
$consumerProject = Join-Path $consumerRoot "UiPackageConsumer.csproj"
$completed = $false

function Invoke-DotNet([string[]] $Arguments) {
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

try {
    New-Item -ItemType Directory -Path $packageRoot, $consumerRoot -Force | Out-Null
    Invoke-DotNet @("pack", $library, "--configuration", "Release", "--output", $packageRoot, "-p:TreatWarningsAsErrors=true")
    Copy-Item -Path (Join-Path $canarySource "*") -Destination $consumerRoot -Recurse -Force
    $escapedPackageRoot = [System.Security.SecurityElement]::Escape($packageRoot)
    $nugetConfig = Join-Path $tempRoot "NuGet.Config"
    @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="UiPackageCanary" value="$escapedPackageRoot" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@ | Set-Content -LiteralPath $nugetConfig -Encoding utf8
    Invoke-DotNet @("restore", $consumerProject, "--configfile", $nugetConfig)
    Invoke-DotNet @("build", $consumerProject, "--configuration", "Release", "--no-restore", "--warnaserror")
    Invoke-DotNet @("run", "--project", $consumerProject, "--configuration", "Release", "--no-build")
    $completed = $true
    Write-Host "Package consumer built and launched successfully."
}
finally {
    if ($completed) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force
    }
    else {
        Write-Host "Canary artifacts preserved at $tempRoot"
    }
}
