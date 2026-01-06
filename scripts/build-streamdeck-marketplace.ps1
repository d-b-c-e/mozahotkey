# Build Stream Deck Plugin for Marketplace Distribution
# Creates a .streamDeckPlugin file ready for Elgato Marketplace submission

param(
    [string]$OutputDir = ".\releases"
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionDir = Split-Path -Parent $scriptDir
$projectDir = Join-Path $solutionDir "src\MozaHotkey.StreamDeck"
$csprojPath = Join-Path $projectDir "MozaHotkey.StreamDeck.csproj"
$manifestPath = Join-Path $projectDir "bin\Release\manifest.json"
$imagesPath = Join-Path $projectDir "Images"

# Get version from csproj
[xml]$csproj = Get-Content $csprojPath
$version = $csproj.Project.PropertyGroup.Version
if (-not $version) { $version = "1.0.0" }

$pluginId = "com.mozahotkey.streamdeck"
$pluginName = "$pluginId.sdPlugin"
$outputPath = Join-Path $solutionDir $OutputDir
$stagingPath = Join-Path $outputPath "marketplace-staging"
$pluginStagingPath = Join-Path $stagingPath $pluginName

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building for Stream Deck Marketplace" -ForegroundColor Cyan
Write-Host "Version: $version" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Clean and create staging directory
if (Test-Path $stagingPath) {
    Remove-Item -Path $stagingPath -Recurse -Force
}
New-Item -ItemType Directory -Path $pluginStagingPath -Force | Out-Null

# Build in Release mode
Write-Host "Step 1: Building Release configuration..." -ForegroundColor Yellow
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

# Generate 288x288 marketplace icon
Write-Host "Step 2: Generating 288x288 marketplace icon..." -ForegroundColor Yellow
Add-Type -AssemblyName System.Drawing

$marketplaceIconPath = Join-Path $imagesPath "marketplaceIcon.png"
$size = 288
$bitmap = New-Object System.Drawing.Bitmap($size, $size)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)

# Set quality
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic

# Fill background with Moza red
$bgBrush = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml("#E31837"))
$graphics.FillRectangle($bgBrush, 0, 0, $size, $size)

# Draw "MOZA" text
$textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
$font = New-Object System.Drawing.Font("Segoe UI", 48, [System.Drawing.FontStyle]::Bold)

$stringFormat = New-Object System.Drawing.StringFormat
$stringFormat.Alignment = [System.Drawing.StringAlignment]::Center
$stringFormat.LineAlignment = [System.Drawing.StringAlignment]::Center

$rect = New-Object System.Drawing.RectangleF(0, -20, $size, $size)
$graphics.DrawString("MOZA", $font, $textBrush, $rect, $stringFormat)

# Draw "Hotkey" subtitle
$subtitleFont = New-Object System.Drawing.Font("Segoe UI", 28, [System.Drawing.FontStyle]::Regular)
$subtitleRect = New-Object System.Drawing.RectangleF(0, 50, $size, $size)
$graphics.DrawString("Hotkey", $subtitleFont, $textBrush, $subtitleRect, $stringFormat)

# Save
$bitmap.Save($marketplaceIconPath, [System.Drawing.Imaging.ImageFormat]::Png)

# Cleanup
$font.Dispose()
$subtitleFont.Dispose()
$textBrush.Dispose()
$bgBrush.Dispose()
$stringFormat.Dispose()
$graphics.Dispose()
$bitmap.Dispose()

Write-Host "Created: marketplaceIcon.png (288x288)" -ForegroundColor Green
Write-Host ""

# Copy plugin files
Write-Host "Step 3: Copying plugin files..." -ForegroundColor Yellow
$sourcePath = Join-Path $projectDir "bin\Release"
Copy-Item -Path "$sourcePath\*" -Destination $pluginStagingPath -Recurse

# Also copy marketplace icon to plugin
Copy-Item -Path $marketplaceIconPath -Destination (Join-Path $pluginStagingPath "Images\marketplaceIcon.png")
Write-Host "Plugin files copied" -ForegroundColor Green
Write-Host ""

# Validate manifest
Write-Host "Step 4: Validating manifest.json..." -ForegroundColor Yellow
$manifest = Get-Content (Join-Path $pluginStagingPath "manifest.json") | ConvertFrom-Json

$validationErrors = @()
$validationWarnings = @()

# Check required fields
if (-not $manifest.Name) { $validationErrors += "Missing 'Name' field" }
if (-not $manifest.Version) { $validationErrors += "Missing 'Version' field" }
if (-not $manifest.Author) { $validationErrors += "Missing 'Author' field" }
if (-not $manifest.Description) { $validationErrors += "Missing 'Description' field" }
if (-not $manifest.Icon) { $validationErrors += "Missing 'Icon' field" }
if (-not $manifest.CodePath) { $validationErrors += "Missing 'CodePath' field" }
if (-not $manifest.Actions -or $manifest.Actions.Count -eq 0) { $validationErrors += "No actions defined" }

# Check SDK version
if ($manifest.SDKVersion -lt 2) {
    $validationWarnings += "SDKVersion should be 2 or higher for marketplace (current: $($manifest.SDKVersion))"
}

# Check URL
if ($manifest.URL -match "yourusername") {
    $validationWarnings += "URL contains placeholder 'yourusername' - update with actual repo URL"
}

# Check that icon files exist
$iconPath = Join-Path $pluginStagingPath "$($manifest.Icon).png"
if (-not (Test-Path $iconPath)) {
    $validationErrors += "Plugin icon not found: $($manifest.Icon).png"
}

# Report validation results
if ($validationErrors.Count -gt 0) {
    Write-Host "Validation ERRORS:" -ForegroundColor Red
    foreach ($err in $validationErrors) {
        Write-Host "  - $err" -ForegroundColor Red
    }
    Write-Host ""
}

if ($validationWarnings.Count -gt 0) {
    Write-Host "Validation WARNINGS:" -ForegroundColor Yellow
    foreach ($warn in $validationWarnings) {
        Write-Host "  - $warn" -ForegroundColor Yellow
    }
    Write-Host ""
}

if ($validationErrors.Count -eq 0) {
    Write-Host "Manifest validation passed!" -ForegroundColor Green
} else {
    Write-Host "Please fix errors before submitting to marketplace" -ForegroundColor Red
}
Write-Host ""

# Create .streamDeckPlugin file (ZIP with different extension)
Write-Host "Step 5: Creating .streamDeckPlugin package..." -ForegroundColor Yellow

$pluginFilePath = Join-Path $outputPath "com.mozahotkey.streamdeck-v$version.streamDeckPlugin"

if (Test-Path $pluginFilePath) {
    Remove-Item $pluginFilePath -Force
}

# Create ZIP then rename
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
Write-Host "Marketplace Package Ready!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Output file:" -ForegroundColor Cyan
Write-Host "  $pluginFilePath" -ForegroundColor White
Write-Host ""
Write-Host "File can be:" -ForegroundColor Yellow
Write-Host "  1. Double-clicked to install locally for testing"
Write-Host "  2. Submitted to Elgato Marketplace for distribution"
Write-Host ""

if ($validationWarnings.Count -gt 0) {
    Write-Host "Before marketplace submission, address warnings above." -ForegroundColor Yellow
    Write-Host ""
}

Write-Host "Marketplace submission checklist:" -ForegroundColor Cyan
Write-Host "  [ ] Update manifest.json URL to actual GitHub repo"
Write-Host "  [ ] Create 1-3 preview screenshots (1920x1080 recommended)"
Write-Host "  [ ] Prepare plugin description for marketplace listing"
Write-Host "  [ ] Test .streamDeckPlugin file installs correctly"
Write-Host "  [ ] Submit at: https://marketplace.elgato.com"
Write-Host ""
