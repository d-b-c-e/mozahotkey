# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/).

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

[0.8.3-alpha]: https://github.com/d-b-c-e/moza-streamdeck-plugin/compare/v0.8.2-alpha...v0.8.3-alpha
[0.8.2-alpha]: https://github.com/d-b-c-e/moza-streamdeck-plugin/compare/v0.8.1-alpha...v0.8.2-alpha
[0.8.1-alpha]: https://github.com/d-b-c-e/moza-streamdeck-plugin/compare/v0.8.0-alpha...v0.8.1-alpha
[0.8.0-alpha]: https://github.com/d-b-c-e/moza-streamdeck-plugin/releases/tag/v0.8.0-alpha
