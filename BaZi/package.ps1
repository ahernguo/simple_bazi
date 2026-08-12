[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$projectPath = Join-Path -Path $PSScriptRoot -ChildPath 'BaZi.csproj'
$targetFramework = 'net10.0-windows10.0.19041.0'

if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
    throw "Project file not found: $projectPath"
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'dotnet was not found on PATH.'
}

if (-not (Get-Command 7z -ErrorAction SilentlyContinue)) {
    throw '7z was not found on PATH.'
}

[xml] $project = Get-Content -LiteralPath $projectPath -Raw
$applicationDisplayVersion = [string] $project.Project.PropertyGroup.ApplicationDisplayVersion

if ([string]::IsNullOrWhiteSpace($applicationDisplayVersion)) {
    throw 'ApplicationDisplayVersion was not found in BaZi.csproj.'
}

$invalidFileNameChars = [System.IO.Path]::GetInvalidFileNameChars()
if ($applicationDisplayVersion.IndexOfAny($invalidFileNameChars) -ge 0) {
    throw "ApplicationDisplayVersion contains invalid filename characters: $applicationDisplayVersion"
}

$rootName = "BaZi_v$applicationDisplayVersion"
$outputArchiveName = "${rootName}_Windows_x64.7z"
$publishDirectory = Join-Path -Path $PSScriptRoot -ChildPath $rootName
$outputArchivePath = Join-Path -Path $PSScriptRoot -ChildPath $outputArchiveName

Write-Host "Application version: $applicationDisplayVersion"
Write-Host '[1/3] Publishing project (net10.0-windows)...'

$publishArguments = @(
    'publish'
    $projectPath
    '-p:TargetFrameworks=net10.0-windows10.0.19041.0'
    '-f'
    $targetFramework
    '-c'
    'Release'
    '-r'
    'win-x64'
    '--self-contained'
    'true'
    '-p:PublishTrimmed=false'
    '-p:WindowsPackageType=None'
    '-o'
    $publishDirectory
)

& dotnet @publishArguments
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

Write-Host ''
Write-Host '[2/3] Cleaning up unnecessary language folders...'

$keptDirectories = @(
    'zh-TW'
    'wwwroot'
    'Includes'
    'Configurations'
    'Microsoft.UI.Xaml'
    'NpuDetect'
)

Get-ChildItem -LiteralPath $publishDirectory -Directory | ForEach-Object {
    if ($_.Name -notin $keptDirectories) {
        Remove-Item -LiteralPath $_.FullName -Recurse -Force
    }
}

Write-Host ''
Write-Host '[3/3] Compressing with 7-Zip (Ultra settings)...'

if (Test-Path -LiteralPath $outputArchivePath -PathType Leaf) {
    Remove-Item -LiteralPath $outputArchivePath -Force
}

$archiveArguments = @(
    'a'
    '-t7z'
    '-mx=9'
    '-ms=on'
    '-xr!*.pdb'
    $outputArchivePath
    ".\$rootName"
)

Push-Location -LiteralPath $PSScriptRoot
try {
    & 7z @archiveArguments
    if ($LASTEXITCODE -ne 0) {
        throw "7-Zip compression failed with exit code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}

Write-Host ''
Write-Host 'Cleaning up temporary publish folder...'
Remove-Item -LiteralPath $publishDirectory -Recurse -Force

Write-Host ''
Write-Host '========================================'
Write-Host "Done! Archive created: $outputArchiveName" -ForegroundColor Green
Write-Host '========================================'
