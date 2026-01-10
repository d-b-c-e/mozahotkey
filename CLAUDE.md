# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MozaHotkey is a Windows utility that provides global hotkey support for Moza Racing wheel bases. It allows users to adjust FFB strength, wheel rotation, and other settings without alt-tabbing out of their sim racing games. It also includes a Stream Deck plugin for direct hardware control.

## Build Commands

```bash
# Build the solution
dotnet build

# Build for release
dotnet build --configuration Release

# Run the application
dotnet run --project src/MozaHotkey.App

# Clean build artifacts
dotnet clean
```

## Architecture

```
MozaHotkey/
├── src/
│   ├── MozaHotkey.Core/           # Core library (SDK wrapper, actions, settings)
│   │   ├── MozaDevice.cs          # Wrapper around Moza SDK
│   │   ├── Actions/               # Hotkey action definitions
│   │   │   ├── MozaAction.cs      # Base action class and implementations
│   │   │   └── ActionRegistry.cs  # Registry of all available actions
│   │   └── Settings/              # Configuration and persistence
│   │       ├── AppSettings.cs     # JSON-based settings storage
│   │       └── HotkeyBinding.cs   # Hotkey binding model
│   │
│   ├── MozaHotkey.App/            # WinForms application
│   │   ├── MainForm.cs            # Main window with action list
│   │   ├── HotkeyDialog.cs        # Hotkey capture dialog
│   │   ├── GlobalHotkeyManager.cs # Windows API hotkey registration
│   │   └── Program.cs             # Entry point with single-instance check
│   │
│   └── MozaHotkey.StreamDeck/     # Stream Deck plugin
│       ├── manifest.json          # Plugin metadata and action definitions
│       ├── Program.cs             # Entry point with BarRaider StreamDeck-Tools
│       ├── MozaDeviceManager.cs   # Singleton device lifecycle
│       ├── Actions/               # Stream Deck action handlers (one per setting)
│       ├── PropertyInspector/     # HTML/JS settings UI for each action
│       └── Images/                # Icon PNG files (72x72)
│
├── scripts/
│   ├── deploy-streamdeck.ps1      # Deploy plugin to Stream Deck
│   └── generate-placeholder-icons.ps1  # Generate placeholder icons
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
| `setMotorNaturalInertia(int)` | Set natural inertia | 0-100 |
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

All getter functions use `ref ERRORCODE` parameter.

## Platform Requirements

- Windows only (uses WinForms and Windows API for global hotkeys)
- .NET 8.0
- x64 architecture (Moza SDK is x64 only)
- Moza Pit House must be installed for the SDK to communicate with hardware

## Settings Location

Settings are stored in: `%LOCALAPPDATA%\MozaHotkey\settings.json`

## Stream Deck Plugin

The Stream Deck plugin provides direct control of Moza wheel settings from Stream Deck buttons and dials (Stream Deck+).

### Build and Deploy

```powershell
# Deploy to Stream Deck (closes/reopens Stream Deck)
.\scripts\deploy-streamdeck.ps1 -KillStreamDeck

# Build only (without deploying)
dotnet build src/MozaHotkey.StreamDeck

# Generate placeholder icons
.\scripts\generate-placeholder-icons.ps1
```

### Plugin Location

Installed to: `%APPDATA%\Elgato\StreamDeck\Plugins\com.mozahotkey.streamdeck.sdPlugin\`

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

Icons are 72x72 PNG files in `src/MozaHotkey.StreamDeck/Images/`:
- pluginIcon.png, categoryIcon.png (Moza branding)
- ffbIcon.png, rotationIcon.png, setRotationIcon.png
- dampingIcon.png, torqueIcon.png, centerIcon.png
- swInertiaIcon.png, speedIcon.png, frictionIcon.png
- inertiaIcon.png, springIcon.png, roadIcon.png
- speedDampingIcon.png, reverseIcon.png, stopIcon.png
- throttleIcon.png, brakeIcon.png, clutchIcon.png, handbrakeIcon.png

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
