# Troubleshooting

## Which copy am I using?

The new framework launcher uses `RingOfPainModKit/melon-test-game/Ring Of Pain.exe`. Read MelonLoader/Latest.log: Game::ApplicationPath must point there. The old `mod-work/playable-copy` uses an earlier experiment. Steam's normal Play button starts the original installation.

## Copy closes or Steam opens another executable

Open Steam, sign in, and use Launch-Framework.cmd. The copy has steam_appid.txt=998740 and is started with its own working directory. This uses Valve's documented development-launch behavior without patching achievement or service code. Do not remove Steam initialization checks. Sandboxed test processes could not connect to Steam; launch normally as the signed-in desktop user.

## No framework markers

Look in `melon-test-game/MelonLoader/Latest.log` and `evidence/unity-current.log`. Initial generation requires network access and may take minutes. Check exact source hashes with `Setup.ps1 -SourceGame '<original>' -ValidateOnly`. If version.dll was disabled, use Disable-Framework.ps1 -Enable while the game is closed. First verify the smoke marker and startup hook before a gameplay mod.

## Runtime download/installation prompt

Setup supplies the complete portable .NET6 core and WindowsDesktop layout in the copy's dotnet folder. A WindowsDesktop zip alone lacks dotnet.exe/host and is insufficient. Do not assume DOTNET_ROOT replaces the loader's own detection; its source supports game-local dotnet lookup. Verify dotnet/dotnet.exe and both shared runtimes exist.

## Mod not loaded

Check manifest enabled, exact assembly filename/type name, dotted versions, dependencies, and errors. Put SDK mods under UserData/RingOfPainModKit/Mods/<id>, not directly in MelonLoader's Mods folder. The host DLL goes in Mods; the shared SDK goes in UserLibs. Restart after config/manifest changes. Malformed configurations/manifests and failed dependencies are logged. The SDK mod system is not a sandbox for untrusted DLLs.

## Only common items remain

Enter the Double Candle *starting chooser*, not the ordinary collection page. Look for ROPMK_CHOOSER_OPEN and ROPMK_RARITY_ALLOWED Rare2/Epic3/Legendary4. Check example enabled and AllowAllRarities config. Locked/unseen eligibility is intentionally retained. A separate hidden archive rarity filter exists and could affect unusual UI state; its behavior needs a targeted test before changing it. Do not globally remove rarity filters.

## Item-read callback error

The first all-rarity trial caught an unnecessary ArchiveUI.Instance prerequisite; the static item-pool adapter was corrected. Subscriber errors are isolated, so the rarity allowance remained active and legendary gameplay succeeded. The corrected adapter passed a live test with 363 entries across all four rarities. Background or early item reads throw deliberately.

## Revert

Close the copy and disable the example or loader using the supplied scripts. Run the original from Steam for normal files. Save backups are private and not automatically restored. Loader cleanup cannot reverse arbitrary changes made by third-party mods or progression already saved during gameplay.
