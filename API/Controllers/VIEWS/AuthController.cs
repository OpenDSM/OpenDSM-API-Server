using Microsoft.AspNetCore.Mvc;
using OpenDSM.Lib.Auth;

namespace API.Controllers.VIEWS;

[Route("/auth")]
public class AuthController : Controller
{
    #region Public Methods

    public IActionResult Index()
    {
        ViewData["LoggedIn"] = false;
        string token = Request.Cookies["auth_token"] ?? "";
        string username = Request.Cookies["auth_username"] ?? "";
        if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(username))
        {
            if (AccountManagement.Instance.TryAttemptLogin(username, token, out User user))
            {
                return View("Profile", user);
            }
        }
        return View("LoginRegister");
    }

    #endregion Public Methods
}