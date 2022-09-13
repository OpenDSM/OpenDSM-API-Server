using System.Text;
using MySql.Data.MySqlClient;

namespace OpenDSM.SQL;
public record APIKey(int user, string key, int total_calls, DateTime created, DateTime last_used);
public static class API
{
    public static int GetNumberOfAPIKeysAllocated()
    {
        using MySqlConnection conn = GetConnection();
        using MySqlCommand cmd = new($"select count(*) from api_keys", conn);
        using MySqlDataReader reader = cmd.ExecuteReader();
        if (reader.HasRows)
        {
            return reader.GetInt32(0);
        }
        return 0;
    }
    public static string GenerateAPIKey(int user)
    {
        Guid guid = Guid.NewGuid();
        string key = guid.ToString().Replace("-", "");
        if (GetUserWithAPIKey(key) != -1)
        {
            return GenerateAPIKey(user);
        }
        bool exists = false;
        using (MySqlConnection conn = GetConnection())
        {
            using MySqlCommand cmd = new($"select user_id from api_keys where `user_id`='{user}' limit 1", conn);
            using MySqlDataReader reader = cmd.ExecuteReader();
            exists = reader.HasRows;
        }
        using (MySqlConnection conn = GetConnection())
        {
            string sql = $"insert into api_keys (`user_id`, `key`) values ('{user}', '{key}')";
            if (exists)
            {
                sql = $"update api_keys set `key`='{key}' where `user_id`='{user}' limit 1";
            }
            using MySqlCommand cmd = new(sql, conn);
            cmd.ExecuteNonQuery();
        }
        return key;
    }

    public static string GetUsersAPIKey(int user){

        using MySqlConnection conn = GetConnection();
        using MySqlCommand cmd = new($"select `key` from api_keys where `user_id`='{user}'", conn);
        using MySqlDataReader reader = cmd.ExecuteReader();
        if (reader.HasRows && reader.Read())
        {
            return reader.GetString(0);
        }
        return "";
    }

    public static int GetUserWithAPIKey(string key)
    {
        using MySqlConnection conn = GetConnection();
        using MySqlCommand cmd = new($"select user_id from api_keys where `key`='{key}' limit 1", conn);
        using MySqlDataReader reader = cmd.ExecuteReader();
        if (reader.HasRows)
        {
            return reader.GetInt32(0);
        }
        return -1;
    }

    public static APIKey GetAPIKey(string key)
    {
        using MySqlConnection conn = GetConnection();
        using MySqlCommand cmd = new($"select * from api_keys where `key`='{key}' limit 1", conn);
        using MySqlDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new(reader.GetInt32("user_id"), reader.GetString("key"), reader.GetInt32("total_calls"), reader.GetDateTime("created"), reader.GetDateTime("last_used"));
        }
        return new(-1, "", 0, DateTime.Now, DateTime.Now);
    }

    public static void IncrementCall(string key)
    {

        using MySqlConnection conn = GetConnection();
        string sql = $"update api_keys set `total_calls`='{GetAPIKey(key).total_calls + 1}', `last_used`='{DateTime.Now:yyyy-MM-dd HH:mm:ss}' where `key`='{key}' limit 1";
        log.Info(sql);
        using MySqlCommand cmd = new(sql, conn);
        cmd.ExecuteNonQuery();
    }


}