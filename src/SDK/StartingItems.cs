namespace RingOfPainModKit.SDK;

/// <summary>Managed policy/event implementation. The game adapter supplies context and applies policies.</summary>
public sealed class StartingItemPolicies : IStartingItemAPI
{
    private readonly object gate=new();
    private readonly Dictionary<Guid,Action<StartingItemContext>> callbacks=new();
    private readonly Dictionary<Guid,HashSet<ItemRarity>> allowances=new();
    private readonly Action<Exception> error;
    public StartingItemPolicies(Action<Exception> reportError) => error=reportError;
    public IDisposable SubscribeOpening(Action<StartingItemContext> callback)
    {
        ArgumentNullException.ThrowIfNull(callback); var id=Guid.NewGuid();
        lock(gate) callbacks.Add(id,callback);
        return new Lease(()=> { lock(gate) callbacks.Remove(id); });
    }
    public IDisposable AllowRarities(params ItemRarity[] rarities)
    {
        ArgumentNullException.ThrowIfNull(rarities);
        if(rarities.Any(r=>r<ItemRarity.Common||r>ItemRarity.Legendary)) throw new ArgumentOutOfRangeException(nameof(rarities));
        var id=Guid.NewGuid(); lock(gate) allowances.Add(id,new HashSet<ItemRarity>(rarities));
        return new Lease(()=> { lock(gate) allowances.Remove(id); });
    }
    public bool IsAllowed(ItemRarity rarity) { lock(gate) return allowances.Values.Any(set=>set.Contains(rarity)); }
    public void Publish(StartingItemContext context)
    {
        Action<StartingItemContext>[] snapshot; lock(gate) snapshot=callbacks.Values.ToArray();
        foreach(var callback in snapshot) try { callback(context); } catch(Exception ex) { error(ex); }
    }
    private sealed class Lease:IDisposable
    {
        private Action? release; public Lease(Action action)=>release=action;
        public void Dispose()=>Interlocked.Exchange(ref release,null)?.Invoke();
    }
}
