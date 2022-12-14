// LFInteractive LLC. - All Rights Reserved

using MySql.Data.MySqlClient;
using OpenDSM.Authentication.Models;
using OpenDSM.Security;
using OpenDSM.SQL;
using System.Text;

namespace OpenDSM.Authentication.SQL;

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
internal static class UsersDB
{

    #region Public Methods

    public static bool CheckUserExists(string username)
    {
        MySqlDataReader reader = Requests.Select(
             table: table,
             column: "id",
             where: new WhereClause(
                 new IndividualWhereClause[] {
                    new("username", username, "=", false),
                    new("email", username, "=", false)
                 }),
             limit: 1
         );
        return reader.HasRows;
    }

    public static UserModel? CreateUser(string username, string email, string password, out FailedReason reason)
    {
        reason = FailedReason.None;
        if (CheckUserExists(username))
        {
            reason = FailedReason.UserAlreadyExists;
            return null;
        }
        if (CheckUserExists(email))
        {
            reason = FailedReason.EmailAlreadyTaken;
            return null;
        }


        string enc_pass = enc.EncryptString(password);
        return Requests.Insert(
            table: table,
            items: new KeyValuePair<string, dynamic>[]
            {
                new("username", username),
                new("email", email),
                new("password", enc_pass)
            }
        ) ? GetUser(username, enc_pass).Result : null;
    }

    public static UserModel? GetUser(int id)
    {
        MySqlDataReader reader = Requests.Select(
            table: table,
            column: "*",
            where: new(new IndividualWhereClause[]
            {
                new("id", id, "=", true)
            }),
            limit: 1
        );
        if (reader.Read())
        {
            int r_id = reader.GetInt32("id");
            string r_username = reader.GetString("username");
            string r_email = reader.GetString("email");
            string r_about = Encoding.ASCII.GetString(Convert.FromBase64String(reader.GetString("about") + "=="));

            return new UserModel(r_id, r_username, r_email, r_about);
        }
        return null;
    }

    public static async Task<UserModel?> GetUser(string token)
    {
        return await Task.Run(() =>
        {
            MySqlDataReader reader = Requests.Select(
                table: table,
                column: "id",
                where: new(new IndividualWhereClause[]
                {
                    new("password", token, "=", true)
                }),
                limit: 1
            );
            if (reader.Read())
            {
                int r_id = reader.GetInt32("id");
                return GetUser(r_id);
            }
            return null;
        });
    }

    public static async Task<UserModel?> GetUser(string username, string password)
    {
        return await Task.Run(() =>
        {
            MySqlDataReader reader = Requests.Select(
                table: table,
                columns: new string[] { "id", "password" },
                where: new(new IndividualWhereClause[]
                {
                    new("username", username, "=", false),
                    new("email", username, "=", false)
                }),
                limit: 1
            );
            if (reader.Read())
            {
                if (password.Equals(enc.DecrptString(reader.GetString("password"))))
                {
                    return GetUser(reader.GetInt32("id"));
                }
            }
            return null;
        });
    }

    /// <summary>
    /// Asynchronous checks if the users credentials are valid or not
    /// </summary>
    /// <param name="username">The users email/username</param>
    /// <param name="password">The users password</param>
    /// <returns>If the credentials are valid or not.<br /> <b><i><u>THE REASON IS NOT RETURNED!</u></i></b></returns>
    public static async Task<bool> IsValidUserCredentials(string username, string password) => await Task.Run(() => IsValidUserCredentials(username, password, out _, out _));

    /// <summary>
    /// Checks if users credentials are valid or not
    /// </summary>
    /// <param name="username">The users email/username</param>
    /// <param name="password">The users password</param>
    /// <param name="reason">The reason the credentails were invalid</param>
    /// <returns>Returns failed reason reference if credentials are invalid</returns>
    public static bool IsValidUserCredentials(string username, string password, out FailedReason reason, out UserModel? user)
    {
        user = null;
        reason = FailedReason.None;
        MySqlDataReader reader = Requests.Select(
            table: table,
            column: "*",
            where: new(new IndividualWhereClause[]
            {
                new("username", username, "=", false),
                new("email", username, "=", false)
            }),
            limit: 1
        );
        if (reader.Read())
        {
            string r_password = enc.DecrptString(reader.GetString("password"));
            if (!password.Equals(r_password))
            {
                reason = FailedReason.InvalidPassword;
                return false;
            }
            user = GetUser(reader.GetInt32("id"));
            return true;
        }
        reason = FailedReason.InvalidUsernameOrEmail;
        return false;
    }

    #endregion Public Methods

    #region Private Fields

    private static readonly Encryption enc = new("users");
    private static readonly string table = "users";

    #endregion Private Fields

}