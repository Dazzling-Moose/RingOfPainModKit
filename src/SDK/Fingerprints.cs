using System.Security.Cryptography;
namespace RingOfPainModKit.SDK;
/// <summary>Exact supported-build integrity check; mismatch throws before game patches are installed.</summary>
public static class GameFingerprint
{
    public static void Verify(string gameRoot,IReadOnlyDictionary<string,string> expected)
    {
        foreach(var entry in expected)
        {
            using var stream=File.OpenRead(Path.Combine(gameRoot,entry.Key));
            using var algorithm=SHA256.Create();
            var actual=Convert.ToHexString(algorithm.ComputeHash(stream));
            if(!actual.Equals(entry.Value,StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Unsupported/modified game file: "+entry.Key);
        }
    }
}
