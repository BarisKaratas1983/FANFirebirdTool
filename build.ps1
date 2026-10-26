<#
.SYNOPSIS
    Builds FAN Firebird Tool in Release mode for x86 and x64 and creates the release zips.

.DESCRIPTION
    For each platform:
      1. Rebuilds the project with MSBuild (the CopyEngines target copies engines\<platform>).
      2. Copies only the files that belong in a release into dist\FANFirebirdTool-<version>-<platform>.
      3. Zips that folder and writes a SHA-256 checksum file.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File build.ps1
#>
param(
    [string[]] $Platforms = @('x64', 'x86')
)

$ErrorActionPreference = 'Stop'
$root    = $PSScriptRoot
$project = Join-Path $root 'FANFirebirdTool\FANFirebirdTool.csproj'
$dist    = Join-Path $root 'dist'

# --- Find MSBuild through vswhere (installed with every Visual Studio) ---
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path $vswhere)) { throw "vswhere.exe not found. Is Visual Studio installed?" }
$msbuild = & $vswhere -latest -prerelease -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (-not $msbuild) { throw "MSBuild.exe not found." }
Write-Host "MSBuild: $msbuild" -ForegroundColor DarkGray

Add-Type -AssemblyName System.IO.Compression.FileSystem

if (Test-Path $dist) { Remove-Item $dist -Recurse -Force }
New-Item $dist -ItemType Directory | Out-Null

foreach ($platform in $Platforms) {
    $engines = Join-Path $root "engines\$platform"
    if (-not (Test-Path $engines)) { throw "Engine folder missing: $engines" }

    Write-Host "`n=== Building $platform ===" -ForegroundColor Cyan
    & $msbuild $project /t:Rebuild /p:Configuration=Release /p:Platform=$platform /m /v:minimal /nologo
    if ($LASTEXITCODE -ne 0) { throw "Build failed for $platform." }

    $out = Join-Path $root "FANFirebirdTool\bin\$platform\Release"
    $exe = Join-Path $out 'FANFirebirdTool.exe'

    # Version comes from the exe itself (AssemblyFileVersion), e.g. 1.0.0.0 -> 1.0.0
    $v = [Version](Get-Item $exe).VersionInfo.FileVersion
    $version = "$($v.Major).$($v.Minor).$($v.Build)"

    $name  = "FANFirebirdTool-$version-$platform"
    $stage = Join-Path $dist $name
    New-Item $stage -ItemType Directory | Out-Null

    # Only release files: no .pdb, no settings.ini left over from test runs
    Copy-Item $exe $stage
    Copy-Item (Join-Path $out 'FANFirebirdTool.exe.config') $stage
    Copy-Item (Join-Path $out 'engines') $stage -Recurse

    $lic = Join-Path $stage 'licenses'
    New-Item (Join-Path $lic 'Firebird') -ItemType Directory -Force | Out-Null
    Copy-Item (Join-Path $root 'LICENSE') (Join-Path $lic 'LICENSE.txt')
    Copy-Item (Join-Path $root 'licenses\*') $lic
    Copy-Item (Join-Path $engines 'fb50\IPLicense.txt')  (Join-Path $lic 'Firebird')
    Copy-Item (Join-Path $engines 'fb50\IDPLicense.txt') (Join-Path $lic 'Firebird')

    $zip = Join-Path $dist "$name.zip"
    [IO.Compression.ZipFile]::CreateFromDirectory($stage, $zip, [IO.Compression.CompressionLevel]::Optimal, $true)

    $hash = (Get-FileHash $zip -Algorithm SHA256).Hash.ToLowerInvariant()
    Add-Content (Join-Path $dist 'SHA256SUMS.txt') "$hash  $name.zip"

    $mb = [Math]::Round((Get-Item $zip).Length / 1MB, 1)
    Write-Host "Created $name.zip ($mb MB)" -ForegroundColor Green
}

Write-Host "`nDone. Output: $dist" -ForegroundColor Green
