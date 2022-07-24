// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
global using static OpenDSM.Core.Global;
using ChaseLabs.CLLogger;
using ChaseLabs.CLLogger.Interfaces;

namespace OpenDSM.Core;

public static class Global
{
    #region Public Fields

    public static ILog log = LogManager.Init().SetDumpMethod(1000).SetLogDirectory(LogsDirectory);
    public static ILog log_access = LogManager.Init().SetDumpMethod(1000).SetLogDirectory(AccessLogsDirectory);
    public static ILog log_admin = LogManager.Init().SetDumpMethod(1000).SetLogDirectory(AccessLogsDirectory);
    public static ILog log_sales = LogManager.Init().SetDumpMethod(1000).SetLogDirectory(SalesLogsDirectory);
    public static ILog log_user = LogManager.Init().SetDumpMethod(1000).SetLogDirectory(UserLogsDirectory);

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


    #endregion Public Methods
}