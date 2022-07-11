using Microsoft.AspNetCore.Mvc;

namespace OpenDSM.Server.Controllers.VIEWS;

[Route("Error")]
public class ErrorController : Controller
{
    [Route("{code}")]
    public IActionResult Index(int code)
    {
        ViewData["Title"] = $"{code}";
        return View(code);
    }
}
