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
                ViewData["page"] = "profile";
                return View(user);
            }
        }
        return RedirectToAction("Index");
    }

    public IActionResult Index()
    {
        string token = Request.Cookies["auth_token"] ?? "";
        string username = Request.Cookies["auth_username"] ?? "";
        if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(username))
        {
            if (AccountManagement.Instance.TryAttemptLogin(username, token, out User user))
            {
                ViewData["page"] = "profile";
                return View("Profile", user);
            }
        }
        ViewData["page"] = "login";
        return View("LoginRegister");
    }

    #endregion Public Methods
}