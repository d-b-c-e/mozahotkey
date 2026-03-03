# Session Notes
<!-- Written by /wrapup. Read by /catchup at the start of the next session. -->
<!-- Overwritten each session — history preserved in git log of this file. -->

- **Date:** 2026-03-02
- **Branch:** main

## What Was Done
- Diagnosed user-reported crash: plugin exit code 0xC0000005 (access violation) with no plugin log
- Root cause: missing .NET 8.0 Desktop Runtime — plugin was framework-dependent
- Made plugin **self-contained** (`SelfContained=true` in `src/MozaStreamDeck.Plugin/MozaStreamDeck.Plugin.csproj`)
- Switched all build/deploy/CI scripts from `dotnet build` to `dotnet publish -o`
- Bumped version to 1.0.3, updated CLAUDE.md, README.md, CHANGELOG.md
- Created and published v1.0.3 release via GitHub Actions (36.3 MB compressed)
- Deployed locally and verified: plugin runs, connects to all 3 Stream Deck devices
- Cleaned up stale `src/MozaHotkey.App/` directory (110 files, 20.3 MB) and `.claude/screenshots/`

## Decisions Made
- Self-contained over framework-dependent: eliminates #1 user support issue at cost of ~31 MB package size increase
- No trimming: BarRaider StreamDeck-Tools uses reflection heavily, trimming would be risky
- Used explicit `-o` flag with `dotnet publish` to keep output paths consistent with existing scripts

## Open Items
- [ ] No automated tests exist yet
- [ ] Consider submitting to Stream Deck Marketplace
- [ ] User who reported crash should re-test with v1.0.3

## Next Steps
1. Monitor v1.0.3 feedback from the user who reported the crash
2. Consider adding more troubleshooting docs for common issues
3. Marketplace submission if feedback is positive

## Context for Next Session
v1.0.3 is live with self-contained .NET runtime. The crash was caused by missing .NET 8.0
Desktop Runtime — now bundled. All scripts use `dotnet publish` instead of `dotnet build`.
The stale `src/MozaHotkey.App/` directory was cleaned up. The repo is clean on main.
