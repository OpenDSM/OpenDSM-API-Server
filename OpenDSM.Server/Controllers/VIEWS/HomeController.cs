using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Models;

namespace OpenDSM.Server.Controllers.VIEWS;

[Route("/")]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "The Open Digital Software Marketplace";
        if (AuthController.IsLoggedIn(Request.Cookies, out UserModel? user))
        {
            ViewData["User"] = user;
        }
        return View();
    }
}