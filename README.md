# RingOfPainModKit 0.1.0

Unofficial, experimental C# mod framework for the locally verified Windows x64 Ring of Pain **1.6.02f2**, Unity **2019.4.29f1**, IL2CPP installation. Not affiliated with Twice Different or Humble Games.

## What works and what is still being tested

- **Verified live:** MelonLoader 0.7.3 loads a basic plugin; the framework checks four original file hashes and hooks `GameStateControl.DoAwake()`.
- **Verified live:** `ArchiveUI.OpenForChoice(ItemType)` detects the Double Candle chooser. The optional plugin loads through its manifest and bypasses the Rare2, Epic3, and Legendary4 exclusions in that context.
- **User-confirmed gameplay:** legendary Toxins was selected and used in a run. User also confirmed choices from legendary down through all rarities. Separate blue/purple equipment and unrelated-screen regression tests are not independently confirmed.
- **Managed tests:** dependency ordering/rejection, manifest paths, subscriber exception isolation, and disposable rarity policies pass. These tests do not prove gameplay behavior.
- **Verified live read:** item-pool snapshots returned 363 entries across all four rarities. These are pool entries, not a promise every entry can be safely granted.
- **Unsupported:** custom items, custom rarities, creatures/effects registration, player-stat writes, arbitrary starting loadouts, locked/unseen item granting, hot reload, and an in-game mod menu.

This is a foundation with a small evidenced API, not complete mod support for every game system.

## Use the already prepared copy

1. Open Steam and sign in.
2. Double-click `Launch-Framework.cmd` in this project. It starts `melon-test-game/Ring Of Pain.exe`, the new unpatched loader copy. The earlier `mod-work/playable-copy` belongs to a different binary-patch experiment.
3. Check `melon-test-game/MelonLoader/Latest.log` for `ROPMK_VERSION_PASS`, `ROPMK_HOST_READY`, and `ROPMK_HOOK_PASS`.
4. The local test example was enabled for gameplay verification. New installs default to disabled. Close the game, then use PowerShell:

```powershell
.\scripts\Set-Example.ps1 -Mode Enabled
.\scripts\Set-Example.ps1 -Mode Disabled
```

Changing a manifest or config takes effect after restarting the game. The plugin keeps the existing chooser and existing locked/unseen restrictions. “All Items” means all four existing rarities within the game's retained eligibility rules.

## Fresh setup from source

Use Windows PowerShell or PowerShell 7 in the extracted project directory. Downloaded tools stay local; no system .NET installation is required.

```powershell
.\scripts\Setup.ps1 -SourceGame 'C:\Program Files (x86)\Steam\steamapps\common\Ring of Pain'
.\scripts\Launch.ps1
# Wait for assembly generation and the main menu, then close the copy.
.\scripts\Build.ps1
.\scripts\Launch.ps1
```

Setup checks original files, copies only the game executable/library files and game data, backs up local saves, downloads pinned tools with expected hashes, and installs MelonLoader plus portable .NET in the copy. The first loader launch downloads its assembly-generation dependencies and creates wrappers from your own game. Setup refuses to overwrite an existing copy. For this prepared workspace, skip Setup and use Build/Launch.

Build requires the generated wrappers. It compiles the SDK, host, optional example, and tests; runs the managed tests; and installs our DLLs into the copy while preserving existing manifest settings. Close the copied game before installing. `Build.ps1 -NoInstall` only builds/tests.

## Reversibility

The original installation files remain unchanged. Launching the normal game from Steam uses the original installation. To disable the loader in the copy, close it and run `scripts/Disable-Framework.ps1`; re-enable with `-Enable`. The script renames only the loader's `version.dll` inside the copy. To disable just the example, use Set-Example. No script automatically restores or deletes saves.

Game copies share the normal local save location and normal Steam services. A private backup is under `private-save-backup`; keep it private. To uninstall, close the copy and remove the project/test-copy folder after preserving any source/config you need. There is no original-file restoration step because originals were not patched.

## Project layout

- `src/SDK`: game-independent public contracts, manifests, policies, configuration.
- `src/Host`: MelonLoader plugin and verified IL2CPP adapters.
- `src/DoubleCandleAllItems`: optional SDK example, disabled in its source manifest.
- `src/Tests`: meaningful managed SDK checks.
- `src/MelonSmokeTest`: initial no-hook loader test.
- `src/InspectInterop`: metadata-only wrapper-signature inspector.
- `scripts`: setup/build/launch/toggles/package; pinned tool fingerprints.
- `docs`: tutorial, API reference, findings, compatibility, troubleshooting, validation.
- `tools`, `evidence`, game copies, and private backups: local-only, excluded from releases.

See [getting started](docs/GETTING-STARTED.md), [API reference](docs/API.md), [findings](docs/FINDINGS.md), [compatibility](docs/COMPATIBILITY.md), [troubleshooting](docs/TROUBLESHOOTING.md), and [test checklist](docs/VALIDATION.md).

## Distribution

Run `scripts/Package.ps1` after building. It uses an explicit allowlist and packages only our source, documentation, scripts, and three framework/example DLLs. It does not include game binaries/assets, generated game wrappers, decompiled artifacts, tool downloads, logs, or saves. Recipients need their own compatible game. See THIRD-PARTY.md for upstream dependencies and licenses. Mods are trusted code; runtime exception handling cannot contain every native error or reverse arbitrary mod mutations.
