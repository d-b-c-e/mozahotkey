# Deploy Stream Deck Plugin Script
# This script builds and deploys the MozaHotkey Stream Deck plugin

param(
    [switch]$Release,
    [switch]$KillStreamDeck
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionDir = Split-Path -Parent $scriptDir
$projectDir = Join-Path $solutionDir "src\MozaHotkey.StreamDeck"
$pluginName = "com.mozahotkey.streamdeck.sdPlugin"
$streamDeckPluginsPath = Join-Path $env:APPDATA "Elgato\StreamDeck\Plugins"
$targetPath = Join-Path $streamDeckPluginsPath $pluginName

$config = if ($Release) { "Release" } else { "Debug" }

Write-Host "Building MozaHotkey.StreamDeck ($config)..." -ForegroundColor Cyan
dotnet build "$projectDir\MozaHotkey.StreamDeck.csproj" -c $config
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Kill Stream Deck if requested (required for file replacement)
if ($KillStreamDeck) {
    Write-Host "Stopping Stream Deck..." -ForegroundColor Yellow
    Get-Process -Name "StreamDeck" -ErrorAction SilentlyContinue | Stop-Process -Force
    Start-Sleep -Seconds 2
}

# Create plugins directory if it doesn't exist
if (-not (Test-Path $streamDeckPluginsPath)) {
    New-Item -ItemType Directory -Path $streamDeckPluginsPath -Force | Out-Null
}

# Remove old plugin if exists
if (Test-Path $targetPath) {
    Write-Host "Removing old plugin..." -ForegroundColor Yellow
    Remove-Item -Path $targetPath -Recurse -Force
}

# Copy new plugin
$sourcePath = Join-Path $projectDir "bin\$config"
Write-Host "Deploying to $targetPath..." -ForegroundColor Cyan
Copy-Item -Path $sourcePath -Destination $targetPath -Recurse

Write-Host ""
Write-Host "Plugin deployed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Add icon PNG files to the Images folder (72x72 or 144x144 for @2x)"
Write-Host "2. Restart Stream Deck software"
Write-Host "3. Find 'Moza Racing' category in the action list"
Write-Host ""
Write-Host "Required icon files:" -ForegroundColor Cyan
Write-Host "  - Images/pluginIcon.png (72x72)"
Write-Host "  - Images/categoryIcon.png (72x72)"
Write-Host "  - Images/ffbIcon.png (72x72)"
Write-Host "  - Images/rotationIcon.png (72x72)"
Write-Host "  - Images/setRotationIcon.png (72x72)"
Write-Host "  - Images/dampingIcon.png (72x72)"
Write-Host "  - Images/roadIcon.png (72x72)"
Write-Host "  - Images/torqueIcon.png (72x72)"
Write-Host "  - Images/centerIcon.png (72x72)"
Write-Host "  - Images/settingsIcon.png (72x72)"

if ($KillStreamDeck) {
    Write-Host ""
    Write-Host "Starting Stream Deck..." -ForegroundColor Yellow
    Start-Process (Join-Path $env:ProgramFiles "Elgato\StreamDeck\StreamDeck.exe")
}
