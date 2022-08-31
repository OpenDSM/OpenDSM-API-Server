// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Handlers;
using OpenDSM.Core.Models;

namespace OpenDSM.Server.Controllers.VIEWS;

[Route("/")]
public class HomeController : Controller
{

    #region Public Methods

    public IActionResult Index()
    {
        ViewData["Title"] = "The Open Digital Software Marketplace";
        if (IsLoggedIn(Request, out UserModel? user))
        {
            ViewData["User"] = user;
        }
        return View();
    }

    [Route("search")]
    public IActionResult Search(string? query, SearchCategory? category)
    {
        if (IsLoggedIn(Request, out UserModel? loggedin))
        {
            ViewData["User"] = loggedin;
        }
        ViewData["Title"] = query ?? "Search";
        if (category == null)
        {
            return View("SearchCategorySelector");

        }
        ViewData["query"] = query ?? "";
        ViewData["Category"] = category;
        return View();
    }

    #endregion Public Methods

}