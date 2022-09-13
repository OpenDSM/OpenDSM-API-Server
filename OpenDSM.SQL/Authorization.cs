// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using System.Text;
using CLMath;
using MySql.Data.MySqlClient;

namespace OpenDSM.SQL;

public enum AccountType
{
    User,
    Seller,
    Admin,
}

public enum FailedReason
{
    None,
    InvalidUsernameOrEmail,
    InvalidUsername,
    InvalidPassword,
    InvalidToken,
    InvalidEmail,
    UserAlreadyExists,
    UsernameAlreadyTaken,
    EmailAlreadyTaken,
    AccountDisabled,
}

public record User(int id, string username, string email, string token, AccountType type, bool use_git_readme, string git_username, string git_token, int[] owned_products);

public static class Authorization
{

    #region Public Methods

    public static bool CheckUserExists(string username)
    {
        try
        {
            using MySqlConnection conn = GetConnection();
            MySqlCommand cmd = new($"select id from users where `username` = '{username}' or `email` = '{username}'", conn);
            return cmd.ExecuteReader().HasRows;
        }
        catch (Exception ex)
        {
            log.Error($"Unable to check if user exists", ex);
            return false;
        }
    }

    public static bool GetUserFromID(int id, out User user)
    {
        string username = "";
        string email = "";
        string git_username = "";
        string git_token = "";
        AccountType type = AccountType.User;
        int[] owned_products = Array.Empty<int>();
        bool use_git_readme = false;
        user = null;
        try
        {
            using MySqlConnection conn = GetConnection();
            using MySqlCommand cmd = new($"select * from users where id = '{id}'", conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    username = reader.GetString("username");
                    email = reader.GetString("email");
                    use_git_readme = reader.GetBoolean("use_git_readme");
                    try
                    {
                        git_username = reader.GetString("git_username");
                    }
                    catch
                    {
                        git_username = "";
                    }
                    try
                    {
                        git_token = reader.GetString("git_token");

                    }
                    catch
                    {
                        git_token = "";
                    }

                    type = reader.GetByte("type") switch
                    {
                        1 => AccountType.Seller,
                        2 => AccountType.Admin,
                        _ => AccountType.User
                    };
                    try
                    {
                        owned_products = Array.ConvertAll(reader.GetString("owned_product_ids").Split(";", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries), i => Convert.ToInt32(i));
                    }
                    catch
                    {
                        owned_products = Array.Empty<int>();
                    }
                    user = new(id, username, email, "", type, use_git_readme, git_username, git_token, owned_products);
                    return true;
                }
            }

        }
        catch (Exception e)
        {
            log.Error($"Unable to query users from sql: {id}", e.Message, e.StackTrace ?? "");
        }
        return false;
    }

    public static bool GetUserFromUsername(string username, out User user)
    {
        using MySqlConnection conn = GetConnection();
        MySqlCommand cmd = new($"select id from users where username = '{username}' limit 1", conn);
        MySqlDataReader reader = cmd.ExecuteReader();
        user = null;
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                return GetUserFromID(reader.GetInt32(0), out user);
            }
        }
        return false;
    }

    public static bool GetUserFromAPIKey(string api, out User user)
    {
        using MySqlConnection conn = GetConnection();
        MySqlCommand cmd = new($"select user_id from api_keys where `key`='{api}' limit 1", conn);
        MySqlDataReader reader = cmd.ExecuteReader();
        user = null;
        if (reader.HasRows)
        {
            if (reader.Read())
            {
                return GetUserFromID(reader.GetInt32(0), out user);
            }
        }
        return false;
    }

    public static bool Login(string username, string password, out FailedReason reason, out User user)
    {
        reason = FailedReason.None;
        user = null;

        using MySqlConnection conn = GetConnection();
        MySqlCommand cmd = new($"select id, password from users where username = '{username}' or email = '{username}'", conn);
        MySqlDataReader reader = cmd.ExecuteReader();
        if (!reader.HasRows)
        {
            reason = FailedReason.InvalidUsernameOrEmail;
            return false;
        }
        else
        {
            try
            {
                reader.Read();
                string enc_pwd = reader.GetString("password");
                string pwd = CLAESMath.DecryptStringAES(enc_pwd);

                if (!string.IsNullOrEmpty(pwd))
                {
                    if (pwd.Equals(password) && GetUserFromID(reader.GetInt32("id"), out user))
                    {
                        user = new(user.id, user.username, user.email, enc_pwd, user.type, user.use_git_readme, user.git_username, user.git_token, user.owned_products);
                        return true;
                    }
                    else
                    {
                        reason = FailedReason.InvalidPassword;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Unable to log user in", ex);
            }
        }

        return false;
    }

    public static bool LoginWithToken(string email, string token, out FailedReason reason, out User user) => Login(email, CLAESMath.DecryptStringAES(token), out reason, out user);

    public static bool CreateUser(string username, string email, string password, out FailedReason reason)
    {
        reason = FailedReason.None;
        if (CheckUserExists(username))
        {
            reason = FailedReason.UsernameAlreadyTaken;
        }
        else if (CheckUserExists(email))
        {
            reason = FailedReason.EmailAlreadyTaken;
        }
        else
        {
            try
            {
                string sql = $"INSERT INTO `users`(`username`, `email`, `type`, `password`, `owned_products`) VALUES ('{username}','{email}','{(byte)AccountType.User}','{CLAESMath.EncryptStringAES(password)}','')";
                using MySqlConnection conn = GetConnection();
                MySqlCommand cmd = new(sql, conn);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                log.Error($"Unable to create user", ex.Message, ex.StackTrace);
                return false;
            }
        }
        return false;
    }

    public static int[] GetUsers(int count, int page)
    {
        List<int> users = new();
        using MySqlConnection conn = GetConnection();
        using MySqlCommand cmd = new($"select id from users limit {count} offset {count * page}", conn);
        using MySqlDataReader reader = cmd.ExecuteReader();
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                users.Add(reader.GetInt32(0));
            }
        }
        return users.ToArray();

    }

    public static int[] GetUsersWithPartialUsername(int page, int count, params string[] partials)
    {
        List<int> users = new();
        StringBuilder partialBuilder = new();
        for (int i = 0; i < partials.Length; i++)
        {
            partialBuilder.Append($"`username` like '%{partials[i]}%'");
            if (i != partials.Length - 1)
            {
                partialBuilder.Append(" or ");
            }
        }
        using MySqlConnection conn = GetConnection();
        using MySqlCommand cmd = new($"select id from users where {partialBuilder} limit {count} offset {count * page}", conn);
        using MySqlDataReader reader = cmd.ExecuteReader();
        if (reader.HasRows)
        {
            int current = 0;
            while (reader.Read())
            {
                users.Add(reader.GetInt32(0));
                current++;
            }
        }
        return users.ToArray();
    }

    public static bool UpdateProperty(int id, string token, string name, dynamic value)
    {
        if (value == "true" || value == "false")
            value = value == "true" ? 1 : 0;
        using MySqlConnection conn = GetConnection();
        MySqlCommand cmd = new($"update users set {name}='{value}' where `id`='{id}' and `password`='{token}' limit 1", conn);
        return cmd.ExecuteNonQuery() > 1;
    }

    #endregion Public Methods

}