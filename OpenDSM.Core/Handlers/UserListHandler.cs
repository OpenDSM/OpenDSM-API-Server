
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
            if (Authorization.TryGetUserFromID(id, out User user))
            {
                return new(user);
            }
        }
        catch (Exception e)
        {
            log.Error($"Unable to GetuserFromID: {id}", e.Message, e.StackTrace ?? "");
        }
        return null;
    }
    /// <summary>
    /// Attempts to get user from api key
    /// </summary>
    /// <param name="api">api key</param>
    /// <param name="user">the returned user</param>
    /// <returns>If the task was successfull</returns>
    public static bool TryGetByAPIKey(string api, out UserModel user) => (user = GetByAPIKey(api)) != null;

    /// <summary>
    /// Gets the user by api key or null
    /// </summary>
    /// <param name="api">Api key</param>
    /// <returns></returns>
    public static UserModel? GetByAPIKey(string api)
    {
        if (Authorization.TryGetUserFromAPIKey(api, out User u))
            return new(u);
        return null;
    }


    /// <summary>
    /// Gets a user based on username or returns null if no user was found
    /// </summary>
    /// <param name="username">The username</param>
    /// <returns>The user object or null if no user was found</returns>
    public static UserModel? GetByUsername(string username)
    {
        if (Authorization.TryGetUserFromUsername(username, out User user))
        {
            return new(user);
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
        if (Authorization.TryValidateUserCredentials(username, password, out reason, out User user))
        {
            return new(user);
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
    public static bool TryGetUserWithToken(string loginName, string loginToken, out UserModel? user)
    {
        user = null;
        if (Authorization.TryValidateUserCredentialsWithToken(loginName, loginToken, out _, out User u))
        {
            user = new(u);
            return true;
        }
        return false;
    }

}
