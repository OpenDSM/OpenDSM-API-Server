using ChaseLabs.CLConfiguration;
using HashidsNet;
using OpenDSM.Core.Handlers;

namespace OpenDSM.Security;

public class HashIds
{
    public static HashIds Instance = Instance ??= new();
    private readonly IHashids hash;
    private HashIds()
    {
        ConfigManager manager = new("sec", Core.Global.RootDirectory);
        hash = new Hashids(manager.GetOrCreate("hash_key", Guid.NewGuid().ToString().Replace("-", "")).Value, 11);
    }
    public string Get(int id) => hash.Encode(id);
    public int Get(string key) => hash.DecodeSingle(key);
}
