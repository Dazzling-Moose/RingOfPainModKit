using RingOfPainModKit.SDK;
int passed=0;
void Check(bool condition,string name){if(!condition)throw new Exception(name);passed++;Console.WriteLine("PASS "+name);}
void Reject(Action action,string name){try{action();}catch(Exception){passed++;Console.WriteLine("PASS "+name);return;}throw new Exception("Did not reject: "+name);}
ModManifest Manifest(string id)=>new(){Id=id,Name=id,Version="1.0.0",Assembly="Example.dll",EntryType="Example.Plugin"};
var dependency=Manifest("example.base"); var child=Manifest("example.child"); child.Dependencies.Add(dependency.Id,"1.0.0");
Check(DependencyResolver.Order(new[]{child,dependency})[0].Id==dependency.Id,"dependencies initialize first");
Reject(()=>DependencyResolver.Order(new[]{child}),"missing dependency rejected");
dependency.Enabled=false;Reject(()=>DependencyResolver.Order(new[]{child,dependency}),"disabled dependency rejected");dependency.Enabled=true;
dependency.Version="0.9.0";Reject(()=>DependencyResolver.Order(new[]{child,dependency}),"old dependency rejected");dependency.Version="1.0.0";
dependency.Dependencies.Add(child.Id,"1.0.0");Reject(()=>DependencyResolver.Order(new[]{child,dependency}),"cycle rejected");dependency.Dependencies.Clear();
Reject(()=>DependencyResolver.Order(new[]{dependency,dependency}),"duplicate ID rejected");
foreach(var path in new[]{"../escape.dll","..\\escape.dll","C:\\escape.dll","module.exe"}){var invalid=Manifest("example.bad");invalid.Assembly=path;Reject(invalid.Validate,"invalid assembly path "+path);}
int errors=0,delivered=0;var policies=new StartingItemPolicies(_=>errors++);
using var bad=policies.SubscribeOpening(_=>throw new Exception("mod error"));
var good=policies.SubscribeOpening(_=>delivered++);policies.Publish(new StartingItemContext(0,"1.6.02f2"));
Check(errors==1&&delivered==1,"subscriber failure does not suppress later subscriber");good.Dispose();good.Dispose();policies.Publish(new StartingItemContext(0,"1.6.02f2"));
Check(delivered==1,"subscription disposal is idempotent and removes handler");
var lease=policies.AllowRarities(ItemRarity.Rare,ItemRarity.Legendary);Check(policies.IsAllowed(ItemRarity.Rare)&&!policies.IsAllowed(ItemRarity.Epic),"rarity allowance is exact");lease.Dispose();Check(!policies.IsAllowed(ItemRarity.Rare),"rarity allowance cleanup restores policy");
Reject(()=>policies.AllowRarities((ItemRarity)8),"flag value cannot be treated as rarity");

var testRoot=Path.Combine(Path.GetTempPath(),"ropmk-test-"+Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(testRoot);
try {
    var file=Path.Combine(testRoot,"probe.bin");File.WriteAllBytes(file,new byte[]{1,2,3});
    using var sha=System.Security.Cryptography.SHA256.Create();
    var expected=new Dictionary<string,string>{{"probe.bin",Convert.ToHexString(sha.ComputeHash(File.ReadAllBytes(file)))}};
    GameFingerprint.Verify(testRoot,expected);Check(true,"matching fingerprint accepted");
    File.WriteAllBytes(file,new byte[]{3,2,1});Reject(()=>GameFingerprint.Verify(testRoot,expected),"modified file fingerprint rejected");
    expected=new Dictionary<string,string>{{"missing.bin",new string('0',64)}};
    Reject(()=>GameFingerprint.Verify(testRoot,expected),"missing game file rejected");
    var released=0;var ctx=new ModContext("test",testRoot,policies,new EmptyItems(),_=>{});
    ctx.Track(new TestResource(()=>released++));ctx.ReleaseResources(_=>{});ctx.ReleaseResources(_=>{});
    Check(released==1,"tracked cleanup is repeatable without disposing resource twice");
} finally {
    // Delete only test-owned files, never recursively remove a computed directory.
    foreach(var f in Directory.GetFiles(testRoot))File.Delete(f);Directory.Delete(testRoot);
}

Console.WriteLine($"{passed} managed SDK checks passed. No gameplay behavior is verified by these checks.");

sealed class TestResource:IDisposable {private readonly Action action;public TestResource(Action a)=>action=a;public void Dispose()=>action();}
sealed class EmptyItems:IItemAPI {public IReadOnlyList<ItemSnapshot> GetRegisteredItems()=>Array.Empty<ItemSnapshot>();}
