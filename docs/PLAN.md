# RingOfPainModKit implementation plan

## Architecture

Use MelonLoader 0.7.3 x64 after verified local smoke and native game-hook tests. It supplies CoreCLR, IL2CPP interop generation, logging, and Harmony support. RingOfPainModKit adds a small C# SDK and one host plugin. Mods use JSON manifests and a managed interface; game adapters translate only verified game types. No game code, generated interop assemblies, assets, or game copies belong in release packages.

BepInEx builds 788, 787, and v6.0.0-pre.2 failed before plugin execution in IL2CPPChainloader.OnInvokeMethod. The cause is unresolved; the established MelonLoader fallback passed. No custom native loader is planned.

## Phases and acceptance criteria

1. **Evidence and safety:** revalidate source hashes, retain metadata/disassembly evidence, back up local saves, create a fresh unpatched game copy. Pass: four source hashes match prior records; copied DLL matches original; no old experimental byte patch.
2. **Loader smoke test:** start clean copy with Steam app-ID context and MelonLoader. Pass: copied executable stays running; interop generation completes; loader log records chainloader initialization.
3. **Minimum SDK:** build host and log-only mod; add build, installation, launch, and disable scripts. Pass: host and log-only mod report their IDs and versions in the running game.
4. **Verified hook:** exact-signature lookup of ArchiveUI.OpenForChoice(ItemType), initially observational. Pass: entering Double Candle logs a hook event from the copied game, with no gameplay change. This requires an interactive action; a successful build is not proof.
5. **Foundation:** manifests, dependencies, per-mod JSON configuration, exception isolation, exact game fingerprint guard, disposable subscriptions and host-owned Harmony patches and per-mod subscription ownership. Pass: meaningful managed tests cover invalid manifests, dependency failures/cycles, event exception isolation, and incompatible fingerprints; live log shows accepted/rejected mods.
6. **Optional DoubleCandleAllItems:** context-scoped rarity exclusion override through the adapter. Preserve normal loot and unrelated screens. Default to preserving lock/seen rules until item eligibility and save implications are understood. Pass: user can select and equip a previously seen/unlocked item at each of four rarities; normal loot and other candle modes remain unchanged. Locked-item granting is future work, not a presumed supported feature.
7. **Documentation and packaging:** SDK reference, tutorial, compatibility matrix, findings, troubleshooting, and source-only distribution. Pass: package excludes test-game, generated game assemblies, proprietary analysis, save backups, and downloaded tool binaries.

## Confirmed findings

- Ring of Pain 1.6.02f2, Unity 2019.4.29f1; metadata magic FAB11BAF; Il2CppDumper layout 24.4/24.5.
- GameAssembly SHA-256: 96A36107519C7142C4B162F32BEF0DD16E9FDB5A0F302B4FFC982C0F3BB2777A.
- ArchiveUI.OpenForChoice(ItemType) calls ExcludeRarity(ItemTier) for Rare2, Epic3, Legendary4; ArchiveListView.GetListOfAllItems checks excluded rarity, slot, locked, and seen rules.
- LightVariant.DoubleCandle=1; ItemTier values are 1/2/3/4; ItemTierFlags values are 1/2/4/8.
- Steam can close a directly launched copy and relaunch the installed original. steam_appid.txt=998740 plus correct working directory solved that in the earlier test.
- Prior blue-test bytes were verified in a running process, but gameplay success has not been established.

## Risks and boundaries

- First-run interop generation needs downloads and can be slow or fail. Generated signatures must be checked before hooking.
- Steam and the game use the same saves for all copies. Backup is local and private; nothing changes achievement or service code.
- Runtime mods are arbitrary trusted code, not a security sandbox. Exception isolation cannot contain all native failures.
- Hash guards intentionally reject future game builds. Hook cleanup removes owned patches/subscriptions; it cannot automatically reverse arbitrary gameplay mutations or saves.
- Custom items, creatures, status-effect registration, asset bundles, in-game mod UI, and write access to stats remain unsupported until supported by evidence and runtime tests.
- The custom serialized asset type trees are stripped; base headers and names were read, but complete item asset layouts have not been inferred.

## Current milestone status

Loader and real startup/Double Candle observation hooks passed. Optional SDK mod discovery/config and rarity allowance hooks passed. User confirmed legendary Toxins in gameplay. Separate blue/purple equipment and unrelated-screen regression checks remain open. Corrected item snapshots passed a live 363-entry read across all four rarities. See VALIDATION.md for measured coverage.
