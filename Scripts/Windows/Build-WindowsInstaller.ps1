#Requires -Version 5.1
<#
.SYNOPSIS
  Builds a Libation Windows setup.exe from a published bin directory using Inno Setup.

.DESCRIPTION
  Optionally runs dotnet publish (same projects as CI), removes standalone WindowsConfigApp
  artifacts, then compiles Scripts/Windows/Libation.iss.

.PARAMETER Version
  Libation version string (e.g. 13.4.1).

.PARAMETER Ui
  Avalonia (Chardonnay) or WinForms (Classic). Classic is x64 only.

.PARAMETER Architecture
  x64 or arm64 (Avalonia only).

.PARAMETER BinDir
  Folder containing published files. Defaults to repo-root bin.

.PARAMETER SkipPublish
  Skip dotnet publish; BinDir must already contain a complete publish output.

.EXAMPLE
  .\Build-WindowsInstaller.ps1 -Version 13.4.1 -Ui Avalonia -Architecture x64

.EXAMPLE
  .\Build-WindowsInstaller.ps1 -Version 13.4.1 -Ui WinForms
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $Version,

    [ValidateSet('Avalonia', 'WinForms')]
    [string] $Ui = 'Avalonia',

    [ValidateSet('x64', 'arm64')]
    [string] $Architecture = 'x64',

    [string] $BinDir = '',

    [switch] $SkipPublish
)

$ErrorActionPreference = 'Stop'

if ($Ui -eq 'WinForms' -and $Architecture -ne 'x64') {
    throw 'Classic (WinForms) is only published for x64.'
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$sourceDir = Join-Path $repoRoot 'Source'
if (-not $BinDir) {
    $BinDir = Join-Path $repoRoot 'bin'
}
if (-not (Test-Path $BinDir)) {
    New-Item -ItemType Directory -Path $BinDir -Force | Out-Null
}
$BinDir = (Resolve-Path $BinDir).Path

$runtime = "win-$Architecture"
$releaseName = if ($Ui -eq 'WinForms') { 'classic' } else { 'chardonnay' }
$outputBase = if ($Ui -eq 'WinForms') {
    "Libation-Classic.$Version-windows-$releaseName-$Architecture-setup"
} else {
    "Libation.$Version-windows-$releaseName-$Architecture-setup"
}
$architecturesAllowed = if ($Architecture -eq 'arm64') { 'arm64' } else { 'x64compatible' }

function Find-ISCC {
    $candidates = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
        "${env:LocalAppData}\Programs\Inno Setup 6\ISCC.exe"
    )
    foreach ($path in $candidates) {
        if (Test-Path $path) { return $path }
    }
    return $null
}

if (-not $SkipPublish -and (Test-Path $BinDir)) {
    Write-Host "Clearing $BinDir before publish ..."
    Get-ChildItem -LiteralPath $BinDir -Force | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
}

if (-not $SkipPublish) {
    Write-Host "Publishing Libation ($Ui, $runtime) to $BinDir ..."
    Push-Location $sourceDir
    try {
        $publishArgs = @(
            '--runtime', $runtime,
            '--configuration', 'Release',
            '--output', $BinDir,
            '-p:PublishProtocol=FileSystem',
            '-p:PublishReadyToRun=false',
            '-p:SelfContained=true'
        )
        & "C:\Program Files\dotnet\dotnet.exe" publish "Libation$Ui/Libation$Ui.csproj" @publishArgs
        & "C:\Program Files\dotnet\dotnet.exe" publish 'LoadByOS/WindowsConfigApp/WindowsConfigApp.csproj' @publishArgs
        & "C:\Program Files\dotnet\dotnet.exe" publish 'LibationCli/LibationCli.csproj' @publishArgs
        & "C:\Program Files\dotnet\dotnet.exe" publish "Hangover$Ui/Hangover$Ui.csproj" @publishArgs
    }
    finally {
        Pop-Location
    }
}

Write-Host 'Removing standalone WindowsConfigApp launcher artifacts ...'
$removeFiles = @(
    'WindowsConfigApp.exe',
    'WindowsConfigApp.runtimeconfig.json',
    'WindowsConfigApp.deps.json'
)
Push-Location $BinDir
try {
    foreach ($file in $removeFiles) {
        if (Test-Path $file) { Remove-Item $file -Force }
    }
    if (-not (Test-Path 'Libation.exe')) {
        throw "Libation.exe not found in $BinDir. Publish or point -BinDir at a complete output folder."
    }
    if (-not (Test-Path 'ZipExtractor.exe')) {
        throw "ZipExtractor.exe not found in $BinDir. It is required for in-app upgrades."
    }
}
finally {
    Pop-Location
}

$iscc = Find-ISCC
if (-not $iscc) {
    throw @"
Inno Setup 6 (ISCC.exe) was not found. Install from https://jrsoftware.org/isdl.php
and ensure ISCC.exe is under Program Files (x86)\Inno Setup 6\ or similar.
"@
}

$setupIcon = Join-Path $PSScriptRoot 'libation.ico'
if (-not (Test-Path $setupIcon)) {
    throw "Installer icon not found: $setupIcon"
}
$iss = Join-Path $PSScriptRoot 'Libation.iss'
$defines = @(
    "/DMyVersion=$Version",
    "/DMySetupIcon=$setupIcon",
    "/DMyReleaseName=$releaseName",
    "/DMyArchitecture=$Architecture",
    "/DOutputBaseFilename=$outputBase",
    "/DSourceDir=$BinDir",
    "/DOutputDir=$BinDir",
    "/DArchitecturesAllowed=$architecturesAllowed"
)

Write-Host "Compiling $outputBase.exe ..."
& $iscc $defines $iss
if ($LASTEXITCODE -ne 0) {
    throw "ISCC failed with exit code $LASTEXITCODE."
}

$setupPath = Join-Path $BinDir "$outputBase.exe"
Write-Host "Done: $setupPath"
