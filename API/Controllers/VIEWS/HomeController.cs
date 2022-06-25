using Microsoft.AspNetCore.Mvc;
using OpenDSM.Lib.Auth;

namespace API.Controllers.VIEWS;

[Route("/")]
public class HomeController : Controller
{
    #region Public Methods

    public IActionResult Index()
    {
        ViewData["Username"] = "";
        ViewData["LoggedIn"] = false;
        string token = Request.Cookies["auth_token"] ?? "";
        string username = Request.Cookies["auth_username"] ?? "";
        if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(username))
        {
            if (AccountManagement.Instance.TryAttemptLogin(username, token, out User user))
            {
                ViewData["Username"] = user.Username;
                ViewData["LoggedIn"] = true;
            }
        }
        return View();
    }

    [Route("/product/{Owner}/{Slug}")]
    public IActionResult Product(string Owner, string Slug)
    {
        ViewBag.Owner = Owner;
        ViewBag.Slug = Slug;

        ViewData["Username"] = "";
        ViewData["LoggedIn"] = false;
        string token = Request.Cookies["auth_token"] ?? "";
        string username = Request.Cookies["auth_username"] ?? "";
        if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(username))
        {
            if (AccountManagement.Instance.TryAttemptLogin(username, token, out User user))
            {
                ViewData["Username"] = user.Username;
                ViewData["LoggedIn"] = true;
            }
        }

        return View();
    }

    #endregion Public Methods
}