// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Models;

namespace OpenDSM.Server.Controllers.VIEWS;

[Route("Error")]
public class ErrorController : Controller
{

    #region Public Methods

    [Route("{code}")]
    public IActionResult Index(int code)
    {
        ViewData["Title"] = $"{code}";

        if (IsLoggedIn(Request.Cookies, out UserModel? user))
        {
            ViewData["User"] = user;
        }
        return View(code);
    }

    #endregion Public Methods

}