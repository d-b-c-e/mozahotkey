# Build Windows App Release Package
# Creates either an Inno Setup installer or a ZIP package

param(
    [switch]$SkipInstaller,
    [string]$InnoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    [string]$OutputDir = ".\releases"
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionDir = Split-Path -Parent $scriptDir
$projectDir = Join-Path $solutionDir "src\MozaHotkey.App"
$csprojPath = Join-Path $projectDir "MozaHotkey.App.csproj"
$installerDir = Join-Path $solutionDir "installer"
$issPath = Join-Path $installerDir "MozaHotkey.iss"

# Get version from csproj
[xml]$csproj = Get-Content $csprojPath
$version = $csproj.Project.PropertyGroup.Version
if (-not $version) { $version = "1.0.0" }

$outputPath = Join-Path $solutionDir $OutputDir

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building MozaHotkey Windows App" -ForegroundColor Cyan
Write-Host "Version: $version" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Create output directory
if (-not (Test-Path $outputPath)) {
    New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
}

# Build in Release mode
Write-Host "Building Release configuration..." -ForegroundColor Yellow
Push-Location $solutionDir
try {
    dotnet publish $csprojPath -c Release -r win-x64 --self-contained false
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed!" -ForegroundColor Red
        exit 1
    }
} finally {
    Pop-Location
}

Write-Host "Build succeeded!" -ForegroundColor Green
Write-Host ""

# Check for Inno Setup
$hasInnoSetup = Test-Path $InnoSetupPath

if (-not $SkipInstaller -and $hasInnoSetup) {
    Write-Host "Creating installer with Inno Setup..." -ForegroundColor Yellow

    # Update version in .iss file
    $issContent = Get-Content $issPath -Raw
    $issContent = $issContent -replace '#define MyAppVersion ".*"', "#define MyAppVersion `"$version`""
    $issContent | Set-Content $issPath -NoNewline

    # Run Inno Setup compiler
    & $InnoSetupPath $issPath
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Inno Setup compilation failed!" -ForegroundColor Red
        exit 1
    }

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Installer created successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Output: $outputPath\MozaHotkey-Setup-v$version.exe" -ForegroundColor Cyan
}
elseif (-not $SkipInstaller -and -not $hasInnoSetup) {
    Write-Host "Inno Setup not found at: $InnoSetupPath" -ForegroundColor Yellow
    Write-Host "Creating ZIP package instead..." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To create an installer, install Inno Setup 6 from:" -ForegroundColor Cyan
    Write-Host "https://jrsoftware.org/isdl.php" -ForegroundColor Cyan
    Write-Host ""
    $SkipInstaller = $true
}

if ($SkipInstaller) {
    # Create ZIP package as fallback
    Write-Host "Creating ZIP package..." -ForegroundColor Yellow

    $releaseName = "MozaHotkey-App-v$version"
    $stagingPath = Join-Path $outputPath "staging\$releaseName"
    $publishPath = Join-Path $projectDir "bin\Release\net8.0-windows\win-x64\publish"

    # Clean and create staging directory
    if (Test-Path $stagingPath) {
        Remove-Item -Path $stagingPath -Recurse -Force
    }
    New-Item -ItemType Directory -Path $stagingPath -Force | Out-Null

    # Copy published files
    Copy-Item -Path "$publishPath\*" -Destination $stagingPath -Recurse

    # Create README
    $readmeContent = @"
MozaHotkey Windows Application v$version
========================================

Control your Moza Racing wheel base settings via global hotkeys!

REQUIREMENTS
------------
- Windows 10/11 (x64)
- .NET 8.0 Desktop Runtime
  Download: https://dotnet.microsoft.com/download/dotnet/8.0
- Moza Pit House installed (required for SDK communication)
- Moza Racing wheel base connected

INSTALLATION
------------
1. Extract all files to a folder (e.g., C:\Program Files\MozaHotkey)
2. Run MozaHotkey.App.exe
3. The app will appear in the system tray

USAGE
-----
- Right-click the tray icon to configure hotkeys
- Assign keyboard shortcuts to actions
- Use hotkeys while in-game to adjust settings

Settings are saved to: %LOCALAPPDATA%\MozaHotkey\settings.json

For more information, visit:
https://github.com/d-b-c-e/mozahotkey

"@
    $readmeContent | Out-File -FilePath (Join-Path $stagingPath "README.txt") -Encoding UTF8

    # Create ZIP
    $zipPath = Join-Path $outputPath "$releaseName.zip"
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory($stagingPath, $zipPath)

    # Clean up staging
    Remove-Item -Path (Join-Path $outputPath "staging") -Recurse -Force

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "ZIP package created successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Output: $zipPath" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Green
