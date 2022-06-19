using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.VIEWS;

[Route("/")]
public class HomeController : Controller
{
    #region Public Methods

    public IActionResult Index()
    {
        return View();
    }

    [Route("/product/{Owner}/{Slug}")]
    public IActionResult Product(string Owner, string Slug)
    {
        ViewBag.Owner = Owner;
        ViewBag.Slug = Slug;
        return View();
    }

    #endregion Public Methods
}