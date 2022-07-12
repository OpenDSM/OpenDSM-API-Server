using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Models;

namespace OpenDSM.Server.Controllers.VIEWS;
[Route("/product")]
public class ProductController : Controller
{
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
    [Route("create")]
    public IActionResult Create()
    {
        if (AuthController.IsLoggedIn(Request.Cookies, out UserModel? user))
        {
            ViewData["Title"] = "Create Product";
            ViewData["User"] = user;
            return View(user);
        }
        return RedirectToAction("Index", "Error", new { code = 401 });
    }
}
