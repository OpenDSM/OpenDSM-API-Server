using Microsoft.AspNetCore.Mvc;
using OpenDSM.Lib.Auth;

namespace API.Controllers.VIEWS;

[Route("/auth")]
public class AuthController : Controller
{
    #region Public Methods

    [Route("create")]
    public IActionResult Create()
    {
        string token = Request.Cookies["auth_token"] ?? "";
        string username = Request.Cookies["auth_username"] ?? "";
        if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(username))
        {
            if (AccountManagement.Instance.TryAttemptLogin(username, token, out User user))
            {
                ViewData["Title"] = "Create Product";
                ViewData["page"] = "profile";
                ViewData["User"] = user;
                return View(user);
            }
        }
        return RedirectToAction("Index");
    }

    public IActionResult Index(string? page = "")
    {
        string token = Request.Cookies["auth_token"] ?? "";
        string username = Request.Cookies["auth_username"] ?? "";
        if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(username))
        {
            if (AccountManagement.Instance.TryAttemptLogin(username, token, out User user))
            {
                if (string.IsNullOrEmpty(page))
                {
                    ViewData["Title"] = "Profile";
                    ViewData["page"] = "profile";
                    ViewData["User"] = user;
                    return View("Profile", user);
                }
                else
                {
                    return View($"Profile/{page}", user);
                }
            }
        }
        ViewData["Title"] = "Login/Register";
        ViewData["page"] = "login";
        return View("LoginRegister");
    }

    #endregion Public Methods
}