# MozaHotkey

A Windows utility for controlling Moza Racing wheel bases via global hotkeys and Stream Deck integration. Adjust FFB strength, wheel rotation, and other settings without alt-tabbing out of your sim racing games.

## Features

- **Global Hotkeys**: Bind keyboard shortcuts to adjust wheel settings from anywhere
- **Stream Deck Plugin**: Full support for Stream Deck buttons and Stream Deck+ dials
- **Real-time Display**: See current values on your Stream Deck buttons/dials
- **Per-Action Settings**: Each action has its own configurable increment value

### Supported Settings

| Setting | Range | Description |
|---------|-------|-------------|
| FFB Strength | 0-100% | Force feedback intensity |
| Wheel Rotation | 90-2700° | Steering lock angle |
| Natural Dampening | 0-100% | Simulated steering resistance |
| Max Torque | 50-100% | Maximum torque output limit |
| Steering Wheel Inertia | 100-1550g | Simulated wheel weight |
| Maximum Wheel Speed | 0-100% | Wheel rotation speed limit |
| Natural Friction | 0-100% | Static friction simulation |
| Natural Inertia | 0-100% | Rotational inertia simulation |
| Spring Strength | 0-100% | Center spring force |

## Requirements

- Windows 10/11 (x64)
- .NET 8.0 Runtime
- Moza Pit House installed (required for SDK communication)
- Moza Racing wheel base (R5, R9, R12, R16, R21, etc.)

### For Stream Deck Plugin

- Elgato Stream Deck software 6.0+
- Stream Deck, Stream Deck+, or compatible device

## Installation

### Building from Source

The Moza SDK is included in this repository, so no additional setup is required.

```bash
# Clone the repository
git clone https://github.com/yourusername/MozaHotkey.git
cd MozaHotkey

# Build the solution
dotnet build --configuration Release
```

### Stream Deck Plugin Installation

```powershell
# Deploy to Stream Deck (automatically restarts Stream Deck)
.\scripts\deploy-streamdeck.ps1 -KillStreamDeck
```

The plugin will be installed to:
`%APPDATA%\Elgato\StreamDeck\Plugins\com.mozahotkey.streamdeck.sdPlugin\`

## Usage

### Desktop Application

1. Run `MozaHotkey.App.exe`
2. The app minimizes to the system tray
3. Right-click the tray icon to configure hotkeys
4. Assign keyboard shortcuts to actions
5. Use hotkeys while in-game to adjust settings

Settings are saved to: `%LOCALAPPDATA%\MozaHotkey\settings.json`

### Stream Deck Plugin

1. Open Stream Deck software
2. Find "Moza Racing" in the action categories
3. Drag actions to your Stream Deck buttons or dials

**Button Configuration:**
- **Direction**: Choose Increase or Decrease
- **Increment**: Amount to change per press

**Dial Configuration (Stream Deck+):**
- **Increment**: Amount to change per rotation tick
- Rotate clockwise to increase, counter-clockwise to decrease
- Press dial to refresh current value display

**Display:**
- Button shows setting label (icon) at top, current value at bottom
- Dial LCD shows current value with indicator bar
- "N/C" indicates device not connected

## Troubleshooting

### Device Not Connected (N/C)

1. Ensure Moza Pit House is installed and can see your wheel base
2. Make sure the wheel base is powered on and connected via USB
3. Try restarting the Stream Deck software
4. Check that no other application is exclusively using the Moza SDK

### Values Not Updating

- The plugin retries connection automatically every second
- Press a dial or interact with a button to force a refresh
- Restart Stream Deck if issues persist

### Build Errors

- Ensure .NET 8.0 SDK is installed
- Run `dotnet restore` to restore NuGet packages

## Development

### Project Structure

```
MozaHotkey/
├── src/
│   ├── MozaHotkey.Core/        # Core library (SDK wrapper)
│   ├── MozaHotkey.App/         # WinForms hotkey application
│   └── MozaHotkey.StreamDeck/  # Stream Deck plugin
├── scripts/                     # Build and deployment scripts
└── lib/MozaSDK/                # Moza SDK (included)
```

### Adding a New Action

1. Add SDK wrapper methods to `MozaHotkey.Core/MozaDevice.cs`
2. Create action class in `MozaHotkey.StreamDeck/Actions/`
3. Add action definition to `manifest.json`
4. Create icon in `Images/` folder
5. Update Property Inspector if needed

See `CLAUDE.md` for detailed architecture documentation.

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test with actual Moza hardware if possible
5. Submit a pull request

## License

MIT License - see LICENSE file for details.

## Acknowledgments

- [Moza Racing / Gudsen Technology Co., Ltd](https://mozaracing.com/) for the SDK
- [BarRaider](https://github.com/BarRaider) for StreamDeck-Tools library
- The sim racing community for feedback and testing

## Third-Party Licenses

The Moza SDK (`lib/MozaSDK/`) is the property of Gudsen Technology Co., Ltd. The SDK includes components licensed under:
- OpenSSL License
- libcoap License
- nanaCbor License

See `lib/MozaSDK/Licenses/` for full license texts.

## Disclaimer

This project is not affiliated with, endorsed by, or sponsored by Gudsen Technology Co., Ltd (Moza Racing) or Elgato. The Moza SDK is included for convenience and remains the property of Gudsen Technology Co., Ltd. Use at your own risk.
