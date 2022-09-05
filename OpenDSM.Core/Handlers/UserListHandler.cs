
using System.Text.Json;
using OpenDSM.Core.Models;
using OpenDSM.SQL;

namespace OpenDSM.Core.Handlers;

public static class UserListHandler
{
    public static UserModel[] GetUserFromPartials(int maxSize, params string[] partials)
    {
        List<UserModel> users = new();
        int[] u = Authorization.GetUsersWithPartialUsername(maxSize, partials);
        Parallel.ForEach(u, id =>
        {
            UserModel? user = GetByID(id);
            if (user != null)
                users.Add(user);
        });

        return users.ToArray();
    }


    public static bool TryGetByID(int id, out UserModel? user)
    {
        return (user = GetByID(id)) != null;
    }
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

    public static UserModel? GetByUsername(string username)
    {
        if (Authorization.GetUserFromUsername(username, out int id, out string email, out AccountType type, out bool use_git_readme, out string git_username, out string git_token, out int[] products))
        {
            return new(id, username, email, "", type, use_git_readme, new())
            {
                GitUsername = git_username,
                GitToken = git_token,
            };
        }
        return null;
    }
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

    public static bool TryGetUser(string username, string password, out UserModel? user)
    {
        return TryGetUser(username, password, out user, out FailedReason _);
    }

    public static bool TryGetUser(string username, string password, out UserModel? user, out FailedReason reason)
    {
        user = GetUser(username, password, out reason);
        return user != null;
    }

    public static bool TryGetUserWithToken(string email, string password, out UserModel? user)
    {
        user = null;
        if (Authorization.LoginWithToken(email, password, out var reason, out AccountType type, out bool use_git_readme, out int id, out string r_email, out string uname, out string token, out string products, out string r_git_username, out string r_git_token))
        {
            user = new(id, uname, email, token, type, use_git_readme, string.IsNullOrWhiteSpace(products) ? new() : JsonSerializer.Deserialize<Dictionary<int, UserProductStat>>(products))
            {
                GitToken = r_git_token,
                GitUsername = r_git_username
            }; ;
            return true;
        }
        return false;
    }

}
