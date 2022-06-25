using Microsoft.AspNetCore.Mvc;
using OpenDSM.Lib.Auth;

namespace API.Controllers.VIEWS;

[Route("/auth")]
public class AuthController : Controller
{
    #region Public Methods

    [Route("login")]
    public IActionResult Login()
    {
        ViewData["Username"] = "";
        ViewData["LoggedIn"] = false;
        string token = Request.Cookies["auth_token"] ?? "";
        string username = Request.Cookies["auth_username"] ?? "";
        if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(username))
        {
            if (AccountManagement.Instance.TryAttemptLogin(username, token, out User user))
            {
                return RedirectToAction("Index", "Home");
            }
        }
        return View();
    }

    [Route("register")]
    public IActionResult Register()
    {
        ViewData["Username"] = "";
        ViewData["LoggedIn"] = false;
        string token = Request.Cookies["auth_token"] ?? "";
        string username = Request.Cookies["auth_username"] ?? "";
        if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(username))
        {
            if (AccountManagement.Instance.TryAttemptLogin(username, token, out User user))
            {
                return RedirectToAction("Index", "Home");
            }
        }
        return View();
    }

    #endregion Public Methods
}