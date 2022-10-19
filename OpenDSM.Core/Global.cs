// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
global using static OpenDSM.Core.Global;
using ChaseLabs.CLLogger;
using ChaseLabs.CLLogger.Interfaces;
using HashidsNet;
using OpenDSM.Core.Handlers;
using OpenDSM.SQL;

namespace OpenDSM.Core;

public static class Global
{

    #region Public Fields

    public static IHashids HashIds = new Hashids(ConfigHandler.Instance.Salt, 11);
    public static string InstallLocation = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location)?.FullName ?? "";
    public static ILog log = LogManager.Init().SetDumpMethod(1000).SetLogDirectory(LogsDirectory).SetPattern("[OPENDSM] (%TYPE%: %DATE%): %MESSAGE%");
    public static ILog log_access = LogManager.Init().SetDumpMethod(1000).SetLogDirectory(AccessLogsDirectory).SetPattern("[ACCESS] (%TYPE%: %DATE%): %MESSAGE%");
    public static ILog log_admin = LogManager.Init().SetDumpMethod(1000).SetLogDirectory(AccessLogsDirectory).SetPattern("[ADMIN] (%TYPE%: %DATE%): %MESSAGE%");
    public static ILog log_sales = LogManager.Init().SetDumpMethod(1000).SetLogDirectory(SalesLogsDirectory).SetPattern("[SALES] (%TYPE%: %DATE%): %MESSAGE%");
    public static ILog log_user = LogManager.Init().SetDumpMethod(1000).SetLogDirectory(UserLogsDirectory).SetPattern("[USER] (%TYPE%: %DATE%): %MESSAGE%");

    public static string wwwroot = "";

    #endregion Public Fields

    #region Public Properties

    public static string AccessLogsDirectory => Directory.CreateDirectory(Path.Combine(LogsDirectory, "Access")).FullName;
    public static string AdminLogsDirectory => Directory.CreateDirectory(Path.Combine(LogsDirectory, "Admin")).FullName;
    public static string ApplicationName => "OpenDSM";
    public static string CompanyName => "LFInteractive";
    public static string Copywrite => $"All Rights Reserved - {CompanyName} LLC. (c) 2021-{DateTime.Now.Year}";
    public static string FFMpegDirectory => Directory.CreateDirectory(Path.Combine(RootDirectory, "FFMpeg")).FullName;
    public static string LogsDirectory => Directory.CreateDirectory(Path.Combine(RootDirectory, "Logs")).FullName;
    public static string ProductDataDirectory => Directory.CreateDirectory(Path.Combine(RootDirectory, "ProductData")).FullName;
    public static string ProfileDataDirectory => Directory.CreateDirectory(Path.Combine(RootDirectory, "ProfileData")).FullName;
    public static string RootDirectory => Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), CompanyName, ApplicationName)).FullName;
    public static string SalesLogsDirectory => Directory.CreateDirectory(Path.Combine(LogsDirectory, "Sales")).FullName;
    public static string UserLogsDirectory => Directory.CreateDirectory(Path.Combine(LogsDirectory, "Users")).FullName;

    #endregion Public Properties

    #region Public Methods

    public static string GetProductDirectory(int id) => Directory.CreateDirectory(Path.Combine(ProductDataDirectory, id.ToString())).FullName;

    public static string GetUsersProfileDirectory(int id) => Directory.CreateDirectory(Path.Combine(ProfileDataDirectory, id.ToString())).FullName;

    public static void InitHandlers()
    {
        _ = ConfigHandler.Instance;
        //_ = PaymentHandler.Instance;
        _ = Connections.Instance;
        if (Requests.TableExists("test"))
        {
            log.Debug("I");
        }
        else
        {
            log.Debug("NOPE");

        }
    }


    #endregion Public Methods

}