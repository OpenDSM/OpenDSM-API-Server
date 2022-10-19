using System.Text;
using MySql.Data.MySqlClient;

namespace OpenDSM.SQL;
public record APIKey(int user, string key, int total_calls, DateTime created, DateTime last_used);
public static class API
{
    #region Public Methods

    public static string GenerateAPIKey(int user)
    {
        Guid guid = Guid.NewGuid();
        string key = guid.ToString().Replace("-", "");
        if (GetUserWithAPIKey(key) != -1)
        {
            return GenerateAPIKey(user);
        }
        bool exists = Select(
            table: table,
            column: "user_id",
            where: new(new IndividualWhereClause[]{
                new("user_id", user, "=")
            }),
            limit: 1
        ).HasRows;
        if (exists)
        {
            Update(
                table: table,
                items: new KeyValuePair<string, dynamic>[]{
                    new("key", key)
                },
                limit: 1,
                where: new(new IndividualWhereClause[]{
                    new("user_id", user, "=")
                })
            );
        }
        else
        {
            Insert(
                table: table,
                items: new KeyValuePair<string, dynamic>[]{
                    new("user_id", user),
                    new("key", key),
                }
            );
        }
        return key;
    }

    public static APIKey GetAPIKey(string key)
    {
        using MySqlDataReader reader = Select(
            table: table,
            column: "*",
            where: new(new IndividualWhereClause[]{
                new("key", key, "=")
            }),
            limit: 1
        );
        if (reader.Read())
        {
            return new(reader.GetInt32("user_id"), reader.GetString("key"), reader.GetInt32("total_calls"), reader.GetDateTime("created"), reader.GetDateTime("last_used"));
        }
        return new(-1, "", 0, DateTime.Now, DateTime.Now);
    }

    public static int GetNumberOfAPIKeysAllocated()
    {
        using MySqlDataReader reader = Select(
            table: table,
            column: "count(*)",
            where: null
        );
        if (reader.HasRows)
        {
            return reader.GetInt32(0);
        }
        return 0;
    }

    public static string GetUsersAPIKey(int user)
    {
        using MySqlDataReader reader = Select(
            table: table,
            column: "key",
            where: new(new IndividualWhereClause[]{
                new("user_id", user, "=")
            }),
            limit: 1
        );
        if (reader.HasRows && reader.Read())
        {
            return reader.GetString(0);
        }
        return "";
    }

    public static int GetUserWithAPIKey(string key)
    {
        using MySqlDataReader reader = Select(
            table: table,
            column: "user_id",
            where: new(new IndividualWhereClause[]{
                new("key", key, "=")
            }),
            limit: 1
        );
        if (reader.HasRows)
        {
            return reader.GetInt32(0);
        }
        return -1;
    }

    public static void IncrementCall(string key)
    {
        Update(
            table: table,
            items: new KeyValuePair<string, dynamic>[]
            {
                new("total_calls", GetAPIKey(key).total_calls + 1),
                new("last_used", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
            },
            where: new(new IndividualWhereClause[]{
                new("key", key, "=")
            }),
            limit: 1
        );
    }

    #endregion Public Methods

    #region Private Fields

    private static readonly string table = "api_keys";

    #endregion Private Fields
}