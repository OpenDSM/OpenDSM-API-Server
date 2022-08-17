using Microsoft.AspNetCore.Mvc;
using OpenDSM.SQL;

namespace OpenDSM.Server.Controllers.API;

[ApiController]
[Route("/api/db")]
public class DBController : ControllerBase
{
    #region Public Methods

    [HttpGet("status")]
    public IActionResult Index()
    {
        if (Connections.TestConnection(out string version))
        {
            return new JsonResult(new
            {
                ok = true,
                version
            });
        }

        return BadRequest(new
        {
            ok = false,
        });
    }

    #endregion Public Methods
}
