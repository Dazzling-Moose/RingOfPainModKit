# Creating a mod

A loader gets C# code into this IL2CPP game. The host then discovers small SDK mods from JSON manifests. Your mod need not reference the generated game assemblies for the supported SDK features.

## Minimal project

Create a .NET 6 C# class library and reference `RingOfPainModKit.SDK.dll`, or reference its source project. Use a project reference with `Private=false` for a binary SDK reference: the host supplies the shared assembly through MelonLoader's UserLibs directory. Do not ship a separate copy inside each mod folder.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="RingOfPainModKit.SDK">
      <HintPath>PATH-TO-SDK/RingOfPainModKit.SDK.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>
</Project>
```

```csharp
using RingOfPainModKit.SDK;
namespace MyMod;
public sealed class Plugin : IRingOfPainMod
{
    public void Initialize(ModContext context)
    {
        context.Log("My mod initialized.");
        context.Track(context.StartingItems.SubscribeOpening(selection =>
            context.Log("Double Candle chooser opened.")));
    }
    public void Dispose() { }
}
```

This example observes an event and makes no gameplay change. Copy your built DLL and this manifest to `melon-test-game/UserData/RingOfPainModKit/Mods/my-mod/`:

```json
{
  "id": "my-mod",
  "name": "My Mod",
  "version": "0.1.0",
  "assembly": "MyMod.dll",
  "entryType": "MyMod.Plugin",
  "enabled": true,
  "gameVersion": "1.6.02f2",
  "minimumKitVersion": "0.1.0",
  "dependencies": {}
}
```

Replace MyMod.dll with your actual assembly filename. Restart the copied game and look for `ROPMK_MOD_LOADED: my-mod`. Open the Double Candle starting chooser to check the event message. DLL loading alone does not verify event execution.

## Configuration and cleanup

A public settings class with a public parameterless constructor can be loaded through `context.ReadConfiguration<Settings>()`. Missing configuration creates defaults in `UserData/RingOfPainModKit/Config/<id>/config.json`. Existing invalid JSON disables that mod's initialization rather than silently discarding the user's file. Property names use System.Text.Json's default casing. Configuration has no automatic migration or reload.

Use `context.Track(...)` for event subscriptions and rarity allowance leases. The host releases tracked resources if initialization fails and during shutdown. Put your other cleanup in Dispose. Do not retain game object pointers in background tasks. Item reads require the initialized game and Unity's main thread.

## Dependencies

Dependencies map IDs to minimum numeric dotted versions, e.g. `{"my-base-mod":"0.1.0"}`. Dependencies initialize first. Missing, disabled, too-old dependencies or cycles reject the enabled graph safely. A dependency that fails initialization causes its dependents to be skipped. IDs must be unique; versions currently follow System.Version rather than full SemVer prerelease syntax. Additional private helper-DLL resolution is not a supported SDK feature yet; keep the initial mod self-contained apart from the SDK.

## Optional gameplay example

`src/DoubleCandleAllItems` is a complete buildable SDK example. Its AllowAllRarities config defaults true, while the manifest defaults disabled. It registers all four rarity allowances and logs chooser events. It does not alter item definitions or normal loot pools. The host applies those allowances only to the exact chooser instance during the verified Double Candle opening method.

## Build and release

Run Build.ps1 after the initial loader generation. The SDK API has no proprietary compile dependency; the host requires the locally generated wrappers and loader libraries. Run Package.ps1 to produce `artifacts/RingOfPainModKit-0.1.0.zip`. The source package includes our three compiled DLLs but no third-party or game DLLs. Fresh recipients perform Setup and first-run generation themselves. Never distribute your game copy, interop assemblies, declaration dump, or private save backup.
