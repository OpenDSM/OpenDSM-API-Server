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

    [HttpGet("video/{yt_id}")]
    public IActionResult GetVideo(string yt_id)
    {
        if (YTHandler.TryGetYoutubeDirectURL(yt_id, out Uri url))
        {
            return new JsonResult(new
            {
                url = url.AbsoluteUri
            });
        }
        return BadRequest(new
        {
            message = "Unable to parse youtube url"
        });
    }
}
