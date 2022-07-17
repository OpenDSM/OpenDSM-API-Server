using Microsoft.AspNetCore.Mvc;

namespace OpenDSM.Server.Controllers.VIEWS;

[Route("/popup")]
public class PopupController : Controller
{
    [Route("{name}")]
    public IActionResult Index(string name)
    {
        return View(name);
    }
}
