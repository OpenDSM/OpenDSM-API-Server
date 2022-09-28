using System.Text;
using System.Xml.Linq;
using CLMath;
using MySql.Data.MySqlClient;

namespace OpenDSM.SQL;

public static class Versions
{
    #region Public Methods

    public static void CreatePlatformVersion(Platform platform, string download_url, long version_id, long filesize)
    {
        Insert(
            table: "platform",
            items: new KeyValuePair<string, dynamic>[]
            {
                new("version_id", version_id),
                new("platform_id", (byte)platform),
                new("download_url", download_url),
                new("filesize", filesize)
            }
        );
    }

    public static void CreateVersion(long git_id, int product_id, string name, byte releaseType, string changelog, DateTime posted)
    {
        Insert(
            table: "versions",
            items: new KeyValuePair<string, dynamic>[]
            {
                new("id", git_id),
                new("product_id", product_id),
                new("name", name),
                new("type", releaseType),
                new("changelog", CLConverter.EncodeBase64(changelog)),
                new("posted", posted.ToString("yyyy-MM-dd HH:mm:ss.fffffff"))
            }
        );
    }

    public static bool GetPlatformVersionByID(byte platform_type, long version_id, out string download_url, out int total_downloads, out int weekly_downloads, out long filesize)
    {
        download_url = "";
        total_downloads = weekly_downloads = 0;
        filesize = 0;

        using MySqlDataReader reader = Select(
            table: "platforms",
            column: "*",
            where: new(new IndividualWhereClause[]
            {
                new("platform_id", platform_type, "="),
                new("version_id", version_id, "=")
            })
        );

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

        using (MySqlDataReader reader = Select(
            table: "versions",
            column: "*",
            where: new(new IndividualWhereClause[]
            {
                new("id", id, "="),
                new("product_id", product_id, "="),
            })
        ))
        {
            if (reader.Read())
            {
                name = reader.GetString("name");
                releaseType = reader.GetByte("type");
                posted = reader.GetDateTime("posted");
                changelog = CLConverter.DecodeBase64(reader.GetString("changelog"));
            }
        }
        using (MySqlDataReader reader = Select(
            table: "platforms",
            column: "platform_id",
            where: new(new IndividualWhereClause[]
            {
                new("version_id", id, "=")
            })
        ))
        {
            List<byte> platform_list = new();
            while (reader.Read())
            {
                platform_list.Add(reader.GetByte(0));
            }
            platforms = platform_list.ToArray();
        }
        return !string.IsNullOrEmpty(name) && platforms.Any();
    }

    public static long[] GetVersionsByProductID(int product_id, int count, int page)
    {
        try
        {
            List<long> versions = new();

            using MySqlDataReader reader = Select(
                table: "versions",
                column: "id",
                where: new(new IndividualWhereClause[]
                {
                    new("product_id", product_id, "=")
                }),
                orderby: new("posted"),
                limit: count,
                offset: count * page
            );
            while (reader.Read())
            {
                versions.Add(reader.GetInt64(0));
            }
            return versions.ToArray();
        }
        catch (Exception e)
        {
            log.Error($"Unable to get list of versions for {product_id}", e.Message, e.StackTrace ?? "");
            return Array.Empty<long>();
        }
    }

    public static bool PlatformExists(long version_id, byte platform)
    {
        return GetPlatformVersionByID(platform, version_id, out _, out _, out _, out _);
    }

    public static bool RemoveVersion(int product_id, long version_id)
    {
        return Delete(
            table: "versions",
            where: new(new IndividualWhereClause[]
            {
                new("id", version_id, "="),
                new("product_id", product_id, "=")
            })
        );
    }

    public static void UpdateVersion(long git_id, int product_id, string name, byte releaseType, string changelog)
    {
        Update(
            table: "versions",
            items: new KeyValuePair<string, dynamic>[]
            {
                new("name", name),
                new("type", releaseType),
                new("changelog", CLConverter.EncodeBase64(changelog)),
            },
            where: new(new IndividualWhereClause[]
            {
                new("id", git_id, "="),
                new("product_id", product_id, "="),
            })
        );
    }
    public static bool VersionExists(long id, int product_id)
    {
        return GetVersionByID(id, product_id, out _, out _, out _, out _, out _);
    }

    #endregion Public Methods

}
