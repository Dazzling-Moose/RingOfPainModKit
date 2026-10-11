using MelonLoader;
[assembly: MelonInfo(typeof(RingOfPainModKit.MelonSmokeTest.Plugin), "RingOfPainModKit Smoke Test", "0.1.0", "Community")]
[assembly: MelonGame("Twice Different", "Ring Of Pain")]
namespace RingOfPainModKit.MelonSmokeTest;
public sealed class Plugin:MelonMod
{
    public override void OnInitializeMelon()=>LoggerInstance.Msg("ROPMK_SMOKE_PASS: basic IL2CPP plugin loaded; no gameplay hooks installed.");
}
