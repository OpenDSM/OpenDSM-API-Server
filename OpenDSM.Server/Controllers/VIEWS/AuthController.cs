// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Handlers;
using OpenDSM.Core.Models;
using Org.BouncyCastle.Ocsp;

namespace OpenDSM.Server.Controllers.VIEWS;

[Route("/auth")]
public class AuthController : Controller
{

    #region Public Methods

    [Route("login")]
    public IActionResult Login([FromQuery] string? @ref)
    {
        ViewData["Title"] = "Login";
        if (IsLoggedIn(Request, out UserModel? user))
        {
            return RedirectToAction("Index", "Home");
        }
        ViewData["ref"] = @ref ?? "";
        return View();
    }

    [Route("/profile/{id?}")]
    public IActionResult Profile(int? id)
    {
        ViewData["is_personal"] = false;
        if (IsLoggedIn(Request, out UserModel? loggedin))
        {
            ViewData["User"] = loggedin;
        }
        if (id.HasValue)
        {
            UserModel? user = UserListHandler.GetByID(id.Value);
            if (user == null)
                return RedirectToAction("Index", "Error", new { code = 404 });
            ViewData["Title"] = $"{user?.Username}'s Profile";
            return View(user);
        }
        if (loggedin != null)
        {
            ViewData["is_personal"] = true;
            ViewData["Title"] = $"Your Profile";
            return View(loggedin);
        }
        return RedirectToAction("Index", "Home");
    }

    [Route("signup")]
    public IActionResult Signup([FromQuery] string? @ref)
    {
        ViewData["Title"] = "Signup";
        if (IsLoggedIn(Request, out UserModel? _))
        {
            return RedirectToAction("Index", "Home");
        }
        ViewData["ref"] = @ref ?? "";
        return View();
    }

    #endregion Public Methods

}