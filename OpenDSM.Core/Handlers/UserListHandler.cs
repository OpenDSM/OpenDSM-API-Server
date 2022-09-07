
using System.Text.Json;
using OpenDSM.Core.Models;
using OpenDSM.SQL;

namespace OpenDSM.Core.Handlers;

/// <summary>
/// Handles all user list actions
/// </summary>
public static class UserListHandler
{

    /// <summary>
    /// Gets a list of users matching partial string array
    /// </summary>
    /// <param name="count">The max number of results</param>
    /// <param name="page">The page offset of results</param>
    /// <param name="partials">The array of words</param>
    /// <returns></returns>
    public static UserModel[] GetUserFromPartials(int count, int page, params string[] partials)
    {
        List<UserModel> users = new();
        int[] user_ids = Authorization.GetUsersWithPartialUsername(page, count, partials);
        Parallel.ForEach(user_ids, id =>
        {
            if (TryGetByID(id, out UserModel user))
                users.Add(user);
        });

        return users.ToArray();
    }

    /// <summary>
    /// Attempts to get a user based on the user id specified.
    /// </summary>
    /// <param name="id">The user id</param>
    /// <param name="user">The user object found</param>
    /// <returns>If the user was found or not</returns>
    public static bool TryGetByID(int id, out UserModel user) => (user = GetByID(id)) != null;

    /// <summary>
    /// Gets a user based on user id specified or returns null if none was found
    /// </summary>
    /// <param name="id">The users id</param>
    /// <returns>The user object or null if no user was found</returns>
    public static UserModel? GetByID(int id)
    {
        try
        {
            if (Authorization.GetUserFromID(id, out string username, out string email, out AccountType type, out bool use_git_readme, out string git_username, out string git_token, out int[] _))
            {
                return new(id, username, email, "", type, use_git_readme, new())
                {
                    GitUsername = git_username,
                    GitToken = git_token,
                };
            }
        }
        catch (Exception e)
        {
            log.Error($"Unable to GetuserFromID: {id}", e.Message, e.StackTrace ?? "");
        }
        return null;
    }

    /// <summary>
    /// Gets a user based on username or returns null if no user was found
    /// </summary>
    /// <param name="username">The username</param>
    /// <returns>The user object or null if no user was found</returns>
    public static UserModel? GetByUsername(string username)
    {
        if (Authorization.GetUserFromUsername(username, out int id, out string email, out AccountType type, out bool use_git_readme, out string git_username, out string git_token, out _))
        {
            return new(id, username, email, "", type, use_git_readme, new())
            {
                GitUsername = git_username,
                GitToken = git_token,
            };
        }
        return null;
    }
    /// <summary>
    /// Creates a user object based on username and password
    /// </summary>
    /// <param name="username">The users email or username</param>
    /// <param name="password">The users password</param>
    /// <param name="reason">If the task failed, this is the reason</param>
    /// <returns>The user object created or null if the task failed</returns>
    public static UserModel? GetUser(string username, string password, out FailedReason reason)
    {
        if (Authorization.Login(username, password, out reason, out AccountType type, out bool use_git_readme, out int id, out string email, out string uname, out string token, out string products, out string r_git_username, out string r_git_token))
        {
            return new(id, uname, email, token, type, use_git_readme, string.IsNullOrWhiteSpace(products) ? new() : JsonSerializer.Deserialize<Dictionary<int, UserProductStat>>(products))
            {
                GitToken = r_git_token,
                GitUsername = r_git_username
            };
        }
        return null;
    }

    /// <summary>
    /// Attempts to create user object based on username and password
    /// </summary>
    /// <param name="username">The users email or username</param>
    /// <param name="password">The users password</param>
    /// <param name="user">The created user object</param>
    /// <returns>If the user object was able to be created or not</returns>
    public static bool TryGetUser(string username, string password, out UserModel user) => TryGetUser(username, password, out user, out FailedReason _);

    /// <summary>
    /// Attempts to create user object based on username and password, and outputs the failed reason.
    /// </summary>
    /// <param name="username">The users email or username</param>
    /// <param name="password">The users password</param>
    /// <param name="user">The created user object</param>
    /// <param name="reason">If the task failed, this is the reason</param>
    /// <returns>If the user object was able to be created or not</returns>
    public static bool TryGetUser(string username, string password, out UserModel user, out FailedReason reason) => (user = GetUser(username, password, out reason)) != null;

    /// <summary>
    /// Attempts to create user object based on 
    /// </summary>
    /// <param name="loginName">The users email or username</param>
    /// <param name="loginToken">The users login token</param>
    /// <param name="user">The created user object or null</param>
    /// <returns>If the user object was able to be created</returns>
    public static bool TryGetUserWithToken(string loginName, string loginToken, out UserModel user)
    {
        user = null;
        if (Authorization.LoginWithToken(loginName, loginToken, out _, out AccountType type, out bool use_git_readme, out int id, out string r_email, out string uname, out _, out string products, out string r_git_username, out string r_git_token))
        {
            user = new(id, uname, r_email, loginToken, type, use_git_readme, string.IsNullOrWhiteSpace(products) ? new() : JsonSerializer.Deserialize<Dictionary<int, UserProductStat>>(products))
            {
                GitToken = r_git_token,
                GitUsername = r_git_username
            }; ;
            return true;
        }
        return false;
    }

}
