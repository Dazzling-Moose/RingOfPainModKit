using System.Reflection;
using System.Runtime.Loader;
using HarmonyLib;
using MelonLoader;
using Il2CppRingOfPain;
using RingOfPainModKit.SDK;
[assembly: MelonInfo(typeof(RingOfPainModKit.Host.Plugin), "RingOfPainModKit", "0.1.0", "Community")]
[assembly: MelonGame("Twice Different", "Ring Of Pain")]
namespace RingOfPainModKit.Host;
public sealed class Plugin:MelonMod
{
    internal static Plugin? Active;
    internal readonly StartingItemPolicies Policies;
    private readonly List<(IRingOfPainMod Mod,ModContext Context)> loaded=new();
    private HarmonyLib.Harmony? hooks;
    internal int MainThread;
    internal bool Ready;
    internal static void Log(string text)=>Active?.LoggerInstance.Msg(text);
    public Plugin()=>Policies=new StartingItemPolicies(ex=>LoggerInstance.Error(ex.ToString()));
    public override void OnInitializeMelon()
    {
        Active=this; MainThread=Environment.CurrentManagedThreadId;
        try
        {
            VerifyVersion();
            hooks=new HarmonyLib.Harmony("community.ringofpainmodkit");
            Patch(typeof(GameStateControl),nameof(GameStateControl.DoAwake),Type.EmptyTypes,null,nameof(GameHooks.Startup));
            Patch(typeof(ArchiveUI),nameof(ArchiveUI.OpenForChoice),new[]{typeof(ItemType)},nameof(GameHooks.Opening),null,nameof(GameHooks.ClosedScope));
            Patch(typeof(ArchiveListView),nameof(ArchiveListView.ExcludeRarity),new[]{typeof(ItemTier)},nameof(GameHooks.Exclude),null);
            LoadMods();
            Log("ROPMK_HOST_READY: verified version; hooks registered. Waiting for game method execution.");
        }
        catch(Exception ex) { LoggerInstance.Error("Framework disabled: "+ex); Cleanup(); }
    }
    private void Patch(Type type,string name,Type[] args,string? prefix,string? postfix,string? finalizer=null)
    {
        var target=type.GetMethod(name,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance,null,args,null)
            ?? throw new MissingMethodException(type.FullName,name);
        if(target.ReturnType!=typeof(void)) throw new InvalidDataException("Unexpected method return type: "+target);
        HarmonyMethod? H(string? n)=>n is null?null:new HarmonyMethod(typeof(GameHooks).GetMethod(n,BindingFlags.Static|BindingFlags.Public)!);
        hooks!.Patch(target,H(prefix),H(postfix),null,H(finalizer),null);
    }
    private static void VerifyVersion()
    {
        var root=MelonLoader.Utils.MelonEnvironment.GameRootDirectory;
        var expected=new Dictionary<string,string> {
            ["GameAssembly.dll"]="96A36107519C7142C4B162F32BEF0DD16E9FDB5A0F302B4FFC982C0F3BB2777A",
            ["Ring Of Pain_Data/il2cpp_data/Metadata/global-metadata.dat"]="F27BE1FB6DEB8C2EF47E41A2CC08BFD687E37E9E5E1CE4429E5BC11576C19B75",
            ["Ring Of Pain_Data/resources.assets"]="19AC92538DD6E0496CB58C983B728F6725C8CF880A5269A9F5CD69AEE7029726",
            ["Ring Of Pain_Data/sharedassets0.assets"]="3791929250ED5F21219787CC94AAE2476FFE19C1A4D1B35421C491E7B2C5D91C" };
        GameFingerprint.Verify(root,expected);
        Log("ROPMK_VERSION_PASS: four original game hashes match tested 1.6.02f2 installation.");
    }
    private void LoadMods()
    {
        var root=Path.Combine(MelonLoader.Utils.MelonEnvironment.GameRootDirectory,"UserData","RingOfPainModKit");
        var mods=Path.Combine(root,"Mods");Directory.CreateDirectory(mods);
        var entries=new Dictionary<string,(ModManifest Manifest,string Directory)>();
        var duplicate=false;
        foreach(var dir in Directory.GetDirectories(mods).OrderBy(x=>x,StringComparer.Ordinal))
        {
            var path=Path.Combine(dir,"manifest.json");if(!File.Exists(path)) continue;
            try {
                var m=ModManifest.Read(path);
                if(m.GameVersion!="1.6.02f2"||Version.Parse(m.MinimumKitVersion)>new Version(0,1,0)) throw new InvalidDataException("Incompatible manifest: "+m.Id);
                if(entries.ContainsKey(m.Id)){duplicate=true;throw new InvalidDataException("Duplicate mod ID: "+m.Id);}
                entries.Add(m.Id,(m,dir));
                Log($"Manifest {m.Id}: enabled={m.Enabled}");
            } catch(Exception ex) { LoggerInstance.Error(path+": "+ex.Message); }
        }
        if(duplicate)throw new InvalidDataException("Duplicate IDs reject the enabled mod graph.");
        var successful=new HashSet<string>();
        foreach(var m in DependencyResolver.Order(entries.Values.Select(e=>e.Manifest)))
        {
            if(m.Dependencies.Keys.Any(d=>!successful.Contains(d))) { LoggerInstance.Error("Skipping failed dependency: "+m.Id);continue; }
            var context=new ModContext(m.Id,Path.Combine(root,"Config",m.Id),Policies,new ItemAdapter(),s=>Log("["+m.Id+"] "+s));
            IRingOfPainMod? mod=null;
            try {
                var assembly=AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(entries[m.Id].Directory,m.Assembly));
                var type=assembly.GetType(m.EntryType,true)!;
                mod=Activator.CreateInstance(type) as IRingOfPainMod ?? throw new InvalidDataException("Entry type does not implement IRingOfPainMod.");
                mod.Initialize(context);loaded.Add((mod,context));successful.Add(m.Id);Log("ROPMK_MOD_LOADED: "+m.Id);
            } catch(Exception ex) { LoggerInstance.Error(m.Id+": "+ex);try {mod?.Dispose();}catch(Exception dispose){LoggerInstance.Error(dispose);}context.ReleaseResources(e=>LoggerInstance.Error(e)); }
        }
    }
    public override void OnDeinitializeMelon()=>Cleanup();
    private void Cleanup()
    {
        foreach(var entry in loaded.AsEnumerable().Reverse()) {
            try {entry.Mod.Dispose();}catch(Exception ex){LoggerInstance.Error(ex);}
            entry.Context.ReleaseResources(ex=>LoggerInstance.Error(ex));
        }
        loaded.Clear();hooks?.UnpatchSelf();hooks=null;Ready=false;
    }
}
public static class GameHooks
{
    [ThreadStatic] private static IntPtr chooser;
    public static void Startup()
    {
        if(Plugin.Active is not { } host)return;
        host.Ready=true;Plugin.Log("ROPMK_HOOK_PASS: GameStateControl.DoAwake() executed.");
    }
    public static void Opening(ArchiveUI __instance,ItemType itemType,out IntPtr __state)
    {
        __state=chooser;chooser=IntPtr.Zero;
        try {
            if(Plugin.Active is not { } host||!host.Ready)return;
            var state=GameStateControl.Instance;
            if(state==null||state.PlayerData.LastSelectedGameRunSet.LightVariant!=LightVariant.DoubleCandle)return;
            chooser=__instance.archiveListView.Pointer;
            Plugin.Log("ROPMK_CHOOSER_OPEN: verified DoubleCandle context.");
            host.Policies.Publish(new StartingItemContext((int)itemType,"1.6.02f2"));
        }catch(Exception ex){chooser=IntPtr.Zero;Plugin.Log("Chooser observation failed; preserving game filters: "+ex);}
    }
    public static Exception? ClosedScope(Exception? __exception,IntPtr __state) {chooser=__state;return __exception;}
    public static bool Exclude(ArchiveListView __instance,ItemTier itemTier)
    {
        try {
            if(chooser!=IntPtr.Zero&&__instance.Pointer==chooser&&Plugin.Active!.Policies.IsAllowed((ItemRarity)(int)itemTier)) {
                Plugin.Log("ROPMK_RARITY_ALLOWED: "+itemTier);return false;
            }
        } catch(Exception ex){Plugin.Log("Rarity policy failed; preserving filter: "+ex);}
        return true;
    }
}
internal sealed class ItemAdapter:IItemAPI
{
    public IReadOnlyList<ItemSnapshot> GetRegisteredItems()
    {
        var host=Plugin.Active??throw new InvalidOperationException("Framework unavailable.");
        if(!host.Ready||Environment.CurrentManagedThreadId!=host.MainThread)throw new InvalidOperationException("Item reads require initialized game and Unity main thread.");
        var result=new List<ItemSnapshot>();
        foreach(var captured in ArchiveListView.GetAllItemsFromPools()) {
            var item=captured.Item;if(item==null)continue;
            result.Add(new ItemSnapshot((int)item.Type,item.GUID,item.Name,(ItemRarity)(int)item.Tier,(int)item.Slot,item.BaseAttack,item.BaseDefense,item.BaseSpeed,item.BaseMaxHealth,item.BaseClarity));
        }
        return result.AsReadOnly();
    }
}





