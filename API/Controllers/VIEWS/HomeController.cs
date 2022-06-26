using Microsoft.AspNetCore.Mvc;
using OpenDSM.Lib.Auth;
using OpenDSM.Lib.Objects;

namespace API.Controllers.VIEWS;

[Route("/")]
public class HomeController : Controller
{
    #region Public Methods

    public IActionResult Index()
    {
        ViewData["Title"] = "The Open Digital Software Marketplace";
        ViewData["LoggedIn"] = false;
        ViewData["page"] = "home";
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

    [Route("/product/{id}")]
    public IActionResult Product(long id)
    {
        ViewData["Title"] = "Product";
        ViewData["page"] = "";

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

        return View(Products.Instance.GetProduct(id));
    }

    [Route("/search")]
    public IActionResult Search(string? query)
    {
        return View(query);
    }

    #endregion Public Methods
}