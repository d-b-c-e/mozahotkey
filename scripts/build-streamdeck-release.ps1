# Build Stream Deck Plugin Release Package
# Creates a distributable ZIP with an installer batch script

param(
    [string]$OutputDir = ".\releases"
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionDir = Split-Path -Parent $scriptDir
$projectDir = Join-Path $solutionDir "src\MozaHotkey.StreamDeck"
$csprojPath = Join-Path $projectDir "MozaHotkey.StreamDeck.csproj"

# Get version from csproj
[xml]$csproj = Get-Content $csprojPath
$version = $csproj.Project.PropertyGroup.Version
if (-not $version) { $version = "1.0.0" }

$pluginName = "com.mozahotkey.streamdeck.sdPlugin"
$releaseName = "MozaHotkey-StreamDeck-v$version"
$outputPath = Join-Path $solutionDir $OutputDir
$stagingPath = Join-Path $outputPath "staging\$releaseName"
$pluginStagingPath = Join-Path $stagingPath $pluginName

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building MozaHotkey Stream Deck Plugin" -ForegroundColor Cyan
Write-Host "Version: $version" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Clean and create staging directory
if (Test-Path $stagingPath) {
    Remove-Item -Path $stagingPath -Recurse -Force
}
New-Item -ItemType Directory -Path $pluginStagingPath -Force | Out-Null

# Build in Release mode
Write-Host "Building Release configuration..." -ForegroundColor Yellow
Push-Location $solutionDir
try {
    dotnet build $csprojPath -c Release
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed!" -ForegroundColor Red
        exit 1
    }
} finally {
    Pop-Location
}

Write-Host "Build succeeded!" -ForegroundColor Green
Write-Host ""

# Copy plugin files
Write-Host "Copying plugin files..." -ForegroundColor Yellow
$sourcePath = Join-Path $projectDir "bin\Release"
Copy-Item -Path "$sourcePath\*" -Destination $pluginStagingPath -Recurse

# Create install.bat
Write-Host "Creating installer script..." -ForegroundColor Yellow
$installBatContent = @'
@echo off
setlocal

echo ========================================
echo MozaHotkey Stream Deck Plugin Installer
echo ========================================
echo.

set "PLUGIN_NAME=com.mozahotkey.streamdeck.sdPlugin"
set "STREAMDECK_PLUGINS=%APPDATA%\Elgato\StreamDeck\Plugins"
set "TARGET_PATH=%STREAMDECK_PLUGINS%\%PLUGIN_NAME%"
set "SOURCE_PATH=%~dp0%PLUGIN_NAME%"

:: Check if Stream Deck plugins directory exists
if not exist "%STREAMDECK_PLUGINS%" (
    echo ERROR: Stream Deck plugins directory not found.
    echo Please ensure Stream Deck software is installed.
    echo Expected path: %STREAMDECK_PLUGINS%
    echo.
    pause
    exit /b 1
)

:: Check if source plugin exists
if not exist "%SOURCE_PATH%" (
    echo ERROR: Plugin files not found.
    echo Expected path: %SOURCE_PATH%
    echo.
    pause
    exit /b 1
)

:: Stop Stream Deck if running
echo Checking for running Stream Deck...
tasklist /FI "IMAGENAME eq StreamDeck.exe" 2>NUL | find /I /N "StreamDeck.exe">NUL
if "%ERRORLEVEL%"=="0" (
    echo Stopping Stream Deck...
    taskkill /IM StreamDeck.exe /F >NUL 2>&1
    timeout /t 3 /nobreak >NUL
)

:: Remove old plugin if exists
if exist "%TARGET_PATH%" (
    echo Removing previous installation...
    rmdir /S /Q "%TARGET_PATH%"
)

:: Copy new plugin
echo Installing plugin...
xcopy "%SOURCE_PATH%" "%TARGET_PATH%" /E /I /Q

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERROR: Failed to copy plugin files.
    pause
    exit /b 1
)

echo.
echo ========================================
echo Installation complete!
echo ========================================
echo.
echo Starting Stream Deck...

:: Start Stream Deck
start "" "%ProgramFiles%\Elgato\StreamDeck\StreamDeck.exe"

echo.
echo The plugin should now appear in Stream Deck under "Moza Racing"
echo.
pause
'@

$installBatPath = Join-Path $stagingPath "install.bat"
$installBatContent | Out-File -FilePath $installBatPath -Encoding ASCII

# Create uninstall.bat
$uninstallBatContent = @'
@echo off
setlocal

echo ========================================
echo MozaHotkey Stream Deck Plugin Uninstaller
echo ========================================
echo.

set "PLUGIN_NAME=com.mozahotkey.streamdeck.sdPlugin"
set "STREAMDECK_PLUGINS=%APPDATA%\Elgato\StreamDeck\Plugins"
set "TARGET_PATH=%STREAMDECK_PLUGINS%\%PLUGIN_NAME%"

:: Check if plugin is installed
if not exist "%TARGET_PATH%" (
    echo Plugin is not installed.
    echo.
    pause
    exit /b 0
)

:: Stop Stream Deck if running
echo Checking for running Stream Deck...
tasklist /FI "IMAGENAME eq StreamDeck.exe" 2>NUL | find /I /N "StreamDeck.exe">NUL
if "%ERRORLEVEL%"=="0" (
    echo Stopping Stream Deck...
    taskkill /IM StreamDeck.exe /F >NUL 2>&1
    timeout /t 3 /nobreak >NUL
)

:: Remove plugin
echo Removing plugin...
rmdir /S /Q "%TARGET_PATH%"

echo.
echo ========================================
echo Uninstallation complete!
echo ========================================
echo.
echo Starting Stream Deck...
start "" "%ProgramFiles%\Elgato\StreamDeck\StreamDeck.exe"
echo.
pause
'@

$uninstallBatPath = Join-Path $stagingPath "uninstall.bat"
$uninstallBatContent | Out-File -FilePath $uninstallBatPath -Encoding ASCII

# Create README.txt
$readmeContent = @"
MozaHotkey Stream Deck Plugin v$version
=====================================

Control your Moza Racing wheel base settings directly from Stream Deck!

REQUIREMENTS
------------
- Windows 10/11 (x64)
- Elgato Stream Deck software 6.0+
- Moza Pit House installed (required for SDK communication)
- Moza Racing wheel base connected

INSTALLATION
------------
1. Close Stream Deck software if running
2. Double-click 'install.bat'
3. Stream Deck will restart automatically
4. Find "Moza Racing" in the action categories

UNINSTALLATION
--------------
Double-click 'uninstall.bat'

USAGE
-----
- Drag actions to Stream Deck buttons or dials
- Configure direction (increase/decrease) and increment values
- For Stream Deck+ dials: rotate to adjust, press to refresh display

TROUBLESHOOTING
---------------
- "N/C" on buttons: Wheel base not detected. Ensure Moza Pit House can see your device.
- Values not updating: Press dial or restart Stream Deck

For more information, visit:
https://github.com/d-b-c-e/mozahotkey

"@

$readmePath = Join-Path $stagingPath "README.txt"
$readmeContent | Out-File -FilePath $readmePath -Encoding UTF8

# Create ZIP file
Write-Host "Creating ZIP package..." -ForegroundColor Yellow
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
Write-Host "Release package created successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Output: $zipPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "Package contents:" -ForegroundColor Yellow
Write-Host "  - $pluginName/     (plugin files)"
Write-Host "  - install.bat      (installer)"
Write-Host "  - uninstall.bat    (uninstaller)"
Write-Host "  - README.txt       (instructions)"
