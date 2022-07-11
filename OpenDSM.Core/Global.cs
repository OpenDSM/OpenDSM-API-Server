using ChaseLabs.CLLogger;
using ChaseLabs.CLLogger.Interfaces;

namespace OpenDSM.Core;

public static class Global
{
    public static ILog log = LogManager.Init().SetDumpMethod(1000).SetLogDirectory(LogsDirectory);
    public static ILog log_user = LogManager.Init().SetDumpMethod(1000).SetLogDirectory(UserLogsDirectory);
    public static ILog log_access = LogManager.Init().SetDumpMethod(1000).SetLogDirectory(AccessLogsDirectory);
    public static ILog log_admin = LogManager.Init().SetDumpMethod(1000).SetLogDirectory(AccessLogsDirectory);
    public static ILog log_sales = LogManager.Init().SetDumpMethod(1000).SetLogDirectory(SalesLogsDirectory);
    public static string ApplicationName => "OpenDSM";
    public static string CompanyName => "LFInteractive";
    public static string Copywrite => $"All Rights Reserved - {CompanyName} LLC. (c) 2021-{DateTime.Now.Year}";
    public static string RootDirectory => Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), CompanyName, ApplicationName)).FullName;
    public static string LogsDirectory => Directory.CreateDirectory(Path.Combine(RootDirectory, "Logs")).FullName;
    public static string UserLogsDirectory => Directory.CreateDirectory(Path.Combine(LogsDirectory, "Users")).FullName;
    public static string AccessLogsDirectory => Directory.CreateDirectory(Path.Combine(LogsDirectory, "Access")).FullName;
    public static string AdminLogsDirectory => Directory.CreateDirectory(Path.Combine(LogsDirectory, "Admin")).FullName;
    public static string SalesLogsDirectory => Directory.CreateDirectory(Path.Combine(LogsDirectory, "Sales")).FullName;
}