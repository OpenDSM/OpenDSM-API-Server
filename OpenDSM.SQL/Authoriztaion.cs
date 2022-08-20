// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using MySql.Data.MySqlClient;
using System.Data.SqlTypes;
using CLMath;
using System.Text.Json;

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

public static class Authoriztaion
{

    #region Public Methods

    public static bool CheckUserExists(string username)
    {
        try
        {
            using MySqlConnection conn = new(Instance.ConnectionString);
            conn.Open();
            MySqlCommand cmd = new($"select * from users where `username` = '{username}' or `email` = '{username}'", conn);
            return cmd.ExecuteReader().HasRows;
        }
        catch (Exception ex)
        {
            log.Error($"Unable to check if user exists", ex);
            return false;
        }
    }

    public static bool GetUserFromID(int id, out string username, out string email, out AccountType type, out bool use_git_readme, out string git_username, out string git_token, out int[] owned_products)
    {
        username = "";
        email = "";
        git_username = "";
        git_token = "";
        type = AccountType.User;
        owned_products = Array.Empty<int>();
        use_git_readme = false;
        try
        {

            using MySqlConnection conn = new(Instance.ConnectionString);
            conn.Open();
            MySqlCommand cmd = new($"select * from users where id = '{id}'", conn);
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
                    conn.Close();
                    return true;
                }
            }
            conn.Close();
        }
        catch (Exception e)
        {
            log.Error($"Unable to query users from sql: {id}", e.Message, e.StackTrace ?? "");
        }
        return false;
    }

    public static bool GetUserFromUsername(string username, out int id, out string email, out AccountType type, out bool use_git_readme, out string git_username, out string git_token, out int[] owned_products)
    {
        id = 0;
        email = "";
        git_username = "";
        git_token = "";
        type = AccountType.User;
        owned_products = Array.Empty<int>();
        use_git_readme = false;
        using MySqlConnection conn = new(Instance.ConnectionString);
        conn.Open();
        MySqlCommand cmd = new($"select * from users where username = '{username}'", conn);
        MySqlDataReader reader = cmd.ExecuteReader();
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                id = reader.GetInt32("id");
                email = reader.GetString("email");
                use_git_readme = reader.GetBoolean("use_git_readme");
                git_username = reader.GetString("git_username");
                git_token = reader.GetString("git_token");
                type = reader.GetInt16("type") switch
                {
                    1 => AccountType.Seller,
                    2 => AccountType.Admin,
                    _ => AccountType.User
                };
                owned_products = Array.ConvertAll(reader.GetString("owned_product_ids").Split(";", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries), i => Convert.ToInt32(i));
                conn.Close();
                return true;
            }
        }
        conn.Close();
        return false;
    }
    public static bool Login(string username, string password, out FailedReason reason, out AccountType type, out bool use_git_readme, out int r_id, out string r_email, out string r_username, out string r_token, out string r_owned_products, out string r_git_username, out string r_git_token)
    {
        reason = FailedReason.None;
        type = AccountType.User;
        r_email = "";
        r_username = "";
        r_owned_products = "";
        r_id = 0;
        r_token = "";
        r_git_token = "";
        r_git_username = "";
        use_git_readme = false;
        using (MySqlConnection conn = new(Instance.ConnectionString))
        {
            conn.Open();
            MySqlCommand cmd = new($"select * from users where username = '{username}' or email = '{username}'", conn);
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
                    r_id = reader.GetInt32("id");
                    r_email = reader.GetString("email");
                    r_username = reader.GetString("username");
                    type = reader.GetInt16("type") switch
                    {
                        1 => AccountType.Seller,
                        2 => AccountType.Admin,
                        _ => AccountType.User
                    };
                    try
                    {
                        r_owned_products = reader.GetString("owned_products");
                    }
                    catch (SqlNullValueException)
                    {
                        r_owned_products = "";
                    }
                    try
                    {
                        r_git_username = !reader.IsDBNull(6) ? reader.GetString("git_username") : "";
                    }
                    catch (SqlNullValueException)
                    {
                        r_git_username = "";
                    }
                    try
                    {
                        r_git_token = !reader.IsDBNull(7) ? reader.GetString("git_token") : "";

                    }
                    catch (SqlNullValueException)
                    {
                        r_git_token = "";
                    }
                    use_git_readme = reader.GetBoolean("use_git_readme");
                    string enc_pwd = reader.GetString("password");
                    string pwd = CLAESMath.DecryptStringAES(enc_pwd);

                    if (!string.IsNullOrEmpty(pwd))
                    {
                        if (pwd.Equals(password))
                        {
                            r_token = enc_pwd;
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
            conn.Close();
        }

        return false;
    }

    public static bool LoginWithToken(string email, string token, out FailedReason reason, out AccountType type, out bool use_git_readme, out int r_id, out string r_email, out string r_username, out string r_token, out string r_owned_products, out string r_git_username, out string r_git_token)
    {
        string pwd = CLAESMath.DecryptStringAES(token);
        return Login(email, pwd, out reason, out type, out use_git_readme, out r_id, out r_email, out r_username, out r_token, out r_owned_products, out r_git_username, out r_git_token);
    }

    public static bool Register(string username, string email, string password, out FailedReason reason)
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
                using MySqlConnection conn = new(Instance.ConnectionString);
                conn.Open();
                //string sql = $"INSERT INTO `users`(`username`, `email`, `type`, `owned_product_ids`, `git_username`, `git_token`, `password`, `use_git_readme`) VALUES ('{username}','{email}','{(byte)AccountType.User}','','','','{CLAESMath.EncryptStringAES(password)}','0')";
                string sql = $"INSERT INTO `users`(`username`, `email`, `type`, `password`) VALUES ('{username}','{email}','{(byte)AccountType.User}','{CLAESMath.EncryptStringAES(password)}')";
                MySqlCommand cmd = new(sql, conn);
                log.Debug(sql);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                log.Error($"Unable to create user", ex);
                return false;
            }
        }
        return false;
    }

    public static bool UpdateProperty(int id, string token, string name, dynamic value)
    {
        using MySqlConnection conn = new(Instance.ConnectionString);
        conn.Open();
        if (value == "true" || value == "false")
            value = value == "true" ? 1 : 0;
        string query = $"update users set {name}='{value}' where `id`='{id}' and `password`='{token}'";
        MySqlCommand cmd = new(query, conn);
        return cmd.ExecuteNonQuery() > 1;
    }

    #endregion Public Methods

}