// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Models;

namespace OpenDSM.Server.Controllers.API;

[Route("api/product")]
public class ProductController : ControllerBase
{
    #region Public Methods

    [HttpPost("create")]
    public IActionResult CreateProduct([FromForm] string name, [FromForm] string gitRepoName, [FromForm] int user_id, [FromForm] string? yt_key, [FromForm] SQL.PaymentType type, [FromForm] float price, [FromForm] string[]? keywords, [FromForm] int[] tags, [FromForm] string icon, [FromForm] string banner, [FromForm] string[]? gallery)
    {
        if (ProductModel.TryCreateProduct(gitRepoName, UserModel.GetByID(user_id), name, yt_key ?? "", type, (int)(price * 100), keywords ?? Array.Empty<string>(), tags, out ProductModel model))
        {
            model.IconUrl = icon;
            model.BannerImage = banner;
            if (gallery != null)
            {
                model.GalleryImages = gallery;
            }
            return new JsonResult(model);
        }
        return BadRequest();
    }

    [HttpGet("video/{yt_id}")]
    public IActionResult GetVideo(string yt_id)
    {
        if (TryBetYoutubeDirectURL(yt_id, out Uri url))
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

    #endregion Public Methods
}