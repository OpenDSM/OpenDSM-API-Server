using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Handlers;

namespace OpenDSM.Server.Controllers.API;

[Route("/api/video")]
public class VideoController : ControllerBase
{

    #region Public Methods

    [HttpGet("channel/{id}")]
    public IActionResult GetChannelUploads(string id)
    {
        return new JsonResult(YTHandler.GetChannelVideos(id).Result);
    }

    [HttpGet("{key}")]
    public IActionResult GetVideo(string key)
    {
        if (YTHandler.TryGetYoutubeDirectURL(key, out Uri url))
        {
            return RedirectPermanentPreserveMethod(url.AbsoluteUri);
        }
        return BadRequest(new
        {
            message = "Unable to parse youtube url"
        });
    }

    #endregion Public Methods

}
