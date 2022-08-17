// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OpenDSM.Core.Handlers;
using OpenDSM.Core.Models;
using OpenDSM.SQL;
using Tags = OpenDSM.Core.Models.Tags;
namespace OpenDSM.Server.Controllers.API;

[Route("api/product")]
public class ProductController : ControllerBase
{

    #region Public Methods

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

    [HttpPost("create-version")]
    public IActionResult CreateVersion([FromForm] int id, [FromForm] string name, [FromForm] ReleaseType type, [FromForm] string changelog)
    {
        if (IsLoggedIn(Request.Cookies, out UserModel user))
        {
            if (ProductModel.TryGetByID(id, out ProductModel? model))
            {
                int release_id = GitHandler.CreateRelease(user.GitCredentials, model, name, type, changelog).Result;
                if (release_id != -1)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"Release created with id: {release_id}",
                        id = release_id,
                        repo = model.GitRepositoryName,
                        owner = user.GitUsername,
                        git_token = user.GitToken,
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = $"Couldn't create release",
                    id = -1
                });
            }

            return BadRequest(new
            {
                success = false,
                message = $"Couldn't get product from id: {id}",
                id = -1
            });
        }
        return BadRequest(new
        {
            success = false,
            message = "Couldn't authorize user",
            id = -1
        });
    }

    [HttpGet("download")]
    public IActionResult Download(int product_id, long version_id, Platform platform)
    {
        ProductModel? product = ProductModel.GetByID(product_id);
        if (product != null)
        {
            VersionModel? version = product.Versions[version_id];
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

    [HttpDelete("remove-version")]
    public async Task<IActionResult> RemoveVersion(int id, int product)
    {
        if (IsLoggedIn(Request.Cookies, out UserModel? user))
        {
            if (ProductModel.TryGetByID(product, out ProductModel? model))
            {
                if (model.User.Equals( user))
                {
                    if (model.Versions.ContainsKey(id))
                    {
                        if (await GitHandler.RemoveVersion(user.GitCredentials, model, model.Versions[id]))
                        {
                            return Ok(new
                            {
                                message = "Version removed successfully!"
                            });
                        }
                        else
                        {
                            return BadRequest(new
                            {
                                message = "Unable to remove version"
                            });
                        }
                    }

                    return BadRequest(new
                    {
                        message = $"Could not find version with an id of {id} under product \"{model.Name}\""
                    });
                }

                return BadRequest(new
                {
                    message = "User is not authorized!"
                });
            }

            return BadRequest(new
            {
                message = $"Could not find product with an id of {product}"
            });
        }
        return BadRequest(new
        {
            message = "User not logged in!"
        });

    }

    [HttpPatch("update-version")]
    public async Task<IActionResult> UpdateVersion([FromQuery]int id, [FromQuery]int product, [FromForm]string name, [FromForm] string changelog, [FromForm] ReleaseType type)
    {
        if (IsLoggedIn(Request.Cookies, out UserModel? user))
        {
            if (ProductModel.TryGetByID(product, out ProductModel? model))
            {
                if (model.User.Equals(user))
                {
                    if (model.Versions.ContainsKey(id))
                    {
                        if (await GitHandler.UpdateVersion(user.GitCredentials, model, model.Versions[id], name, type, changelog))
                        {
                            return Ok(new
                            {
                                message = "Version updated successfully!"
                            });
                        }
                        else
                        {
                            return BadRequest(new
                            {
                                message = "Unable to update version"
                            });
                        }
                    }

                    return BadRequest(new
                    {
                        message = $"Could not find version with an id of {id} under product \"{model.Name}\""
                    });
                }

                return BadRequest(new
                {
                    message = "User is not authorized!"
                });
            }

            return BadRequest(new
            {
                message = $"Could not find product with an id of {product}"
            });
        }
        return BadRequest(new
        {
            message = "User not logged in!"
        });
    }

    [HttpGet("get")]
    public IActionResult GetProduct(int id)
    {
        return new JsonResult(ProductModel.GetByID(id));
    }
    [HttpPost("trigger-version-check")]
    public IActionResult TriggerVersionCheck([FromQuery] int product_id)
    {
        if (ProductModel.TryGetByID(product_id, out ProductModel? model))
        {
            model.PopulateVersionsFromGit();
            return Ok(new
            {
                model.Versions
            });
        }
        return BadRequest(new
        {
            message = "Product Doesn't Exist"
        });
    }
    [HttpPost("upload-version-asset"), DisableRequestSizeLimit]
    public async Task<IActionResult> UploadAsset([FromQuery] int id, [FromQuery] int release_id, [FromQuery] Platform platform, [FromForm] IFormFile file)
    {
        if (IsLoggedIn(Request.Cookies, out UserModel user))
        {
            if (ProductModel.TryGetByID(id, out ProductModel product))
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
                message = $"Unable to find product with id of {id}"
            });
        }
        return BadRequest(new
        {
            message = "User couldn't be authenticated"
        });
    }

    #endregion Public Methods

}