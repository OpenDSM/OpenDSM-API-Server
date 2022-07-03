global using static OpenDSM.Lib.Data.Global;

using ChaseLabs.CLLogger;
using ChaseLabs.CLLogger.Interfaces;
using OpenDSM.Lib.Auth;
using OpenDSM.Lib.Objects;

namespace OpenDSM.Lib.Data;

public static class Global
{
    #region Fields

    public static ILog log = LogManager.Init().SetDumpMethod(3000).SetMinimumLogType(Lists.LogTypes.All).SetLogDirectory(LogDirectory);

    private static string tos = "";

    #endregion Fields

    #region Properties

    public static string ConfigDirectory => Directory.CreateDirectory(Path.Combine(RootDirectory, "config")).FullName;
    public static string LogDirectory => Directory.CreateDirectory(Path.Combine(RootDirectory, "logs")).FullName;
    public static string ProductsDirectory => Directory.CreateDirectory(Path.Combine(RootDirectory, "products")).FullName;
    public static string RootDirectory => Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LFInteractive", "OpenDSM")).FullName;
    public static string UsersDirectory => Directory.CreateDirectory(Path.Combine(RootDirectory, "users")).FullName;

    #endregion Properties

    #region Public Methods

    public static string GetTOS()
    {
        if (string.IsNullOrEmpty(tos))
        {
            using StreamReader reader = new(new FileStream(Path.Combine(RootDirectory, "tos.txt"), FileMode.OpenOrCreate, FileAccess.Read));
            tos = reader.ReadToEnd();
        }
        return tos;
    }

    public static Task InitiateStartupProceedure()
    {
        return Task.Run(() =>
        {
            Products.Instance.Load();
            AccountManagement.Instance.Load();
        });
    }

    #endregion Public Methods
}