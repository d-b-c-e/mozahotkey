# Session Notes
<!-- Written by /wrapup. Read by /catchup at the start of the next session. -->
<!-- Overwritten each session — history preserved in git log of this file. -->

- **Date:** 2026-02-21
- **Branch:** main

## What Was Done
- Deployed new plugin (`com.dbce.moza-streamdeck`) to Stream Deck after project rename
- Removed stale old plugin (`com.mozahotkey.streamdeck.sdPlugin`) — required killing orphaned `MozaStreamDeck.Plugin` process that was locking files
- Investigated `_Arcade` preset rotation label not updating after Apply Preset
- Investigated missing `ESX-Official` preset — found it's a Steering Wheel preset in `Presets/Steering Wheel/`, not a Motor preset
- Updated CLAUDE.md: added missing scripts to architecture, documented Pit House preset directory structure, added missing icons

## Decisions Made
- Steering Wheel presets (like `ESX-Official`) are intentionally not shown in plugin: they contain LED/button config only, no motor/FFB `deviceParams` the SDK can control
- Pit House preset directory structure documented in CLAUDE.md for future reference

## Open Items
- [ ] `_Arcade` preset rotation label not updating — likely `setMotorLimitAngle(135, 135)` failing on R12 at that low angle (similar to hands-off protection OUTOFRANGE issue). Need to test with wheel connected and check Apply Preset button for "skip" count
- [ ] No automated tests exist yet — unit tests for `PresetManager`, `PresetProfile` parsing, and value clamping would add confidence
- [ ] Old `MozaHotkey.App` build artifacts remain in `src/MozaHotkey.App/bin/` and `obj/` — should be cleaned up or gitignored

## Next Steps
1. Test Apply Preset with `_Arcade` profile while wheel is connected — check if rotation setting reports a skip/error
2. If rotation fails at 135, add SDK error fallback for low rotation values (similar to hands-off protection fallback)
3. Consider polishing icons and preparing for Stream Deck Marketplace submission
4. Add unit tests for core preset parsing and value clamping logic

## Context for Next Session
The plugin was successfully redeployed under the new `com.dbce.moza-streamdeck` plugin ID. Any old Stream Deck button configs from the `com.mozahotkey.streamdeck` era need to be re-added manually. The rotation display issue with _Arcade preset is the top bug to investigate — code review showed no logic bug, so it's likely an SDK-level rejection of 135 degrees on the R12 wheel base.
