namespace RingOfPainModKit.SDK;

public interface IRingOfPainMod : IDisposable
{
    void Initialize(ModContext context);
}

public enum ItemRarity { None=0, Common=1, Rare=2, Epic=3, Legendary=4 }
public sealed record ItemSnapshot(int TypeId, int Guid, string Name, ItemRarity Rarity, int SlotId, int BaseAttack, int BaseDefense, int BaseSpeed, int BaseHealth, int BaseClarity);
public sealed record StartingItemContext(int PreviousItemType, string GameVersion);

public interface IItemAPI
{
    IReadOnlyList<ItemSnapshot> GetRegisteredItems();
}

public interface IStartingItemAPI
{
    IDisposable SubscribeOpening(Action<StartingItemContext> callback);
    IDisposable AllowRarities(params ItemRarity[] rarities);
}

public sealed class ModContext
{
    private readonly List<IDisposable> resources = new();
    public string Id { get; }
    public string ConfigurationDirectory { get; }
    public IStartingItemAPI StartingItems { get; }
    public IItemAPI Items { get; }
    public Action<string> Log { get; }
    public ModContext(string id, string configurationDirectory, IStartingItemAPI startingItems, IItemAPI items, Action<string> log)
        => (Id,ConfigurationDirectory,StartingItems,Items,Log)=(id,configurationDirectory,startingItems,items,log);
    public T Track<T>(T resource) where T:IDisposable { resources.Add(resource); return resource; }
    public T ReadConfiguration<T>() where T:new()
    {
        Directory.CreateDirectory(ConfigurationDirectory);
        var path=Path.Combine(ConfigurationDirectory,"config.json");
        if(File.Exists(path)) return System.Text.Json.JsonSerializer.Deserialize<T>(File.ReadAllText(path)) ?? throw new InvalidDataException("Configuration cannot be null.");
        var value=new T();
        File.WriteAllText(path,System.Text.Json.JsonSerializer.Serialize(value,new System.Text.Json.JsonSerializerOptions { WriteIndented=true }));
        return value;
    }
    public void ReleaseResources(Action<Exception> report)
    {
        foreach(var resource in resources.AsEnumerable().Reverse()) try { resource.Dispose(); } catch(Exception ex) { report(ex); }
        resources.Clear();
    }
}
