# Validation record and gameplay checklist

## Passed

- Original four-file hashes checked against pre-existing records; fresh copies use unpatched GameAssembly.dll.
- MelonLoader smoke plugin wrote ROPMK_SMOKE_PASS.
- Host wrote ROPMK_VERSION_PASS and ROPMK_HOST_READY.
- Real GameStateControl.DoAwake() execution wrote ROPMK_HOOK_PASS.
- User opened Double Candle chooser; log wrote ROPMK_CHOOSER_OPEN.
- SDK manifest discovery loaded optional example, generated its per-mod JSON config, and wrote ROPMK_MOD_LOADED.
- Chooser log recorded Rare2, Epic3, Legendary4 allowances.
- User reported legendary Toxins worked in a run. This is user-confirmed gameplay, independently supported by hook logs; no screenshot or memory inventory read was taken.
- Managed SDK tests cover dependencies, paths, callback isolation, and policy cleanup. Read current build output for test count.

## Caught during incremental tests

- BepInEx trials crashed before custom gameplay code ran; selected MelonLoader instead.
- AppContext.BaseDirectory was empty under this loader; relative SDK-mod assembly load was rejected. Corrected to verified MelonEnvironment.GameRootDirectory.
- Item snapshot subscriber required an unavailable archive singleton even though GetAllItemsFromPools is static. Error was logged without stopping the rarity policies; prerequisite removed. Live retest passed:363 entries across Common 95/Rare 95/Epic 87/Legendary 86.

## Still to verify

Record item names and observations rather than assuming a compiled hook proves behavior:

1. Open Double Candle: verify white, blue, purple, orange eligible cards appear. Passed by user report: "I can choose legendary and all the way down".
2. Select/equip a blue item and a purple item in separate runs.
3. White item selection still works.
4. Disable example and restart: starting chooser returns to common-only rules.
5. Enabled example: ordinary collection filtering and normal loot screens behave as before.
6. Other candle modes behave as before.
7. Repeated chooser open/close has no leaked rarity policy or unexpected filter state.
8. Corrected GetRegisteredItems returns snapshots with all four rarity groups: passed in the live retest.

Locked/unseen items are intentionally not bypassed. Testing every item/effect/save interaction is outside the current coverage. Hook scope and cleanup are designed to preserve unrelated contexts, but full regression coverage remains incomplete.

Local evidence files are private and excluded from releases because game logs and analysis may include user data or proprietary declarations. Documentation publishes technical conclusions and test markers only.
