using ChaseLabs.CLConfiguration;

namespace OpenDSM.Core.Handlers;
public sealed class ConfigHandler
{
    public static ConfigHandler Instance = Instance ??= new();
    private ConfigManager manager;
    public string Salt { get => manager.GetOrCreate("salt", Guid.NewGuid().ToString()).Value; set => manager.GetOrCreate("salt", Guid.NewGuid().ToString()).Value = value; }
    private ConfigHandler()
    {
        manager = new ConfigManager("app", RootDirectory);
    }
}