using RingOfPainModKit.SDK;
namespace DoubleCandleAllItems;
public sealed class Mod:IRingOfPainMod
{
    public sealed class Configuration { public bool AllowAllRarities {get;set;}=true; }
    public void Initialize(ModContext context)
    {
        if(!context.ReadConfiguration<Configuration>().AllowAllRarities){context.Log("Rarity override disabled by config.");return;}
        context.Track(context.StartingItems.AllowRarities(ItemRarity.Common,ItemRarity.Rare,ItemRarity.Epic,ItemRarity.Legendary));
        context.Track(context.StartingItems.SubscribeOpening(_=> {
            context.Log("Double Candle chooser opened; all-rarity policy active.");
            var items=context.Items.GetRegisteredItems();
            context.Log("ROPMK_ITEM_READ_PASS: "+items.Count+" registered pool entries; "+string.Join(", ",items.GroupBy(i=>i.Rarity).Select(g=>g.Key+"="+g.Count())));
        }));
        context.Log("All-rarity policy registered; locked/unseen restrictions retained.");
    }
    public void Dispose() { } // Host disposes resources tracked through ModContext.
}
