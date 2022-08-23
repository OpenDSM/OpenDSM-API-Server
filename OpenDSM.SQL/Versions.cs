using CLMath;
using MySql.Data.MySqlClient;
using System.Text;
using System.Xml.Linq;

namespace OpenDSM.SQL;

public static class Versions
{

    #region Public Methods

    public static void CreatePlatformVersion(Platform platform, string download_url, long version_id, long filesize)
    {
        using MySqlConnection conn = GetConnection();
        using MySqlCommand cmd = new($"insert into `platforms` (`version_id`, `platform_id`, `download_url`, `filesize`) values ('{version_id}', '{(byte)platform}', '{download_url}', '{filesize}')", conn);
        cmd.ExecuteNonQuery();
    }

    public static void CreateVersion(long git_id, int product_id, string name, byte releaseType, string changelog, DateTime posted)
    {
        using MySqlConnection conn = GetConnection();
        using MySqlCommand cmd = new($"INSERT INTO `versions` (`id`, `product_id`, `name`, `type`, `changelog`, `posted`) VALUES ('{git_id}','{product_id}', '{name}', '{releaseType}', '{CLConverter.EncodeBase64(changelog)}', '{posted:yyyy-MM-dd HH:mm:ss.fffffff}')", conn);
        cmd.ExecuteNonQuery();
    }

    public static bool GetPlatformVersionByID(byte platform_type, long version_id, out string download_url, out int total_downloads, out int weekly_downloads, out long filesize)
    {
        download_url = "";
        total_downloads = weekly_downloads = 0;
        filesize = 0;

        using MySqlConnection conn = GetConnection();
        using MySqlCommand cmd = new($"select * from `platforms` where `platform_id`='{platform_type}' and `version_id`='{version_id}'", conn);

        using MySqlDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            download_url = reader.GetString("download_url");
            total_downloads = reader.GetInt32("total_downloads");
            weekly_downloads = reader.GetInt32("weekly_downloads");
            filesize = reader.GetInt64("filesize");
            return true;
        }

        return false;
    }

    public static bool GetVersionByID(long id, int product_id, out string name, out byte releaseType, out byte[] platforms, out string changelog, out DateTime posted)
    {
        platforms = Array.Empty<byte>();
        changelog = name = "";
        releaseType = 0;
        posted = DateTime.Now;

        using MySqlConnection conn = GetConnection();
        using (MySqlCommand cmd = new($"select * from `versions` where `id`='{id}' and `product_id`='{product_id}'", conn))
        {
            using MySqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                name = reader.GetString("name");
                releaseType = reader.GetByte("type");
                posted = reader.GetDateTime("posted");
                changelog = CLConverter.DecodeBase64(reader.GetString("changelog"));
            }
        }
        using (MySqlCommand cmd = new($"select `platform_id` from `platforms` where `version_id`='{id}'", conn))
        {
            using MySqlDataReader reader = cmd.ExecuteReader();
            List<byte> platform_list = new();
            while (reader.Read())
            {
                platform_list.Add(reader.GetByte(0));
            }
            platforms = platform_list.ToArray();
        }
        return !string.IsNullOrEmpty(name) && platforms.Any();
    }

    public static long[] GetVersionsByProductID(int product_id)
    {
        try
        {
            using MySqlConnection conn = GetConnection();
            List<long> versions = new();
            using MySqlCommand cmd = new($"select id from versions where `product_id`='{product_id}'", conn);

            using MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                versions.Add(reader.GetInt64("id"));
            }
            return versions.ToArray();
        }
        catch (Exception e)
        {
            log.Error($"Unable to get list of versions for {product_id}", e.Message, e.StackTrace);
            return Array.Empty<long>();
        }
    }

    public static bool PlatformExists(long version_id, byte platform)
    {
        return GetPlatformVersionByID(platform, version_id, out _, out _, out _, out _);
    }

    public static bool RemoveVersion(int product_id, long version_id)
    {
        using MySqlConnection conn = GetConnection();
        using MySqlCommand cmd = new($"delete from versions where id={version_id} and product_id={product_id}", conn);

        return cmd.ExecuteNonQuery() > 0;
    }

    public static void UpdateVersion(long git_id, int product_id, string name, byte releaseType, string changelog)
    {
        using MySqlConnection conn = GetConnection();
        using MySqlCommand cmd = new($"update versions set name = '{name}', type = '{releaseType}', changelog = '{CLConverter.EncodeBase64(changelog)}' where id = {git_id} and product_id = {product_id}", conn);
        cmd.ExecuteNonQuery();
    }
    public static bool VersionExists(long id, int product_id)
    {
        return GetVersionByID(id, product_id, out _, out _, out _, out _, out _);
    }

    #endregion Public Methods

}
