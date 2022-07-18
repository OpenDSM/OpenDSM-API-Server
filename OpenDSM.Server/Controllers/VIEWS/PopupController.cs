using Microsoft.AspNetCore.Mvc;

namespace OpenDSM.Server.Controllers.VIEWS;

[Route("/popup")]
public class PopupController : Controller
{
    #region Public Methods

    [Route("{name}")]
    public IActionResult Index(string name)
    {
        return View(name);
    }

    #endregion Public Methods
}