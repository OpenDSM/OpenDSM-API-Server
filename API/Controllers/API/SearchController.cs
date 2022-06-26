using Microsoft.AspNetCore.Mvc;
using OpenDSM.Lib.Objects;

namespace API.Controllers.API;

[ApiController]
[Route("/api/search")]
public class SearchController : ControllerBase
{
    #region Public Methods

    [HttpGet()]
    public IActionResult Index(string query, [FromQuery] string[] tags)
    {
        return new JsonResult(new
        {
            result = Products.Instance.Search(query, tags)
        });
    }

    #endregion Public Methods
}