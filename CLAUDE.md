# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

moza-streamdeck-plugin is a Stream Deck plugin for Moza Racing wheel bases. It allows users to adjust FFB strength, wheel rotation, and other settings directly from Stream Deck buttons and dials (Stream Deck+) while in their sim racing games.

## Build Commands

```bash
# Build the solution
dotnet build MozaStreamDeckPlugin.sln

# Build for release
dotnet build MozaStreamDeckPlugin.sln --configuration Release

# Clean build artifacts
dotnet clean MozaStreamDeckPlugin.sln
```

## Architecture

```
moza-streamdeck-plugin/
├── src/
│   ├── MozaStreamDeck.Core/       # Core library (SDK wrapper, preset handling)
│   │   ├── MozaDevice.cs          # Wrapper around Moza SDK
│   │   └── Profiles/              # Pit House preset handling
│   │       ├── PresetProfile.cs   # Model + JSON parser for motor presets
│   │       └── PresetManager.cs   # Find/enumerate Pit House presets
│   │
│   └── MozaStreamDeck.Plugin/     # Stream Deck plugin
│       ├── manifest.json          # Plugin metadata and action definitions
│       ├── Program.cs             # Entry point with BarRaider StreamDeck-Tools
│       ├── MozaDeviceManager.cs   # Singleton device lifecycle
│       ├── Actions/               # Stream Deck action handlers (one per setting)
│       ├── Layouts/               # Stream Deck+ encoder layout JSON
│       ├── PropertyInspector/     # HTML/JS settings UI for each action
│       └── Images/                # Icon PNG files (72x72)
│
├── scripts/
│   ├── deploy-streamdeck.ps1      # Deploy plugin to Stream Deck
│   ├── build-streamdeck-release.ps1      # Build .streamDeckPlugin release package
│   ├── build-streamdeck-marketplace.ps1  # Build for marketplace submission
│   ├── list-sdk-methods.ps1       # List available Moza SDK methods
│   └── generate-placeholder-icons.ps1    # Generate placeholder icons
│
├── .github/
│   └── workflows/
│       └── release.yml            # GitHub Actions: automated release on tag push
│
└── lib/
    └── MozaSDK/                   # Moza SDK DLLs (included in repo)
        └── Licenses/              # Third-party license files
```

## Key SDK Functions

| Function | Description | Range |
|----------|-------------|-------|
| `setMotorFfbStrength(int)` | Set FFB strength | 0-100 |
| `setMotorLimitAngle(int, int)` | Set wheel rotation (hardware, game) | 90-2700 |
| `setMotorNaturalDamper(int)` | Set natural damping | 0-100 |
| `setMotorSpringStrength(int)` | Set center spring strength | 0-100 |
| `setMotorPeakTorque(int)` | Set max torque limit | 50-100 |
| `setMotorNaturalFriction(int)` | Set natural friction | 0-100 |
| `setMotorNaturalInertia(int)` | Set natural inertia | 100-500 |
| `setMotorNaturalInertiaRatio(int)` | Set steering wheel inertia (weight simulation) | 100-1550 |
| `setMotorLimitWheelSpeed(int)` | Set maximum wheel speed | 0-100 |
| `setMotorRoadSensitivity(int)` | Set road sensitivity/feel | 0-10 |
| `setMotorSpeedDamping(int)` | Set speed-dependent damping | 0-100 |
| `setMotorFfbReverse(int)` | Set FFB direction (0=normal, 1=reversed) | 0-1 |
| `stopForceFeedback()` | Emergency stop all force feedback | N/A |
| `CenterWheel()` | Center the steering wheel | N/A |
| `setPedalAccOutDir(int)` | Set throttle pedal direction (0=normal, 1=reversed) | 0-1 |
| `setPedalBrakeOutDir(int)` | Set brake pedal direction (0=normal, 1=reversed) | 0-1 |
| `setPedalClutchOutDir(int)` | Set clutch pedal direction (0=normal, 1=reversed) | 0-1 |
| `setHandbrakeApplicationMode(int)` | Set handbrake mode (0=axis, 1=button) | 0-1 |
| `setHandingShifterAutoBlipSwitch(int)` | Set auto-blip on/off (0=off, 1=on) | 0-1 |
| `setHandingShifterAutoBlipOutput(int)` | Set auto-blip throttle amount | 0-100 |
| `setHandingShifterAutoBlipDuration(int)` | Set auto-blip duration | 0-500 |
| `setMotorSpeedDampingStartPoint(int)` | Set speed damping activation point | 0-200 |
| `setMotorHandsOffProtection(int)` | Set hands-off protection mode | 0-2 |

All getter functions use `ref ERRORCODE` parameter.

## Platform Requirements

- Windows only (Stream Deck software and Moza SDK are Windows-only)
- .NET 8.0
- x64 architecture (Moza SDK is x64 only)
- Moza Pit House must be installed for the SDK to communicate with hardware

## Settings Location

Action settings are managed by BarRaider's StreamDeck-Tools and stored automatically by the Stream Deck software per-action instance. There is no separate settings file.

## Stream Deck Plugin

The plugin provides direct control of Moza wheel settings from Stream Deck buttons and dials (Stream Deck+).

### Build and Deploy

```powershell
# Deploy to Stream Deck (closes/reopens Stream Deck)
.\scripts\deploy-streamdeck.ps1 -KillStreamDeck

# Build only (without deploying)
dotnet build src/MozaStreamDeck.Plugin

# Generate placeholder icons
.\scripts\generate-placeholder-icons.ps1
```

### Plugin Location

Installed to: `%APPDATA%\Elgato\StreamDeck\Plugins\com.dbce.moza-streamdeck.sdPlugin\`

### Available Actions

| Action | Description | Default Increment | Controllers |
|--------|-------------|-------------------|-------------|
| FFB Strength | Adjust Force Feedback strength | 5 | Button, Dial |
| Wheel Rotation | Adjust wheel rotation angle | 90 | Button, Dial |
| Set Rotation | Set specific rotation value | N/A | Button only |
| Natural Damping | Adjust natural damping | 5 | Button, Dial |
| Max Torque | Adjust maximum torque limit | 5 | Button, Dial |
| Steering Wheel Inertia | Adjust wheel weight simulation (100-1550g) | 50 | Button, Dial |
| Maximum Wheel Speed | Adjust max wheel rotation speed | 5 | Button, Dial |
| Natural Friction | Adjust natural friction | 5 | Button, Dial |
| Natural Inertia | Adjust natural inertia | 5 | Button, Dial |
| Wheel Spring Strength | Adjust center spring strength | 5 | Button, Dial |
| Road Sensitivity | Adjust road feel detail (0-10) | 1 | Button, Dial |
| Speed Damping | Adjust speed-dependent damping | 5 | Button, Dial |
| FFB Reverse | Toggle FFB direction | N/A | Button only |
| Stop FFB | Emergency stop all force feedback | N/A | Button only |
| Center Wheel | Center the steering wheel | N/A | Button only |
| Throttle Reverse | Toggle throttle pedal direction | N/A | Button only |
| Brake Reverse | Toggle brake pedal direction | N/A | Button only |
| Clutch Reverse | Toggle clutch pedal direction | N/A | Button only |
| Handbrake Mode | Toggle handbrake axis/button mode | N/A | Button only |
| Auto-Blip Toggle | Toggle automatic rev-match on downshift | N/A | Button only |
| Auto-Blip Output | Adjust auto-blip throttle amount | 5 | Button, Dial |
| Auto-Blip Duration | Adjust auto-blip duration (0-500ms) | 50 | Button, Dial |
| Apply Preset | Apply a Pit House motor preset | N/A | Button only |

### Per-Action Settings

Each action has its own configurable settings in the Property Inspector:
- **Direction** (buttons only): Increase or Decrease
- **Increment Value**: How much to change per press/tick

For dials, direction is determined by rotation direction; only increment is configurable.

### Display Layout

- Icon label (e.g., "FFB", "DAMP") is baked into the icon at the top
- Current value (e.g., "75%", "540°") displays dynamically at the bottom
- Dials show value on the LCD touchscreen with indicator bar

### Icon Requirements

Icons are 72x72 PNG files in `src/MozaStreamDeck.Plugin/Images/`:
- pluginIcon.png, categoryIcon.png, marketplaceIcon.png (Moza branding)
- ffbIcon.png, rotationIcon.png, setRotationIcon.png
- dampingIcon.png, torqueIcon.png, centerIcon.png
- swInertiaIcon.png, speedIcon.png, frictionIcon.png
- inertiaIcon.png, springIcon.png, roadIcon.png
- speedDampingIcon.png, reverseIcon.png, stopIcon.png
- throttleIcon.png, brakeIcon.png, clutchIcon.png, handbrakeIcon.png
- blipToggleIcon.png, blipIcon.png (+ Up/Down variants), presetIcon.png
- settingsIcon.png (generic/utility)

Adjustable actions also have Up/Down variants (e.g., ffbIconUp.png, ffbIconDown.png) for direction indication.

### Action Implementation Pattern

Each action extends `KeyAndEncoderBase` and implements:
- `InitializeDisplay()`: Reads current value from device, updates button/dial display
- `OnTick()`: Retries initialization if device wasn't ready at startup
- `KeyPressed()`: Handles button press (applies direction setting)
- `DialRotate()`: Handles dial rotation (uses tick count * increment)
- `DialDown()`: Refreshes current value display when dial is pressed
- `ReceivedSettings()`: Updates settings from Property Inspector

The `_initialized` flag ensures display updates retry until the Moza device is connected.

### Pit House Preset Integration

Motor presets are loaded from `%USERPROFILE%\Documents\MOZA Pit House\Presets\Motor\*.json`.
The `PresetManager` finds the Pit House directory (handles OneDrive redirection) and enumerates presets.
The `ApplyPresetAction` reads the selected preset JSON and applies all supported `deviceParams` via SDK.

**Pit House preset directory structure:**
```
Documents/MOZA Pit House/
├── Presets/
│   ├── Motor/              ← Plugin scans HERE (wheel base FFB/rotation settings)
│   ├── Steering Wheel/     ← NOT scanned (button mappings, RPM LEDs — no SDK control)
│   ├── Pedals/             ← NOT scanned (pedal curves — no SDK control)
│   ├── config.ini          ← Maps device models to default preset IDs
│   └── favorites.json      ← User's favorited preset IDs
└── LocalParameters/        ← Per-device active settings (not presets)
```

Only `Motor/` presets contain `deviceParams` fields the plugin can apply via SDK.
Steering Wheel presets (e.g., "ESX-Official") contain LED/button config only.

Supported preset fields (11 of ~22):
- `gameForceFeedbackStrength`, `maximumSteeringAngle`, `maximumTorque`
- `mechanicalDamper`, `mechanicalSpringStrength`, `mechanicalFriction`
- `maximumSteeringSpeed`, `gameForceFeedbackReversal`, `speedDependentDamping`
- `initialSpeedDependentDamping`, `safeDrivingEnabled/Mode`

Not supported (no SDK function): `gameForceFeedbackFilter`, `setGameDampingValue`, `setGameFrictionValue`,
`setGameInertiaValue`, `setGameSpringValue`, `softLimitStrength`, `softLimitStiffness`, `constForceExtraMode`,
`forceFeedbackMaping`, `naturalInertiaV2`, `gearJoltLevel`

## MCP Tools (dbce-mcp-server)

The `dbce-mcp-server` provides tools globally via MCP. Key tools for this project:

### Image Generation (for Stream Deck icons)

- `generate_image` — Generate professional action icons via OpenAI gpt-image-1. Supports transparent backgrounds natively. Use for creating visually distinctive icons for each of the 25+ actions.
- `resize_image` — Resize generated icons to exact 72x72 pixels required by Stream Deck SDK.
- `remove_background` — Clean up icon backgrounds to work on Stream Deck's dark display.

### UI Verification

- `screenshot` — Capture the Stream Deck software window to verify how icons render in the actual plugin UI, Property Inspector layout, and dial indicator appearance.

### Tool Selection

- For icon creation workflow: `generate_image` (with transparent BG) → `resize_image` (to 72x72 PNG) → verify with `screenshot` of Stream Deck app
- Do NOT use Playwright MCP for this project — Stream Deck Property Inspectors are local HTML but managed by the SDK, not a regular browser
