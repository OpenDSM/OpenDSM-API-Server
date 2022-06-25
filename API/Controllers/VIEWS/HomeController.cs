using Microsoft.AspNetCore.Mvc;
using OpenDSM.Lib.Auth;

namespace API.Controllers.VIEWS;

[Route("/")]
public class HomeController : Controller
{
    #region Public Methods

    public IActionResult Index()
    {
        ViewData["Title"] = "The Open Digital Software Marketplace";
        ViewData["LoggedIn"] = false;
        string token = Request.Cookies["auth_token"] ?? "";
        string username = Request.Cookies["auth_username"] ?? "";
        if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(username))
        {
            if (AccountManagement.Instance.TryAttemptLogin(username, token, out User user))
            {
                ViewData["User"] = user;
                ViewData["LoggedIn"] = true;
            }
        }
        return View();
    }

    [Route("/product/{Owner}/{Slug}")]
    public IActionResult Product(string Owner, string Slug)
    {
        ViewData["Title"] = "Product";
        ViewBag.Owner = Owner;
        ViewBag.Slug = Slug;

        ViewData["LoggedIn"] = false;
        string token = Request.Cookies["auth_token"] ?? "";
        string username = Request.Cookies["auth_username"] ?? "";
        if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(username))
        {
            if (AccountManagement.Instance.TryAttemptLogin(username, token, out User user))
            {
                ViewData["User"] = user;
                ViewData["LoggedIn"] = true;
            }
        }

        return View();
    }

    #endregion Public Methods
}