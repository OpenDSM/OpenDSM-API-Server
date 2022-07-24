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

    [HttpGet("{id}/images/{name}")]
    public IActionResult GetImage(int id, string name)
    {
        ProductModel model = ProductModel.GetByID(id);
        if (model != null)
        {
            string path = Path.Combine(GetProductDirectory(id), $"{name}.jpg");
            FileStream fs = new(path, FileMode.Open, FileAccess.Read);
            return new FileStreamResult(fs, "image/jpg");
        }
        return BadRequest();
    }

    [HttpGet("video/{yt_id}")]
    public IActionResult GetVideo(string yt_id)
    {
        if (YTHandler.TryGetYoutubeDirectURL(yt_id, out Uri url))
        {
            return Redirect(url.AbsoluteUri);
        }
        return BadRequest(new
        {
            message = "Unable to parse youtube url"
        });
    }

    [Route("{id}")]
    public IActionResult Index(int id)
    {
        if (ProductModel.TryGetByID(id, out ProductModel? model))
        {
            if (AuthController.IsLoggedIn(Request.Cookies, out UserModel? user))
                ViewData["User"] = user;
            ViewData["Title"] = model.Name;
            ClientInfo info = Parser.GetDefault().Parse(Request.Headers["User-Agent"]);
            ViewData["OS"] = info.OS.Family;
            return View(model);
        }
        return RedirectToAction("Index", "Error", new { code = 404 });
    }

    #endregion Public Methods
}