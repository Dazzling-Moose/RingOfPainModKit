# RingOfPainModKit 0.1.0

I made RingOfPainModKit as an unofficial, experimental C# modding framework for **Ring of Pain 1.6.02f2** on Windows x64, using Unity **2019.4.29f1** and IL2CPP.

My goal is to give other modders a starting point so they don’t have to repeat the same setup and reverse engineering.

This project is not affiliated with Twice Different or Humble Games.

## What I have confirmed

- **Plugin loading:** I verified that MelonLoader 0.7.3 loads a basic plugin. The framework checks four original game-file hashes and successfully hooks `GameStateControl.DoAwake()`.
- **Double Candle detection:** I verified that `ArchiveUI.OpenForChoice(ItemType)` detects the Double Candle starting-item chooser. The optional plugin loads through its manifest and bypasses the Rare2, Epic3, and Legendary4 exclusions specifically in that context.
- **Gameplay:** I selected legendary Toxins and used it successfully in a run. I also confirmed that I could choose items across all four rarities, from common through legendary. I haven’t separately documented blue and purple equipment tests or completed checks of unrelated selection screens.
- **Managed tests:** All 19 checks passed, covering dependency handling, manifest paths, subscriber error isolation, rarity-policy cleanup, game-file fingerprints, and tracked resource cleanup. These checks support the framework’s reliability, but they don’t replace gameplay testing.
- **Item reading:** I verified that the item API returned 363 registered pool entries across all four rarities. This confirms that the framework can read those entries; it doesn’t establish that every item can safely be granted.

## What is not supported yet

I haven’t implemented custom items, custom rarities, creature or effect registration, player-stat changes, arbitrary starting loadouts, locked or unseen item granting, hot reload, or an in-game mod menu.

This first version provides a small, tested foundation. There is still more work to do before it supports the game’s broader systems.

## Launching a prepared copy

If you have already completed setup and built the framework:

1. Open Steam and sign in.
2. Double-click `Launch-Framework.cmd`. It starts `melon-test-game/Ring Of Pain.exe`, the separate game copy used by the framework.
3. Check `melon-test-game/MelonLoader/Latest.log` for `ROPMK_VERSION_PASS`, `ROPMK_HOST_READY`, and `ROPMK_HOOK_PASS`.

I enabled DoubleCandleAllItems in my local copy for testing. New installations default to having the example disabled.

Close the game before changing its setting. Use one of these PowerShell commands:

```powershell
# Enable the example.
.\scripts\Set-Example.ps1 -Mode Enabled

# Disable the example.
.\scripts\Set-Example.ps1 -Mode Disabled
```

Manifest and configuration changes take effect after restarting the game.

I kept the existing chooser and its locked-item, unseen-item, and slot restrictions. “All Items” refers to all four existing rarities within those remaining eligibility rules.

## Fresh setup from source

Use Windows PowerShell or PowerShell 7 from the extracted project directory. The downloaded tools stay inside the project; a system-wide .NET installation is not required.

```powershell
.\scripts\Setup.ps1 -SourceGame 'C:\Program Files (x86)\Steam\steamapps\common\Ring of Pain'
.\scripts\Launch.ps1

# Wait for assembly generation and the main menu, then close the copy.

.\scripts\Build.ps1
.\scripts\Launch.ps1
```

Replace the source path if your game is installed somewhere else.

I designed Setup to check the original files, create a separate game copy, back up local saves, download the pinned tools, and install MelonLoader with portable .NET inside that copy.

The first loader launch downloads its assembly-generation dependencies and generates wrappers from your own game. Setup refuses to overwrite an existing copy. If you have already completed setup, use Build and Launch instead.

Build requires those generated wrappers. It compiles the SDK, host, optional example, and tests; runs the managed checks; and installs the framework DLLs into the copy while preserving existing manifest settings.

Close the copied game before installing. Use `Build.ps1 -NoInstall` to build and test without installing.

## Reversibility

I verified that my original game files remained unchanged. Launching the normal game from Steam uses the original installation.

To disable the loader in the copy, close the game and run:

```powershell
.\scripts\Disable-Framework.ps1
```

To re-enable it:

```powershell
.\scripts\Disable-Framework.ps1 -Enable
```

This script renames the loader’s `version.dll` inside the copy. To disable only DoubleCandleAllItems, use Set-Example instead.

The game copies share the normal local save location and Steam services. Setup places a private save backup under `private-save-backup`. Keep that backup private. No script automatically restores or deletes saves.

To uninstall, close the copy and remove the project and test-copy folder after preserving any source or configuration you want to keep. The original game files do not need restoring because the framework does not patch them.

## Project layout

- `src/SDK`: public contracts, manifests, policies, and configuration.
- `src/Host`: the MelonLoader plugin and verified IL2CPP adapters.
- `src/DoubleCandleAllItems`: the optional example, disabled in its source manifest.
- `src/Tests`: managed SDK checks.
- `src/MelonSmokeTest`: the initial plugin-loading test without gameplay hooks.
- `src/InspectInterop`: the wrapper-signature inspection tool.
- `scripts`: setup, build, launch, enable/disable, and packaging scripts.
- `docs`: tutorials, API reference, findings, compatibility, troubleshooting, and validation.
- `tools`, `evidence`, game copies, and private backups: local files excluded from releases.

I documented the setup and findings here:

- [Getting started](docs/GETTING-STARTED.md)
- [API reference](docs/API.md)
- [Reverse-engineering findings](docs/FINDINGS.md)
- [Compatibility](docs/COMPATIBILITY.md)
- [Troubleshooting](docs/TROUBLESHOOTING.md)
- [Validation and test checklist](docs/VALIDATION.md)

## Distribution

Run `scripts/Package.ps1` after building to create a release package.

I checked the prepared v0.1.0 ZIP and confirmed that it contains the project source, documentation, scripts, and three framework/example DLLs. It excludes game binaries and assets, generated game wrappers, decompiled artifacts, downloaded tools, logs, and saves.

Anyone using the framework needs their own compatible copy of Ring of Pain. See [THIRD-PARTY.md](THIRD-PARTY.md) for upstream tools and licenses.

Mods run as trusted code. The framework provides error handling and cleanup, but it cannot contain every native error or undo arbitrary changes made by another mod.
