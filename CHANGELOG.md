# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/).

## [1.0.4] - 2026-03-28

### Added
- **Hands Off Protection** toggle action — cycles through Off / Mode 1 / Mode 2 with automatic fallback for wheel bases that don't support Mode 2
- **Refresh Connection** action — tears down and re-establishes the SDK connection, then refreshes all button displays (fixes stale values after boot-order issues)
- Unit test project (`tests/MozaStreamDeck.Tests/`) with xUnit — 17 tests covering preset JSON parsing and Pit House directory discovery
- Troubleshooting guide (`docs/TROUBLESHOOTING.md`) covering common issues: crashes, N/C display, preset errors, Pit House auto-launch, and log file locations
- Diagnostic logging in Rotation action and ForceRefresh for easier troubleshooting

### Fixed
- **Wheel Rotation display not updating on Refresh** — the Moza SDK takes ~3.5 seconds to re-establish communication after reinitialize; Refresh now retries notifications over 5 seconds instead of a single immediate attempt
- Rotation display no longer briefly flashes "N/C" during refresh when the SDK returns transitional invalid values (0) — keeps the previous valid display until a fresh value is available
- Silent exception swallowing in Rotation action's display initialization — errors are now logged instead of hidden behind a bare `catch`

## [1.0.3] - 2026-03-02

### Changed
- Plugin is now **self-contained** — bundles the .NET 8.0 runtime so users no longer need to install it separately
- Build scripts switched from `dotnet build` to `dotnet publish` for self-contained output
- Release package size increased from ~5 MB to ~36 MB (compressed) due to bundled runtime

### Fixed
- Plugin crash on startup (exit code 0xC0000005) on systems without .NET 8.0 Desktop Runtime installed

## [1.0.2] - 2026-02-28

### Changed
- SDK initialization is now deferred until first user interaction (button press or dial rotation) — prevents Moza Pit House from auto-launching at boot when Stream Deck starts
- Actions show "N/C" until first interaction, then all displays refresh automatically

### Fixed
- Apply Preset: speed damping start point (initialSpeedDependentDamping) no longer fails with OUTOFRANGE when preset value is 0 — SDK rejects 0 so the call is now skipped for disabled speed damping

## [1.0.0] - 2026-02-21

### Changed
- Project renamed from MozaHotkey to moza-streamdeck-plugin
- New plugin ID: `com.dbce.moza-streamdeck` (replaces `com.mozahotkey.streamdeck`)
- Removed standalone WinForms hotkey application; project is now Stream Deck plugin only

## [0.8.3-alpha] - 2026-02-05

### Added
- Apply Preset action to load Moza Pit House motor presets directly from Stream Deck
- Auto-blip shifter actions: toggle on/off, output level (0-100%), and duration (0-500ms)
- Separate hardware and game steering angle limits when applying presets

### Fixed
- Apply Preset crash caused by `Tools.AutoPopulateSettings` failing on complex settings types
- Hands-off protection mapping now correctly enables protection when preset specifies it
- Hands-off protection falls back to level 1 if wheel base doesn't support higher levels (e.g., R12)
- Apply Preset no longer aborts on first setting failure; continues applying remaining settings
- Preset button shows green checkmark when most settings apply, only shows alert when all fail

### Changed
- Speed damping start point range widened from 0-100 to 0-200 to match Pit House preset values
- Apply Preset returns detailed per-setting error reporting instead of a simple count

## [0.8.2-alpha] - 2026-01-26

### Fixed
- Natural Inertia range corrected to 100-500% to match Pit House
- Stream Deck plugin no longer shows 0% on startup before Pit House loads

## [0.8.1-alpha] - 2026-01-09

### Added
- Pedal reverse actions: throttle, brake, and clutch direction toggle
- Handbrake mode toggle action (axis/button)
- Quick Start section in README for easier downloads

## [0.8.0-alpha] - 2026-01-09

### Added
- Initial public release
- Stream Deck plugin with button and Stream Deck+ dial support
- 15 adjustable settings actions with configurable increment and direction:
  FFB Strength, Wheel Rotation, Set Rotation, Natural Damping, Max Torque,
  Steering Wheel Inertia, Maximum Wheel Speed, Natural Friction, Natural Inertia,
  Wheel Spring Strength, Road Sensitivity, Speed Damping
- 3 toggle/trigger actions: FFB Reverse, Stop FFB, Center Wheel
- Real-time value display on buttons and dial LCD screens
- Dynamic direction icons (up/down variants) for button actions
- Custom encoder layout for Stream Deck+ dials
- Per-action settings via Property Inspector (direction, increment value)
- GitHub Actions workflow for automated releases
- Build and deploy scripts for development

[1.0.4]: https://github.com/d-b-c-e/moza-streamdeck-plugin/compare/v1.0.3...v1.0.4
[1.0.3]: https://github.com/d-b-c-e/moza-streamdeck-plugin/compare/v1.0.2...v1.0.3
[1.0.2]: https://github.com/d-b-c-e/moza-streamdeck-plugin/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/d-b-c-e/moza-streamdeck-plugin/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/d-b-c-e/moza-streamdeck-plugin/releases/tag/v1.0.0
[0.8.3-alpha]: https://github.com/d-b-c-e/moza-streamdeck-plugin/compare/v0.8.2-alpha...v0.8.3-alpha
[0.8.2-alpha]: https://github.com/d-b-c-e/moza-streamdeck-plugin/compare/v0.8.1-alpha...v0.8.2-alpha
[0.8.1-alpha]: https://github.com/d-b-c-e/moza-streamdeck-plugin/compare/v0.8.0-alpha...v0.8.1-alpha
[0.8.0-alpha]: https://github.com/d-b-c-e/moza-streamdeck-plugin/releases/tag/v0.8.0-alpha
