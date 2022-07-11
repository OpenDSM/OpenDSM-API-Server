using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Models;

namespace OpenDSM.Server.Controllers.VIEWS;

[Route("/auth")]
public class AuthController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public static bool IsLoggedIn(IRequestCookieCollection cookies, out UserModel? user)
    {
        user = null;
        string username = cookies["auth_username"] ?? "";
        string token = cookies["auth_token"] ?? "";
        if (!(string.IsNullOrEmpty(username) && string.IsNullOrWhiteSpace(token)))
        {
            return UserModel.TryGetUser(username, token, out user);
        }

        return false;
    }
}
