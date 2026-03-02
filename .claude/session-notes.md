# Session Notes
<!-- Written by /wrapup. Read by /catchup at the start of the next session. -->
<!-- Overwritten each session — history preserved in git log of this file. -->

- **Date:** 2026-03-02
- **Branch:** main

## What Was Done
- Diagnosed user-reported crash: plugin exit code 0xC0000005 (access violation) with no plugin log
- Root cause: missing .NET 8.0 Desktop Runtime on user's machine — plugin is framework-dependent
- Made plugin **self-contained** (`SelfContained=true`) so .NET runtime is bundled
- Switched all build/deploy/CI scripts from `dotnet build` to `dotnet publish -o`
- Bumped version to 1.0.3
- Updated CLAUDE.md, README.md, CHANGELOG.md with new build model
- Built release package: 36.3 MB compressed (up from ~5 MB)
- Deployed and verified: plugin runs, no crashes, all 3 Stream Deck devices detected

## Decisions Made
- Self-contained over framework-dependent: eliminates #1 user support issue at the cost of ~31 MB package size increase
- No trimming: BarRaider StreamDeck-Tools uses reflection heavily, trimming would be risky
- Used explicit `-o` flag with `dotnet publish` to keep output paths consistent with existing scripts

## Open Items
- [ ] `src/MozaHotkey.App/` directory has 110 stale build artifact files (no csproj) — safe to delete
- [ ] No automated tests exist yet
- [ ] Consider submitting to Stream Deck Marketplace

## Next Steps
1. Create v1.0.3 release on GitHub
2. Clean up stale `src/MozaHotkey.App/` directory
3. Consider adding a troubleshooting note about the crash for other users
