using System.Text.Json;
using System.Text.RegularExpressions;

namespace RingOfPainModKit.SDK;

public sealed class ModManifest
{
    public string Id { get; set; }="";
    public string Name { get; set; }="";
    public string Version { get; set; }="";
    public string Assembly { get; set; }="";
    public string EntryType { get; set; }="";
    public bool Enabled { get; set; }=true;
    public string GameVersion { get; set; }="1.6.02f2";
    public string MinimumKitVersion { get; set; }="0.1.0";
    public Dictionary<string,string> Dependencies { get; set; }=new();
    public static ModManifest Read(string path)
    {
        var result=JsonSerializer.Deserialize<ModManifest>(File.ReadAllText(path),new JsonSerializerOptions { PropertyNameCaseInsensitive=true }) ?? throw new InvalidDataException("Manifest cannot be null.");
        result.Validate(); return result;
    }
    public void Validate()
    {
        if(!Regex.IsMatch(Id??"",@"^[a-z0-9]+(?:[.-][a-z0-9]+)*$")) throw new InvalidDataException("ID must contain lowercase letters, digits, dots, or hyphens.");
        if(string.IsNullOrWhiteSpace(Name)||string.IsNullOrWhiteSpace(EntryType)) throw new InvalidDataException("Name and entry type are required.");
        if(!System.Version.TryParse(Version,out _)||!System.Version.TryParse(MinimumKitVersion,out _)) throw new InvalidDataException("Versions must be numeric dotted versions, such as 0.1.0.");
        if(string.IsNullOrEmpty(Assembly)||Path.GetFileName(Assembly)!=Assembly||Assembly.IndexOfAny(new[]{'/','\\',':'})>=0||!Assembly.EndsWith(".dll",StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Assembly must be a DLL filename inside the mod directory.");
        if(Dependencies is null) throw new InvalidDataException("Dependencies cannot be null.");
        foreach(var dependency in Dependencies) if(!System.Version.TryParse(dependency.Value,out _)) throw new InvalidDataException("Dependency minimum version is invalid.");
    }
}

public static class DependencyResolver
{
    public static IReadOnlyList<ModManifest> Order(IEnumerable<ModManifest> manifests)
    {
        var all=manifests.Where(m=>m.Enabled).ToDictionary(m=>m.Id,StringComparer.Ordinal);
        var visited=new HashSet<string>(); var visiting=new HashSet<string>(); var result=new List<ModManifest>();
        void Visit(ModManifest mod)
        {
            if(visited.Contains(mod.Id)) return;
            if(!visiting.Add(mod.Id)) throw new InvalidDataException($"Dependency cycle involving {mod.Id}.");
            foreach(var pair in mod.Dependencies.OrderBy(p=>p.Key,StringComparer.Ordinal))
            {
                if(!all.TryGetValue(pair.Key,out var dependency)) throw new InvalidDataException($"{mod.Id} requires missing/disabled mod {pair.Key}.");
                if(System.Version.Parse(dependency.Version)<System.Version.Parse(pair.Value)) throw new InvalidDataException($"{mod.Id} requires {pair.Key} >= {pair.Value}.");
                Visit(dependency);
            }
            visiting.Remove(mod.Id); visited.Add(mod.Id); result.Add(mod);
        }
        foreach(var mod in all.Values.OrderBy(m=>m.Id,StringComparer.Ordinal)) Visit(mod);
        return result;
    }
}
