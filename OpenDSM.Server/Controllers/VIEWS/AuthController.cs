using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Models;

namespace OpenDSM.Server.Controllers.VIEWS;

[Route("/auth")]
public class AuthController : Controller
{
    [Route("login")]
    public IActionResult Login()
    {
        ViewData["Title"] = "Login";
        if (IsLoggedIn(Request.Cookies, out UserModel? user))
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }
    [Route("signup")]
    public IActionResult Signup()
    {
        ViewData["Title"] = "Signup";
        if (IsLoggedIn(Request.Cookies, out UserModel? _))
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [Route("profile")]
    public IActionResult GetProfile()
    {
        if (IsLoggedIn(Request.Cookies, out UserModel? user))
        {
            ViewData["Title"] = $"{user?.Username}'s Profile";
            ViewData["User"] = user;
            return View(user);
        }
        return RedirectToAction("Index", "Home");
    }

    [Route("profile/{id}")]
    public IActionResult GetUsersProfile(int id)
    {
        if (IsLoggedIn(Request.Cookies, out UserModel? loggedin))
        {
            ViewData["User"] = loggedin;
        }
        UserModel? user = UserModel.GetByID(id);
        if (user == null)
            return RedirectToAction("Index", "Error", new { code = 404 });
        return View(user);
    }

    public static bool IsLoggedIn(IRequestCookieCollection cookies, out UserModel? user)
    {
        user = null;
        string email = cookies["auth_email"] ?? "";
        string token = cookies["auth_token"] ?? "";
        if (!string.IsNullOrEmpty(email) && !string.IsNullOrWhiteSpace(token))
        {
            return UserModel.TryGetUserWithToken(email, token, out user);
        }

        return false;
    }
}