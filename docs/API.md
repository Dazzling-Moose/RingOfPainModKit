# Public API 0.1.0

## IRingOfPainMod

`void Initialize(ModContext context)` initializes a trusted SDK mod. `void Dispose()` releases mod-owned resources. The host separately releases everything registered with context.Track even after an initialization failure. Hot unloading and hot reload are unsupported.

## ModContext

| Member | Meaning |
|---|---|
| Id | Manifest's unique mod ID |
| ConfigurationDirectory | Absolute per-mod settings directory |
| Log(string) | Message through the host's MelonLoader logger, prefixed with mod ID |
| ReadConfiguration<T>() where T:new() | Reads config.json or writes defaults; invalid JSON throws |
| Track<T>(T resource) where T:IDisposable | Host-managed cleanup; returns the same resource |
| StartingItems | IStartingItemAPI |
| Items | IItemAPI |

## Starting items and rarity

`IStartingItemAPI.SubscribeOpening(Action<StartingItemContext>)` returns a disposable subscription. It currently fires only for the verified Double Candle path through `ArchiveUI.OpenForChoice(ItemType)`. Context carries PreviousItemType (numeric game ItemType) and GameVersion. Subscriber failures are logged and do not block later subscribers.

`IStartingItemAPI.AllowRarities(params ItemRarity[])` returns a disposable allowance lease. It does not select an item or change normal loot. During the scoped chooser call the adapter skips matching `ExcludeRarity` calls for the exact archive-list object. Multiple mods' allowances are combined by union; disposing one lease removes only that mod's allowance. Values outside 1..4 throw. There is no denial/preference ordering or custom predicate yet.

| SDK enum | Game ItemTier | Colour |
|---|---|---|
| Common=1 | Common1=1 | White |
| Rare=2 | Rare2=2 | Blue |
| Epic=3 | Epic3=3 | Purple |
| Legendary=4 | Legendary4=4 | Orange |

None=0 is reserved and cannot be allowed. ItemTierFlags is a separate bitmask (1,2,4,8); it must not be passed as ItemRarity. Locked/unseen and slot restrictions remain game-controlled. The SDK exposes no direct locked-item granting or arbitrary loadout API.

## Items (verified read-only adapter)

`IItemAPI.GetRegisteredItems()` returns `IReadOnlyList<ItemSnapshot>`, copies of pool entries obtained through verified static `ArchiveListView.GetAllItemsFromPools()`. It must run on the captured Unity main thread after the game initialization hook. Each snapshot includes numeric TypeId, Guid, localized Name, Rarity, numeric SlotId, and BaseAttack/BaseDefense/BaseSpeed/BaseHealth/BaseClarity. BaseHealth maps to the game's BaseMaxHealth property.

These are registered pool entries, not current chooser eligibility or player-owned items. Pool membership can include duplicate item definitions; callers can group by TypeId or Guid when appropriate. Names can depend on localization. Use LINQ for rarity/slot/type queries. Snapshots provide no mutation or native object access. Registration of custom definitions and attribute writes remain future work.

## Hook ownership

Only the host patches game methods, under Harmony ID `community.ringofpainmodkit`. Exact parameter and return types are checked before registration; no raw address patches are installed. Shutdown removes only owned Harmony patches and tracked subscriptions/allowances. A Harmony finalizer restores the per-thread chooser scope, including on managed exceptions. Native crashes and arbitrary mod side effects are outside these guarantees.

## Future modules

PlayerAPI, CreatureAPI, EffectAPI, custom item/rarity definitions, generic EventAPI, custom filters, and starting-loadout grants have no public implementation in this version. The findings document lists real candidate systems for research; those names are not promises of SDK APIs.

Live read test returned 363 pool entries: Common 95, Rare 95, Epic 87, Legendary 86. This verifies snapshot retrieval at chooser opening; it does not verify item mutation, custom registration, or arbitrary eligibility.
