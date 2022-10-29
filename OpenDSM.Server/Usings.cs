global using static OpenDSM.Server.Usings;
using Microsoft.Extensions.Primitives;
using OpenDSM.Authentication.Collections;
using OpenDSM.Authentication.Models;
using System.Net;

namespace OpenDSM.Server;

internal static class Usings
{
    #region Public Methods

    public static bool IsLoggedIn(HttpRequest request, out UserModel user)
    {
        if (request.Cookies["auth_token"] != null)
        {
            try
            {
                IPAddress? client_address = request.HttpContext.Connection.RemoteIpAddress;
                string? token_cookie = request.Cookies["auth_token"];
                if (client_address != null && !string.IsNullOrWhiteSpace(token_cookie))
                    return UserCollection.Login(token_cookie, "", client_address, out user);
            }
            catch { }
        }
        IHeaderDictionary headers = request.Headers;
        if (headers.TryGetValue("auth_token", out StringValues token_header))
        {
            try
            {
                IPAddress? client_address = request.HttpContext.Connection.RemoteIpAddress;
                if (client_address != null)
                    return UserCollection.Login(token_header, "", client_address, out user);
            }
            catch { }
        }

        //if (headers.TryGetValue("api_key", out StringValues value))
        //{
        //    if (UserListHandler.TryGetByAPIKey(value, out user))
        //    {
        //        API.IncrementCall(value);
        //        return true;
        //    }
        //}
        user = null;
        return false;
    }

    #endregion Public Methods
}
