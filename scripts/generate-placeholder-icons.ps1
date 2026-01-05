# Generate placeholder icons for Stream Deck plugin
# Creates simple 72x72 PNG icons with text labels at the top

Add-Type -AssemblyName System.Drawing

$imagesPath = Join-Path (Split-Path -Parent $PSScriptRoot) "src\MozaHotkey.StreamDeck\Images"

# Ensure directory exists
if (-not (Test-Path $imagesPath)) {
    New-Item -ItemType Directory -Path $imagesPath -Force | Out-Null
}

function New-PlaceholderIcon {
    param(
        [string]$Name,
        [string]$Label,
        [string]$BackgroundColor = "#2D2D2D",
        [string]$TextColor = "#FFFFFF"
    )

    $size = 72
    $bitmap = New-Object System.Drawing.Bitmap($size, $size)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)

    # Set quality
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit

    # Fill background
    $bgBrush = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml($BackgroundColor))
    $graphics.FillRectangle($bgBrush, 0, 0, $size, $size)

    # Draw text - smaller font, aligned to top
    $textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml($TextColor))
    # Reduced font sizes by 1 point
    $fontSize = if ($Label.Length -le 3) { 17 } elseif ($Label.Length -le 5) { 13 } else { 9 }
    $font = New-Object System.Drawing.Font("Segoe UI", $fontSize, [System.Drawing.FontStyle]::Bold)

    $stringFormat = New-Object System.Drawing.StringFormat
    $stringFormat.Alignment = [System.Drawing.StringAlignment]::Center
    $stringFormat.LineAlignment = [System.Drawing.StringAlignment]::Near  # Top alignment

    # Rectangle with padding from top (8px from top)
    $rect = New-Object System.Drawing.RectangleF(0, 8, $size, $size)
    $graphics.DrawString($Label, $font, $textBrush, $rect, $stringFormat)

    # Save
    $filePath = Join-Path $imagesPath "$Name.png"
    $bitmap.Save($filePath, [System.Drawing.Imaging.ImageFormat]::Png)

    # Cleanup
    $font.Dispose()
    $textBrush.Dispose()
    $bgBrush.Dispose()
    $graphics.Dispose()
    $bitmap.Dispose()

    Write-Host "Created: $Name.png" -ForegroundColor Green
}

Write-Host "Generating placeholder icons..." -ForegroundColor Cyan
Write-Host ""

# Plugin and category icons (keep centered for these)
New-PlaceholderIcon -Name "pluginIcon" -Label "MOZA" -BackgroundColor "#E31837"
New-PlaceholderIcon -Name "categoryIcon" -Label "MOZA" -BackgroundColor "#E31837"

# Action icons
New-PlaceholderIcon -Name "ffbIcon" -Label "FFB" -BackgroundColor "#4A90D9"
New-PlaceholderIcon -Name "rotationIcon" -Label "ROT" -BackgroundColor "#7B68EE"
New-PlaceholderIcon -Name "setRotationIcon" -Label "SET" -BackgroundColor "#9370DB"
New-PlaceholderIcon -Name "dampingIcon" -Label "DAMP" -BackgroundColor "#20B2AA"
New-PlaceholderIcon -Name "torqueIcon" -Label "TRQ" -BackgroundColor "#FF6347"
New-PlaceholderIcon -Name "centerIcon" -Label "CTR" -BackgroundColor "#FFB347"

# New action icons
New-PlaceholderIcon -Name "swInertiaIcon" -Label "SW IN" -BackgroundColor "#6A5ACD"
New-PlaceholderIcon -Name "speedIcon" -Label "SPEED" -BackgroundColor "#FF8C00"
New-PlaceholderIcon -Name "frictionIcon" -Label "FRIC" -BackgroundColor "#CD853F"
New-PlaceholderIcon -Name "inertiaIcon" -Label "INERT" -BackgroundColor "#8B4513"
New-PlaceholderIcon -Name "springIcon" -Label "SPRNG" -BackgroundColor "#32CD32"

Write-Host ""
Write-Host "Done! Icons created in: $imagesPath" -ForegroundColor Green
