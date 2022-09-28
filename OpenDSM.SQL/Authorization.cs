// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
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
    private static readonly string table = "users";
    #region Public Methods
    /// <summary>
    /// Checks if a user exists
    /// </summary>
    /// <param name="username">The username or email</param>
    /// <returns></returns>
    public static bool CheckUserExists(string username)
        => Select(
        table: table,
        column: "id",
        where: new(
            new IndividualWhereClause[] {
                new("username", username, "=", false),
                new("email", username, "=", false)
             }),
        limit: 1)
        .HasRows;

    public static bool TryGetUserFromID(int id, out User? user)
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
            using MySqlDataReader reader = Select(
                  table: table,
                  column: "*",
                  where: new(new IndividualWhereClause[]{
                    new("id", id, "=", false)
                  }));
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

    public static bool TryGetUserFromUsername(string username, out User? user)
    {

        using MySqlDataReader reader = Select(
             table: table,
             column: "id",
             where: new(new IndividualWhereClause[]{
                new("username", username, "=", false),
                new("email", username, "=", false)
             }),
             limit: 1
         );

        user = null;

        if (reader.HasRows)
        {
            while (reader.Read())
            {
                return TryGetUserFromID(reader.GetInt32(0), out user);
            }
        }
        return false;
    }

    public static bool TryGetUserFromAPIKey(string api, out User? user)
    {
        using MySqlDataReader reader = Select(
              table: "api_keys",
              column: "user_id",
              where: new(new IndividualWhereClause[]{
                new("key", api, "=")
              }),
              limit: 1
          );

        user = null;
        if (reader.HasRows)
        {
            if (reader.Read())
            {
                return TryGetUserFromID(reader.GetInt32(0), out user);
            }
        }
        return false;
    }

    public static bool TryValidateUserCredentials(string username, string password, out FailedReason reason, out User? user)
    {
        reason = FailedReason.None;
        user = null;

        using MySqlDataReader reader = Select(
             table: table,
             columns: new[] { "id", "password" },
             where: new(new IndividualWhereClause[]{
                new("username", username, "=", false),
                new("email", username, "=", false)
             })
         );

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
                    if (pwd.Equals(password) && TryGetUserFromID(reader.GetInt32("id"), out user))
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

    public static bool TryValidateUserCredentialsWithToken(string email, string token, out FailedReason reason, out User user) => TryValidateUserCredentials(email, CLAESMath.DecryptStringAES(token), out reason, out user);

    public static bool TryCreateUser(string username, string email, string password, out FailedReason reason)
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
                return Insert(
                    table: table,
                    items: new KeyValuePair<string, dynamic>[]
                    {
                        new("username", username),
                        new("email", email),
                        new("type", (byte)AccountType.User),
                        new("password", CLAESMath.EncryptStringAES(password)),
                        new("owned_products", ""),
                    }
                );
            }
            catch (Exception ex)
            {
                log.Error($"Unable to create user", ex.Message, ex.StackTrace ?? "");
                return false;
            }
        }
        return false;
    }

    public static int[] GetUsers(int count, int page)
    {
        List<int> users = new();

        using MySqlDataReader reader = Select(
            table: table,
            column: "id",
            where: null,
            limit: count,
            offset: count * page
        );
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

        IndividualWhereClause[] clauses = new IndividualWhereClause[partials.Length];
        for (int i = 0; i < partials.Length; i++)
        {
            clauses[i] = new("username", $"%{partials[i]}%", "LIKE", false);
        }

        using MySqlDataReader reader = Select(
            table: table,
            column: "id",
            where: new(clauses),
            limit: count,
            offset: count * page
        );

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
        // Converts c# boolean to one understood by mysql
        if (value.GetType().Equals(typeof(string)))
            if (bool.TryParse(value, out bool result))
                value = Convert.ToByte(result);
        if (value.GetType().Equals(typeof(bool)))
            value = Convert.ToByte((bool)value);

        return Update(
            table: table,
            items: new KeyValuePair<string, dynamic>[]{
                new(name, value)
            },
            limit: 1,
            where: new(new IndividualWhereClause[]{
                new("id", id, "="),
                new("password", token, "=")
            })
        );
    }

    #endregion Public Methods

}