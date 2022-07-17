using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Models;

namespace OpenDSM.Server.Controllers.VIEWS;

[Route("/")]
public class HomeController : Controller
{
    #region Public Methods

    public IActionResult Index()
    {
        ViewData["Title"] = "The Open Digital Software Marketplace";
        if (AuthController.IsLoggedIn(Request.Cookies, out UserModel? user))
        {
            ViewData["User"] = user;
        }
        return View();
    }

    #endregion Public Methods
}