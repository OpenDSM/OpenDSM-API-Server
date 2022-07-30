using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Handlers;

namespace OpenDSM.Server.Controllers.API;

[Route("/api/yt")]
public class YTController : ControllerBase
{
    [HttpGet("channel/{id}")]
    public IActionResult GetChannelUploads(string id)
    {
        return new JsonResult(YTHandler.GetChannelVideos(id).Result);
    }
}
