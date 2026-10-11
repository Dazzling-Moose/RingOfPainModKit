# Reverse-engineering findings

This document records facts from the locally supplied installation. Addresses below are RVAs (offsets from the loaded GameAssembly module), not universal memory addresses. They are research references, not hardcoded runtime hook addresses. Source declarations, disassembly, generated wrappers and assets remain private; only method identities and factual findings are published here.

## Build and methodology

Original PlayerSettings assets and live loader output agree on Ring Of Pain 1.6.02f2, Twice Different, Unity 2019.4.29f1, Windows x64 IL2CPP. The metadata header has magic FAB11BAF and version24. Il2CppDumper 6.7.46 processed it with metadata layout24.4 and binary layout24.5. It generated a declaration map, address/name map, and dummy assemblies. A later console ReadKey failure occurred after outputs were generated; outputs were checked rather than treating the process result as proof of game behavior.

Native disassembly tied selection code to the metadata names. MelonLoader then generated separate managed wrappers, whose methods, fields and enum types were inspected before compiling hooks. Wrapper namespaces are prefixed (`Il2CppRingOfPain`) and the main game wrapper is `Il2CppRingOfPainRuntime.dll`, not Assembly-CSharp.dll. Several simple methods share native addresses through IL2CPP code sharing; addresses alone do not establish unique method identity. Runtime patches use exact reflected wrapper signatures.

The game files' four expected hashes are in scripts/game-fingerprint.json. The host checks those files before installing its patches. Originals remain unchanged and the loader copy uses an unpatched GameAssembly.

## Starting-item path

1. TitleScreenUI's PlayButtonClick asynchronous state machine reads GameRunSetSelectorUI.GameRunSet. The Double Candle branch compares LightVariant to1.
2. Before opening the chooser, that branch stores the selected GameRunSet into PlayerData.LastSelectedGameRunSet. Disassembly around RVA8871F7..887244 verifies this, so the stored variant is usable for the hook's context check.
3. The branch calls `ArchiveUI.OpenForChoice(ItemType)` at RVA4FE7F0. A direct-call scan found this caller; that scan alone cannot exclude indirect callers.
4. OpenForChoice clears/builds choice filters, excludes the light slot, locked/unseen items, and tiers Rare2, Epic3, Legendary4 through `ArchiveListView.ExcludeRarity(ItemTier)` at RVA3E97B0.
5. `ArchiveListView.GetListOfAllItems()` at RVA3E9B70 uses those filters to populate the chooser.
6. `ItemCardChoiceMaximiser.PressPlay()` at RVA4A09D0 starts its asynchronous body (MoveNext RVA4AD990). That path uses `GameRunSet.WithEquip(ItemType)` RVA702560 and `CardStarter.StartGameFromTitleScreen(GameRunSet,bool)` RVA5B8910.

These identities/signatures were established from metadata/native code; actual startup and chooser hooks were then verified in the running copy. A user selected legendary Toxins and reported it worked in a run.

## Exact implemented hooks

| Method | RVA | Hook purpose | Live evidence |
|---|---|---|---|
| GameStateControl.DoAwake() -> void | 702FC0 | Observational postfix; mark initialized | ROPMK_HOOK_PASS |
| ArchiveUI.OpenForChoice(ItemType) -> void | 4FE7F0 | Prefix detects DoubleCandle and publishes event; finalizer restores scope | ROPMK_CHOOSER_OPEN |
| ArchiveListView.ExcludeRarity(ItemTier) -> void | 3E97B0 | Skip matching exclusions only for scoped list and active allowance | Three ROPMK_RARITY_ALLOWED markers |

No calls in item-drop/normal-loot methods are changed. The original chooser builds its own list as before; we omit only the selected rarity exclusions during the verified context. Locked/unseen and inventory-slot filters remain. That scope is supported by source design and native path analysis; unrelated-screen regression testing is still required.

## Rarity and archive details

ItemTier is None0=0, Common1=1, Rare2=2, Epic3=3, Legendary4=4. ItemTierFlags is a different enum using1,2,4,8. LightVariant.DoubleCandle=1; Candle=0, InvertedCandle=3, SelectiveStone=8.

ArchiveListView's filter data includes excludeLocked (native field offset80 hex), excludeUnseen81, excludeNoneSelector82, excludedSlots88, excludedRarities90. Wrappers expose those fields as properties, but the framework does not mutate them directly. ExcludeRarity adds a tier to the excluded-rarities set.

A second archive UI filter uses UsingAllRarityFilter (B8) and RarityFilter (BC); a native filter helper at3EAFE0 checks it. ArchiveItemsUI.SetUI at3E8C40 hides the rarity controls in choice mode. A stale hidden filter is a research possibility, not a verified cause of the old failed patch. It has not been broadly overridden.

## Items and assets

`EquippableItem` is a ScriptableObject with Type, GUID, localized Name, Slot, Tier and base stat properties. `ItemPool` supplies prefab lookup by GUID/type and multiple item-choice overloads. The verified static `ArchiveListView.GetAllItemsFromPools()` returns a HashSet of ItemPool.CapturedItem; its inherited Item property supplies EquippableItem.

The first snapshot test had an avoidable extra ArchiveUI singleton check and threw while the real chooser instance was alive. It was removed because the pool method is static. The corrected adapter passed a live read test, returning 363 entries (Common 95, Rare 95, Epic 87, Legendary 86). Registered pool entries are not proof that every item is safe to grant.

resources.assets and sharedassets0.assets were inspected. Custom serialized type trees are stripped, so header/name inspection is available but full custom object layouts cannot safely be inferred. No asset edits or guessed serialized fields are used. Runtime generated wrappers are the basis of the current item adapter.

## Other real systems and research roadmap

| Area | Verified candidate types/methods | Current SDK status / next proof needed |
|---|---|---|
| Item lookup and pools | ItemPool.GetPrefabByGUID RVA636F90; GetPrefabByItemType637110; item-choice overloads | Read snapshots only; test item lifetime/unlock/serialization before writes or registration |
| Weighted cards | RingOfPain.WeightedPool; nested WeightedPool.WeightedCardData; AppendToListAsDefault3D7290 | Future; understand selection/weight normalization before custom pools |
| Inventory | Inventory, enumerators/count; nested Inventory.SaveData.ItemData | Future; trace equipment effects, slot checks, saves and cleanup |
| Card spawning | CardSpawn, MonsterPool property, card-positioning functions | Future; no generic card-registration promise |
| Player stats | Player derives from MonsterCard; Player Health/MaxHealth; StatsConfiguration Health/Attack/Defense/Speed | Future; identify effective-vs-base stats and timing before read/write adapter |
| Creatures | MonsterCard Type/Category/Souls; MonsterPool.Initialise6839B0; TryRequestMonster overloads | Future; no custom enemy registration implemented |
| Effects | ItemEffect.Initialise(Item); ActiveItemEffect.Initialise(ActiveItem); StatusEffects.GetImplementation5A0DC0 and RegisterStatusEffectActions5A1940 | Future; trace effect ownership, turn subscriptions and disposal |
| Lifecycle | TurnSystem.DoAwake44C950; GameRunManager state/progression collections; LevelManager paths; GameStateControl saving/main-scene callbacks | Startup observation only; event ordering and save boundaries need tests |

The private evidence index currently covers24 actual top-level/candidate types. `WeightedPool.WeightedCardData` is a nested declaration whose name lacks the parent namespace in the dumper's display; it was separately identified, not an invented top-level API.

## Requested search names that differ from real types

- DoubleCandle is a LightVariant value and state/member references; there is no standalone DoubleCandle class in the indexed metadata.
- ItemData appears in nested filter/save/upgrade/run-state structures, not a universal item-definition class. EquippableItem is the current definition source.
- MonsterData appears nested in MonsterPool and MonsterDescription.
- PlayerStats appears in UI/statistics/modifier types; a central PlayerStats object has not been established.
- RarityFilter is archive filter state, not a separate verified class.
- ChooseItem search results include an asynchronous CardComparison helper and method/flag names; no standalone chooser class was assumed.

## Loader choice and safety

BepInEx Windows IL2CPP builds788,787 and pre.2 failed in native invocation interception before any custom plugin ran. Root cause remains unknown. MelonLoader 0.7.3 passed a small no-hook plugin first, then a real startup observation hook, then a chooser observation, and finally the optional rarity policy. This sequence separates loader failures from gameplay-hook failures.

The copy uses Steam app ID998740 as confirmed by the installed manifest and actual game metadata. Launching with the copy's working directory prevents Steam from silently redirecting the development copy to the original. No achievement, Steam service, or anti-cheat code was modified or intercepted. Copies share normal saves; private backups were made before tests.
