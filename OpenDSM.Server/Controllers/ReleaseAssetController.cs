using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Handlers;
using OpenDSM.Core.Models;
using OpenDSM.SQL;

namespace OpenDSM.Server.Controllers;
[ApiController]
[Route("/products/{product_id}/releases/{release_id}/assets")]
public class ReleaseAssetController : ControllerBase
{

    #region Public Methods

    /// <summary>
    /// Downloads the release asset
    /// </summary>
    /// <param name="product_id">The id of the product</param>
    /// <param name="release_id">The id of the release</param>
    /// <param name="platform">The <see cref="Platform">platform</see> of the release</param>
    /// <returns></returns>
    [HttpGet("{platform}")]
    public IActionResult DownloadRelease([FromRoute] int product_id, [FromRoute] long release_id, [FromRoute] Platform platform)
    {
        if (ProductListHandler.TryGetByID(product_id, out ProductModel product))
        {
            VersionModel? version = product.Versions[release_id];
            if (version != null)
            {
                PlatformVersion? platform_version = version.Platforms.FirstOrDefault(i => i.platform == platform);
                if (platform_version != null)
                {
                    return Redirect(platform_version.downloadUrl);
                }
            }
        }
        return RedirectToAction("Index", "Error", 500);
    }

    /// <summary>
    /// Gets all release assets from release
    /// </summary>
    /// <param name="product_id">The id of the product</param>
    /// <param name="release_id">The id of the release</param>
    /// <returns></returns>
    [HttpGet()]
    public IActionResult GetReleaseAssets([FromRoute] int product_id, [FromRoute] long release_id)
    {
        if (ProductListHandler.TryGetByID(product_id, out ProductModel product))
        {
            if (product.Price == 0 || IsLoggedIn(Request, out UserModel user) && user.OwnedProducts.ContainsKey(product_id))
            {
                if (product.Versions.TryGetValue(release_id, out VersionModel version))
                {
                    return new JsonResult(version.Platforms);
                }
                return BadRequest(new
                {
                    message = $"No release exists with id of '{release_id}'"
                });
            }

            return BadRequest(new
            {
                message = $"You are not authorized to view release information!"
            });
        }
        return BadRequest(new
        {
            message = $"Product with id of '{product_id}' does NOT exist"
        });
    }

    /// <summary>
    /// Uploads release asset
    /// </summary>
    /// <param name="product_id">The id of the product</param>
    /// <param name="release_id">The id of the release</param>
    /// <param name="platform">The <see cref="Platform">platform</see> of the release</param>
    /// <param name="file">The zip file containing release binaries</param>
    /// <returns></returns>
    [HttpPost("{platform}"), DisableRequestSizeLimit]
    public async Task<IActionResult> UploadReleaseAsset([FromRoute] int product_id, [FromRoute] int release_id, [FromRoute] Platform platform, [FromBody] IFormFile file)
    {
        if (IsLoggedIn(Request, out UserModel user))
        {
            if (ProductListHandler.TryGetByID(product_id, out ProductModel product))
            {
                try
                {
                    if (await GitHandler.UploadReleaseAsset(user.GitCredentials, file.OpenReadStream(), product, platform, release_id))
                    {
                        return Ok(new
                        {
                            message = "asset uploaded!"
                        });
                    }
                }
                catch (Exception e)
                {
                    return BadRequest(new
                    {
                        message = $"Unable to upload git release asset: {e.Message}",
                        stacktrace = e.StackTrace
                    });
                }
                return BadRequest(new
                {
                    message = $"Unable to upload git release asset"
                });

            }

            return BadRequest(new
            {
                message = $"Unable to find product with id of {product_id}"
            });
        }
        return BadRequest(new
        {
            message = "User couldn't be authenticated"
        });
    }

    #endregion Public Methods

}