// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Handlers;
using OpenDSM.Core.Models;

namespace OpenDSM.Server.Controllers.API;

[Route("api/product")]
public class ProductController : ControllerBase
{
    #region Public Methods

    [HttpGet("get")]
    public IActionResult GetProduct(int id)
    {
        return new JsonResult(ProductModel.GetByID(id));
    }

    [HttpPost("create")]
    public IActionResult CreateProduct([FromForm] string name, [FromForm] string gitRepoName, [FromForm] int user_id, [FromForm] string? yt_key, [FromForm] bool subscription, [FromForm] bool use_git_readme, [FromForm] float price, [FromForm] string keywords, [FromForm] string tags, [FromForm] string icon, [FromForm] string banner, [FromForm] string[]? gallery)
    {
        try
        {
            keywords = keywords.ToLower().Trim();
            List<int> ts = new();
            foreach (var item in tags.Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                foreach (var tag in Tags.GetTags())
                {
                    if (tag.name == item) ts.Add(tag.id);
                }

            }
            if (ProductModel.TryCreateProduct(gitRepoName, UserModel.GetByID(user_id), name, yt_key ?? "", subscription, use_git_readme, (int)(price * 100), keywords.Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), ts.ToArray(), out ProductModel model))
            {
                model.IconUrl = icon;
                model.BannerImage = banner;
                if (gallery != null && gallery.Any())
                {
                    model.GalleryImages = gallery;
                }
                return new JsonResult(model);
            }
        }
        catch (Exception e)
        {
            log.Error(e.Message, e);
        }
        return BadRequest();
    }


    #endregion Public Methods
}