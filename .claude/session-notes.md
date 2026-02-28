# Session Notes
<!-- Written by /wrapup. Read by /catchup at the start of the next session. -->
<!-- Overwritten each session — history preserved in git log of this file. -->

- **Date:** 2026-02-28
- **Branch:** main

## What Was Done
- Researched recommended Moza wheel settings for arcade racing games (non-FFB, XOutput/x360ce scenarios)
- Updated `_Arcade` Pit House preset (`3e4d0e5e-0af8-4c84-8588-346af5669f3b`) with research-based values:
  - FFB 40→60, Rotation 135→360, Torque 100→75, Damper 25→50, Friction 20→30, Spring 0→40, Inertia 150→250, Speed Damping 25→0
- Fixed `MozaDevice.cs:SetSpeedDampingStartPoint` — SDK rejects 0 but Pit House presets commonly store 0 for "disabled"
- Applied fix in `ApplyPreset` to skip the SDK call when `initialSpeedDependentDamping` is 0
- Saved comprehensive arcade wheel settings research to persistent memory at `arcade-wheel-settings.md`
- Deployed and verified preset applies cleanly (11 settings, 0 skips)

## Decisions Made
- Spring strength 40% for arcade: essential for non-FFB games where the game provides no centering force
- Speed damping start point 0 = skip SDK call: the SDK function `setMotorSpeedDampingStartPoint` rejects 0 even though Pit House stores it — 0 means disabled, so skipping is correct behavior
- Kept clamp at 1-200 in `SetSpeedDampingStartPoint` as documentation of the SDK's actual accepted range

## Open Items
- [ ] Arcade preset is tuned but untested in actual games — user should test in Burnout/NFS/The Crew
- [ ] `_Arcade` preset was manually edited on disk — Pit House may overwrite if user edits via GUI
- [ ] No automated tests exist yet
- [ ] Consider submitting to Stream Deck Marketplace

## Next Steps
1. Test the `_Arcade` preset in actual arcade games and iterate on values
2. Consider creating game-specific presets (NFS, Forza Horizon, Burnout) using the research ranges
3. Bump version for next release with the speed damping start point fix

## Context for Next Session
The `_Arcade` preset was updated with community-researched values optimized for non-FFB arcade games
used via XOutput. Key insight: in non-FFB games, spring/damper/friction are the entire FFB system
since the game sends no forces. The speed damping start point fix (`src/MozaStreamDeck.Core/MozaDevice.cs:817-821`)
is the only code change — skip SDK call for value 0 instead of sending it and getting OUTOFRANGE.
Research notes saved to `.claude/projects/E--Source-moza-streamdeck-plugin/memory/arcade-wheel-settings.md`.
