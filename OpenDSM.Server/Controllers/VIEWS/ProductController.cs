using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Models;

namespace OpenDSM.Server.Controllers.VIEWS;

[Route("/product")]
public class ProductController : Controller
{
    #region Public Methods

    [Route("create")]
    public IActionResult Create()
    {
        if (AuthController.IsLoggedIn(Request.Cookies, out UserModel? user))
        {
            ViewData["Title"] = "Create Product";
            ViewData["User"] = user;
            if (user.IsDeveloperAccount)
            {
                return View(user);
            }
            else
            {
                return RedirectToAction("Profile", "Auth");
            }
        }
        return RedirectToAction("Index", "Error", new { code = 401 });
    }

    [Route("{id}")]
    public IActionResult Index(int id)
    {
        if (ProductModel.TryGetByID(id, out ProductModel? model))
        {
            ViewData["Title"] = model.Name;
            return View(model);
        }
        return RedirectToAction("Index", "Error", new { code = 404 });
    }

    #endregion Public Methods
}