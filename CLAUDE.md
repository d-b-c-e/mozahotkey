# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MozaHotkey is a Windows utility that provides global hotkey support for Moza Racing wheel bases. It allows users to adjust FFB strength, wheel rotation, and other settings without alt-tabbing out of their sim racing games.

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
│       ├── Program.cs             # Entry point with SharpDeck
│       ├── MozaDeviceManager.cs   # Singleton device lifecycle
│       ├── PluginSettings.cs      # Global increment settings
│       ├── Actions/               # Stream Deck action handlers
│       ├── PropertyInspector/     # HTML settings UI
│       └── Images/                # Icon PNG files (72x72)
│
├── scripts/
│   └── deploy-streamdeck.ps1      # Deploy plugin to Stream Deck
│
└── lib/
    └── MozaSDK/                   # Moza SDK DLLs (not in repo, download separately)
```

## SDK Setup

The Moza SDK is required but not included in the repository. To set up:

1. Download the SDK from https://mozaracing.com/pages/sdk
2. Extract the following files to `lib/MozaSDK/`:
   - `SDK_CSharp/AnyCPU/MOZA_API_CSharp.dll`
   - `SDK_CSharp/x64/MOZA_SDK.dll`
   - `SDK_CSharp/x64/MOZA_API_C.dll`

## Key SDK Functions

| Function | Description | Range |
|----------|-------------|-------|
| `setMotorFfbStrength(int)` | Set FFB strength | 0-100 |
| `setMotorLimitAngle(int, int)` | Set wheel rotation (hardware, game) | 90-2700 |
| `setMotorRoadSensitivity(int)` | Set road feel sensitivity | 0-10 |
| `setMotorNaturalDamper(int)` | Set damping strength | 0-100 |
| `setMotorSpringStrength(int)` | Set center spring strength | 0-100 |
| `setMotorPeakTorque(int)` | Set max torque limit | 50-100 |

All getter functions use `ref ERRORCODE` parameter.

## Platform Requirements

- Windows only (uses WinForms and Windows API for global hotkeys)
- .NET 8.0
- x64 architecture (Moza SDK is x64 only)
- Moza Pit House must be installed for the SDK to communicate with hardware

## Settings Location

Settings are stored in: `%LOCALAPPDATA%\MozaHotkey\settings.json`

## Stream Deck Plugin

The Stream Deck plugin provides direct control of Moza wheel settings from Stream Deck buttons.

### Build and Deploy

```powershell
# Deploy to Stream Deck (closes/reopens Stream Deck)
.\scripts\deploy-streamdeck.ps1 -KillStreamDeck

# Build only (without deploying)
dotnet build src/MozaHotkey.StreamDeck
```

### Plugin Location

Installed to: `%APPDATA%\Elgato\StreamDeck\Plugins\com.mozahotkey.streamdeck.sdPlugin\`

### Available Actions

| Action | Description | Configurable |
|--------|-------------|--------------|
| FFB Strength | Increase/decrease FFB | Direction (+ or -) |
| Wheel Rotation | Increase/decrease rotation | Direction (+ or -) |
| Set Rotation | Set specific rotation | Degrees (270-1800) |
| Damping | Increase/decrease damping | Direction (+ or -) |
| Road Sensitivity | Adjust road feel | Direction (+ or -) |
| Max Torque | Adjust torque limit | Direction (+ or -) |
| Center Wheel | Centers the wheel | None |
| Settings | Configure increments | All increment values |

### Icon Requirements

Add PNG icons (72x72 or 144x144 for @2x) to `src/MozaHotkey.StreamDeck/Images/`:
- pluginIcon.png, categoryIcon.png
- ffbIcon.png, rotationIcon.png, setRotationIcon.png
- dampingIcon.png, roadIcon.png, torqueIcon.png
- centerIcon.png, settingsIcon.png
