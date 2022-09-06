// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Handlers;
using OpenDSM.Core.Models;
using UAParser;

namespace OpenDSM.Server.Controllers.VIEWS;

[Route("/product")]
public class ProductController : Controller
{

    #region Public Methods

    [Route("create")]
    public IActionResult Create()
    {
        if (IsLoggedIn(Request, out UserModel? user))
        {
            ViewData["Title"] = "Create Product";
            ViewData["User"] = user;
            if (user.IsDeveloperAccount)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Profile", "Auth");
            }
        }
        return RedirectToAction("Index", "Error", new { code = 401 });
    }

    [Route("{id}")]
    public IActionResult Index(int id, bool? preview)
    {
        if (ProductListHandler.TryGetByID(id, out ProductModel? model))
        {
            if (IsLoggedIn(Request, out UserModel? user))
                ViewData["User"] = user;
            ViewData["Title"] = model.Name;
            ViewData["Preview"] = preview.GetValueOrDefault(false);
            ClientInfo info = Parser.GetDefault().Parse(Request.Headers["User-Agent"]);
            ViewData["OS"] = info.OS.Family;

            if (Request.Cookies[$"page_view"] == null)
            {
                model.AddPageView();
            }

            return View(model);
        }
        return RedirectToAction("Index", "Error", new { code = 404 });
    }

    [Route("modify")]
    public IActionResult Modify(int id)
    {
        if (IsLoggedIn(Request, out UserModel? user))
        {
            ViewData["Title"] = "Modify Product";
            ViewData["User"] = user;
            if (user.IsDeveloperAccount)
            {
                if (ProductListHandler.TryGetByID(id, out ProductModel product) && product.User.Equals(user))
                {
                    ViewData["Product"] = product;
                    return View("Create");
                }
            }
            else
            {
                return RedirectToAction("Profile", "Auth");
            }
        }
        return RedirectToAction("Index", "Error", new { code = 500 });
    }

    [Route("{product_id}/element/Versions")]
    public IActionResult GetVersionsElement(int product_id)
    {
        if (ProductListHandler.TryGetByID(product_id, out ProductModel? model))
        {
            ViewData["IsOwner"] = false;
            ViewData["HasProduct"] = false;
            if (IsLoggedIn(Request, out UserModel? user))
            {
                if (user.Equals(model.User))
                    ViewData["IsOwner"] = true;
                if (user.OwnedProducts.ContainsKey(product_id))
                    ViewData["HasProduct"] = true;
            }
            return View($"Elements/Versions", model);
        }
        return RedirectToAction("index", "error", new { code = 404 });
    }

    [Route("{product_id}/element/Reviews")]
    public IActionResult GetReviewsElement(int product_id, [FromQuery] int? filter = -1)
    {
        if (ProductListHandler.TryGetByID(product_id, out ProductModel? model))
        {
            ViewData["IsOwner"] = false;
            ViewData["HasProduct"] = false;
            ViewData["Filter"] = filter.GetValueOrDefault(-1);
            if (IsLoggedIn(Request, out UserModel? user))
            {
                if (user.Equals(model.User))
                    ViewData["IsOwner"] = true;
                if (user.OwnedProducts.ContainsKey(product_id))
                    ViewData["HasProduct"] = true;
            }
            return View($"Elements/Reviews", model);
        }
        return RedirectToAction("index", "error", new { code = 404 });
    }

    #endregion Public Methods

}