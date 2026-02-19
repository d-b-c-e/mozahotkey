# Build Stream Deck Plugin Release Package
# Creates a .streamDeckPlugin file for distribution

param(
    [string]$OutputDir = ".\releases"
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionDir = Split-Path -Parent $scriptDir
$projectDir = Join-Path $solutionDir "src\MozaStreamDeck.Plugin"
$csprojPath = Join-Path $projectDir "MozaStreamDeck.Plugin.csproj"
$imagesPath = Join-Path $projectDir "Images"

# Get version from csproj
[xml]$csproj = Get-Content $csprojPath
$version = $csproj.Project.PropertyGroup.Version
if (-not $version) { $version = "1.0.0" }

$pluginId = "com.dbce.moza-streamdeck"
$pluginName = "$pluginId.sdPlugin"
$outputPath = Join-Path $solutionDir $OutputDir
$stagingPath = Join-Path $outputPath "staging"
$pluginStagingPath = Join-Path $stagingPath $pluginName

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building Moza Stream Deck Plugin" -ForegroundColor Cyan
Write-Host "Version: $version" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Create output directory
if (-not (Test-Path $outputPath)) {
    New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
}

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

# Generate 288x288 marketplace icon if needed
Write-Host "Generating marketplace icon..." -ForegroundColor Yellow
Add-Type -AssemblyName System.Drawing

$marketplaceIconPath = Join-Path $imagesPath "marketplaceIcon.png"
$size = 288
$bitmap = New-Object System.Drawing.Bitmap($size, $size)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)

$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic

$bgBrush = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml("#E31837"))
$graphics.FillRectangle($bgBrush, 0, 0, $size, $size)

$textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
$font = New-Object System.Drawing.Font("Segoe UI", 48, [System.Drawing.FontStyle]::Bold)

$stringFormat = New-Object System.Drawing.StringFormat
$stringFormat.Alignment = [System.Drawing.StringAlignment]::Center
$stringFormat.LineAlignment = [System.Drawing.StringAlignment]::Center

$rect = New-Object System.Drawing.RectangleF(0, -20, $size, $size)
$graphics.DrawString("MOZA", $font, $textBrush, $rect, $stringFormat)

$subtitleFont = New-Object System.Drawing.Font("Segoe UI", 28, [System.Drawing.FontStyle]::Regular)
$subtitleRect = New-Object System.Drawing.RectangleF(0, 50, $size, $size)
$graphics.DrawString("Racing", $subtitleFont, $textBrush, $subtitleRect, $stringFormat)

$bitmap.Save($marketplaceIconPath, [System.Drawing.Imaging.ImageFormat]::Png)

$font.Dispose()
$subtitleFont.Dispose()
$textBrush.Dispose()
$bgBrush.Dispose()
$stringFormat.Dispose()
$graphics.Dispose()
$bitmap.Dispose()

Write-Host "Marketplace icon ready" -ForegroundColor Green
Write-Host ""

# Copy plugin files
Write-Host "Copying plugin files..." -ForegroundColor Yellow
$sourcePath = Join-Path $projectDir "bin\Release"
Copy-Item -Path "$sourcePath\*" -Destination $pluginStagingPath -Recurse

# Also copy marketplace icon to plugin
Copy-Item -Path $marketplaceIconPath -Destination (Join-Path $pluginStagingPath "Images\marketplaceIcon.png") -Force
Write-Host "Plugin files copied" -ForegroundColor Green
Write-Host ""

# Create .streamDeckPlugin file (ZIP with different extension)
Write-Host "Creating .streamDeckPlugin package..." -ForegroundColor Yellow

$pluginFilePath = Join-Path $outputPath "$pluginId-v$version.streamDeckPlugin"

if (Test-Path $pluginFilePath) {
    Remove-Item $pluginFilePath -Force
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$tempZipPath = Join-Path $outputPath "temp-plugin.zip"

if (Test-Path $tempZipPath) {
    Remove-Item $tempZipPath -Force
}

# The .streamDeckPlugin is a ZIP of the .sdPlugin folder
[System.IO.Compression.ZipFile]::CreateFromDirectory($stagingPath, $tempZipPath)

# Rename to .streamDeckPlugin
Move-Item -Path $tempZipPath -Destination $pluginFilePath

Write-Host "Package created!" -ForegroundColor Green
Write-Host ""

# Clean up staging
Remove-Item -Path $stagingPath -Recurse -Force

# Summary
Write-Host "========================================" -ForegroundColor Green
Write-Host "Release Package Ready!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Output: $pluginFilePath" -ForegroundColor Cyan
Write-Host ""
Write-Host "To install:" -ForegroundColor Yellow
Write-Host "  Double-click the .streamDeckPlugin file"
Write-Host ""
