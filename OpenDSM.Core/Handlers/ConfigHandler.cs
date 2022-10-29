using ChaseLabs.CLConfiguration;

namespace OpenDSM.Core.Handlers;
public sealed class ConfigHandler
{

    #region Public Fields

    public static ConfigHandler Instance = Instance ??= new();

    #endregion Public Fields

    #region Public Properties

    public string Salt { get => manager.GetOrCreate("salt", Guid.NewGuid().ToString()).Value; set => manager.GetOrCreate("salt", Guid.NewGuid().ToString()).Value = value; }

    #endregion Public Properties

    #region Private Constructors

    private ConfigHandler()
    {
        manager = new ConfigManager("app", RootDirectory);
    }

    #endregion Private Constructors

    #region Private Fields

    private ConfigManager manager;

    #endregion Private Fields

}