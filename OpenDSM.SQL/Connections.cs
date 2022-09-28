// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
global using static OpenDSM.SQL.Connections;
using ChaseLabs.CLConfiguration;
using ChaseLabs.CLLogger;
using ChaseLabs.CLLogger.Interfaces;
using MySql.Data.MySqlClient;
namespace OpenDSM.SQL;

public class Connections
{

    #region Public Fields

    public static Connections Instance = Instance ??= new();
    public static ILog log = LogManager.Init().SetDumpMethod(DumpType.NoDump).SetPattern("[DATABASE] (%TYPE%: %DATE%): %MESSAGE%");

    #endregion Public Fields

    #region Internal Fields

    internal string ConnectionString;

    #endregion Internal Fields

    #region Private Fields

    private ConfigManager manager;

    #endregion Private Fields

    #region Private Constructors

    private Connections()
    {
        log = LogManager.Init().SetDumpMethod(DumpType.NoDump).SetPattern("[DATABASE] (%TYPE%: %DATE%): %MESSAGE%");
        manager = new("db", Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LFInteractive", "OpenDSM")).FullName);
        ConnectionString = GetConnectionString();
    }


    #endregion Private Constructors

    #region Public Methods

    public string GetConnectionString(int tries = 0)
    {
        string username = GetUsername();
        string password = GetDatabasePassword();
        int port = GetPort();
        string connection = $"datasource=localhost;port={port};username={username};password={password};database=opendsm;";
        if (!TestConnection(connection))
        {
            if (tries > 5)
            {
                log.Fatal($"Unable to verify the connection string", "Exiting!");
                Environment.Exit(1);
            }
            log.Error($"Invalid Connection String: `{connection}`", "Retrying in 5 seconds!");
            Thread.Sleep(1000 * 5);
            return GetConnectionString(tries + 1);
        }
        log.Info("Successfully connected to Database!");
        return connection;
    }

    public bool TestConnection() => TestConnection(Instance.ConnectionString, out string _);

    public bool TestConnection(string connection_string) => TestConnection(connection_string, out string _);

    public bool TestConnection(out string version) => TestConnection(Instance.ConnectionString, out version);

    public bool TestConnection(string connection_string, out string version)
    {
        bool success = true;
        version = "";
        MySqlConnection? conn = null;
        try
        {
            conn = new(connection_string);
            conn.Open();

            using MySqlCommand cmd = new("select version()", conn);
            object scalar = cmd.ExecuteScalar();
            if (scalar != null && !string.IsNullOrWhiteSpace(scalar.ToString()))
            {
                version = scalar.ToString() ?? "";
            }
            else
            {
                success = false;
            }
        }
        catch
        {
            success = false;
        }
        finally
        {
            if (conn != null)
                conn.Close();

        }
        return success;
    }

    #endregion Public Methods

    #region Private Methods

    private string GetDatabasePassword()
    {
        string password = manager.GetOrCreate("password", "").Value;

        if (string.IsNullOrWhiteSpace(password))
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Enter Database Password >> ");
            Console.ResetColor();
            password = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(password))
                return GetDatabasePassword();
            manager.GetOrCreate("password", "").Value = password;
        }

        return password.ToLower() == "_blank_" ? "" : password;
    }

    private int GetPort()
    {
        int port = manager.GetOrCreate("port", -1).Value;

        if (port == -1)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Enter Database Port >> ");
            Console.ResetColor();
            string s_port = Console.ReadLine() ?? "";
            if (!int.TryParse(s_port, out port))
                return GetPort();
            manager.GetOrCreate("port", "").Value = port;
        }

        return port;
    }

    private string GetUsername()
    {
        string username = manager.GetOrCreate("username", "").Value;

        if (string.IsNullOrWhiteSpace(username))
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Enter Database Username >> ");
            Console.ResetColor();
            username = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(username))
                return GetUsername();
            manager.GetOrCreate("username", "").Value = username;
        }

        return username;
    }

    #endregion Private Methods
}