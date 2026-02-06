# Generate placeholder icons for Stream Deck plugin
# Creates simple 72x72 PNG icons with text labels at the top
# Also generates Up/Down variants with +/- indicators and color tinting

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
        [string]$TextColor = "#FFFFFF",
        [switch]$GenerateVariants
    )

    $size = 72

    # Create base icon
    CreateIcon -Name $Name -Label $Label -BackgroundColor $BackgroundColor -TextColor $TextColor

    # Create Up/Down variants if requested
    if ($GenerateVariants) {
        # Up variant: + symbol below label
        CreateIcon -Name "${Name}Up" -Label $Label -BackgroundColor $BackgroundColor -TextColor $TextColor -Indicator "+"

        # Down variant: - symbol below label
        CreateIcon -Name "${Name}Down" -Label $Label -BackgroundColor $BackgroundColor -TextColor $TextColor -Indicator "-"
    }
}

function BlendColors {
    param(
        [string]$BaseColor,
        [string]$TintColor,
        [double]$Amount
    )

    $base = [System.Drawing.ColorTranslator]::FromHtml($BaseColor)
    $tint = [System.Drawing.ColorTranslator]::FromHtml($TintColor)

    $r = [int]($base.R * (1 - $Amount) + $tint.R * $Amount)
    $g = [int]($base.G * (1 - $Amount) + $tint.G * $Amount)
    $b = [int]($base.B * (1 - $Amount) + $tint.B * $Amount)

    # Clamp values
    $r = [Math]::Min(255, [Math]::Max(0, $r))
    $g = [Math]::Min(255, [Math]::Max(0, $g))
    $b = [Math]::Min(255, [Math]::Max(0, $b))

    return "#{0:X2}{1:X2}{2:X2}" -f $r, $g, $b
}

function CreateIcon {
    param(
        [string]$Name,
        [string]$Label,
        [string]$BackgroundColor,
        [string]$TextColor,
        [string]$Indicator = $null
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

    # Draw label text - fixed font size for consistency
    $textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml($TextColor))
    $fontSize = 8
    $font = New-Object System.Drawing.Font("Segoe UI", $fontSize, [System.Drawing.FontStyle]::Bold)

    $stringFormat = New-Object System.Drawing.StringFormat
    $stringFormat.Alignment = [System.Drawing.StringAlignment]::Center
    $stringFormat.LineAlignment = [System.Drawing.StringAlignment]::Near  # Top alignment

    # Rectangle with padding from top
    $topPadding = if ($Indicator) { 4 } else { 8 }
    $rect = New-Object System.Drawing.RectangleF(0, $topPadding, $size, $size)
    $graphics.DrawString($Label, $font, $textBrush, $rect, $stringFormat)

    # Draw indicator (+/-) centered below the label if specified
    if ($Indicator) {
        # Larger font for visibility
        $indicatorFont = New-Object System.Drawing.Font("Segoe UI", 16, [System.Drawing.FontStyle]::Bold)

        # Same color as label (white)
        $indicatorBrush = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml($TextColor))

        # Center horizontally, position below the label
        $indicatorFormat = New-Object System.Drawing.StringFormat
        $indicatorFormat.Alignment = [System.Drawing.StringAlignment]::Center
        $indicatorFormat.LineAlignment = [System.Drawing.StringAlignment]::Near

        # Fixed position - centered vertically below label text
        $indicatorY = 28
        $indicatorRect = New-Object System.Drawing.RectangleF(0, $indicatorY, $size, $size)
        $graphics.DrawString($Indicator, $indicatorFont, $indicatorBrush, $indicatorRect, $indicatorFormat)

        $indicatorFont.Dispose()
        $indicatorBrush.Dispose()
        $indicatorFormat.Dispose()
    }

    # Save
    $filePath = Join-Path $imagesPath "$Name.png"
    $bitmap.Save($filePath, [System.Drawing.Imaging.ImageFormat]::Png)

    # Cleanup
    $font.Dispose()
    $textBrush.Dispose()
    $bgBrush.Dispose()
    $stringFormat.Dispose()
    $graphics.Dispose()
    $bitmap.Dispose()

    Write-Host "Created: $Name.png" -ForegroundColor Green
}

Write-Host "Generating placeholder icons..." -ForegroundColor Cyan
Write-Host ""

# Plugin and category icons (no variants needed)
New-PlaceholderIcon -Name "pluginIcon" -Label "MOZA" -BackgroundColor "#E31837"
New-PlaceholderIcon -Name "categoryIcon" -Label "MOZA" -BackgroundColor "#E31837"

# Action icons with Up/Down variants for adjustable settings
New-PlaceholderIcon -Name "ffbIcon" -Label "FFB`nStrength" -BackgroundColor "#4A90D9" -GenerateVariants
New-PlaceholderIcon -Name "rotationIcon" -Label "Wheel`nRotation" -BackgroundColor "#7B68EE" -GenerateVariants
New-PlaceholderIcon -Name "dampingIcon" -Label "Natural`nDamping" -BackgroundColor "#20B2AA" -GenerateVariants
New-PlaceholderIcon -Name "torqueIcon" -Label "Max`nTorque" -BackgroundColor "#FF6347" -GenerateVariants
New-PlaceholderIcon -Name "swInertiaIcon" -Label "Wheel`nInertia" -BackgroundColor "#6A5ACD" -GenerateVariants
New-PlaceholderIcon -Name "speedIcon" -Label "Max`nSpeed" -BackgroundColor "#FF8C00" -GenerateVariants
New-PlaceholderIcon -Name "frictionIcon" -Label "Natural`nFriction" -BackgroundColor "#CD853F" -GenerateVariants
New-PlaceholderIcon -Name "inertiaIcon" -Label "Natural`nInertia" -BackgroundColor "#8B4513" -GenerateVariants
New-PlaceholderIcon -Name "springIcon" -Label "Spring`nStrength" -BackgroundColor "#32CD32" -GenerateVariants
New-PlaceholderIcon -Name "roadIcon" -Label "Road`nSensitivity" -BackgroundColor "#4682B4" -GenerateVariants
New-PlaceholderIcon -Name "speedDampingIcon" -Label "Speed`nDamping" -BackgroundColor "#2E8B57" -GenerateVariants

# Action icons without variants (no direction setting)
New-PlaceholderIcon -Name "setRotationIcon" -Label "Set`nRotation" -BackgroundColor "#9370DB"
New-PlaceholderIcon -Name "centerIcon" -Label "Center`nWheel" -BackgroundColor "#FFB347"
New-PlaceholderIcon -Name "reverseIcon" -Label "FFB`nReverse" -BackgroundColor "#DC143C"
New-PlaceholderIcon -Name "stopIcon" -Label "STOP`nFFB" -BackgroundColor "#B22222"

# Pedal reverse toggles
New-PlaceholderIcon -Name "throttleIcon" -Label "Throttle`nReverse" -BackgroundColor "#2E8B57"
New-PlaceholderIcon -Name "brakeIcon" -Label "Brake`nReverse" -BackgroundColor "#CD5C5C"
New-PlaceholderIcon -Name "clutchIcon" -Label "Clutch`nReverse" -BackgroundColor "#708090"

# Handbrake mode toggle
New-PlaceholderIcon -Name "handbrakeIcon" -Label "E-Brake`nMode" -BackgroundColor "#8B0000"

# Auto-blip (shifter) actions
New-PlaceholderIcon -Name "blipToggleIcon" -Label "Auto`nBlip" -BackgroundColor "#B8860B"
New-PlaceholderIcon -Name "blipIcon" -Label "Blip`nOutput" -BackgroundColor "#DAA520" -GenerateVariants

# Preset (profile switcher)
New-PlaceholderIcon -Name "presetIcon" -Label "Apply`nPreset" -BackgroundColor "#6A0DAD"

Write-Host ""
Write-Host "Done! Icons created in: $imagesPath" -ForegroundColor Green
