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
            }
        }
        return View();
    }

    [Route("/product/{id}")]
    public IActionResult Product(uint id)
    {
        Product product = Products.Instance.GetProduct(id);
        ViewData["Title"] = product.Name;
        ViewData["page"] = "";

        string token = Request.Cookies["auth_token"] ?? "";
        string username = Request.Cookies["auth_username"] ?? "";
        if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(username))
        {
            if (AccountManagement.Instance.TryAttemptLogin(username, token, out User user))
            {
                ViewData["User"] = user;
            }
        }

        return View(product);
    }

    [Route("/search")]
    public IActionResult Search(string? query)
    {
        ViewData["Title"] = "Search";
        ViewData["page"] = "search";

        string token = Request.Cookies["auth_token"] ?? "";
        string username = Request.Cookies["auth_username"] ?? "";
        if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(username))
        {
            if (AccountManagement.Instance.TryAttemptLogin(username, token, out User user))
            {
                ViewData["User"] = user;
            }
        }
        return View(query);
    }

    #endregion Public Methods
}