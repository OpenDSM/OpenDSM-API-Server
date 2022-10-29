using Octokit;
using OpenDSM.Authentication.Models;
using OpenDSM.Authentication.SQL;
using System.Net;

namespace OpenDSM.Authentication.Collections;

public static class UserCollection
{
    #region Public Methods

    public static bool CreateUser(string email, string username, string password, out UserModel? user, out FailedReason reason) => (user = UsersDB.CreateUser(username, email, password, out reason)) != null;
    public static bool GetById(int user_id, out UserModel? user) => (user = SQL.UsersDB.GetUser(user_id)) == null;

    public static bool Login(string username, string password, string client_name, IPAddress connection_address, out UserModel? user)
    {
        try
        {
            user = SQL.UsersDB.GetUser(username, password).Result;
            if (user != null)
            {
                user.Clients.Add(client_name, connection_address);
                return true;
            }
        }
        catch { user = null; }
        return false;
    }
    public static bool Login(string token, string client_name, IPAddress connection_address, out UserModel? user)
    {
        try
        {
            user = SQL.UsersDB.GetUser(token).Result;
            if (user != null && user.Clients.Contains(client_name, connection_address))
            {
                user.Clients.Add(client_name, connection_address);
                return true;
            }
        }
        catch { user = null; }
        return false;
    }

    #endregion Public Methods
}
