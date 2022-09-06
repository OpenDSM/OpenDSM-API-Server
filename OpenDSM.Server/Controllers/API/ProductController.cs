// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Handlers;
using OpenDSM.Core.Models;
using OpenDSM.SQL;
using Tags = OpenDSM.Core.Models.Tags;
namespace OpenDSM.Server.Controllers.API;

[Route("api/product")]
public class ProductController : ControllerBase
{

    #region Public Methods

    [HttpGet()]
    public IActionResult GetProducts(ProductListType? type, int? page, int? items_per_page)
    {
        ProductModel[] productModels = ProductListHandler.GetProducts(page.GetValueOrDefault(0), items_per_page.GetValueOrDefault(20), type.GetValueOrDefault(ProductListType.Latest));
        object[] products = new object[productModels.Count()];

        Parallel.For(0, products.Count(), i =>
        {
            products[i] = productModels[i].ToObject();
        });

        return new JsonResult(products);
    }

    [HttpPost()]
    public IActionResult CreateProduct([FromForm] string name, [FromForm] string gitRepoName, [FromForm] string shortSummery, [FromForm] int user_id, [FromForm] string? yt_key, [FromForm] bool subscription, [FromForm] bool use_git_readme, [FromForm] float price, [FromForm] string keywords, [FromForm] string tags, [FromForm] string icon, [FromForm] string banner)
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
            if (ProductListHandler.TryCreateProduct(gitRepoName, shortSummery, UserListHandler.GetByID(user_id), name, yt_key ?? "", subscription, use_git_readme, (int)(price * 100), keywords.Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), ts.ToArray(), out ProductModel model))
            {
                model.IconImage = icon;
                model.BannerImage = banner;
                return new JsonResult(model);
            }
        }
        catch (Exception e)
        {
            log.Error(e.Message, e);
        }
        return BadRequest();
    }

    [HttpPost("{id}/version")]
    public IActionResult CreateVersion([FromRoute] int id, [FromForm] string name, [FromForm] ReleaseType type, [FromForm] string changelog)
    {
        if (IsLoggedIn(Request, out UserModel user))
        {
            if (ProductListHandler.TryGetByID(id, out ProductModel? model))
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

    [HttpGet("{product_id}/version/{version_id}")]
    public IActionResult DownloadVersion(int product_id, long version_id, Platform platform)
    {
        if (ProductListHandler.TryGetByID(product_id, out ProductModel product))
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

    [HttpGet("{id}")]
    public IActionResult GetProduct(int id)
    {
        return new JsonResult(ProductListHandler.GetByID(id).ToObject());
    }
    [HttpDelete("{product}/version/{id}")]
    public async Task<IActionResult> RemoveVersion(int id, int product)
    {
        if (IsLoggedIn(Request, out UserModel? user))
        {
            if (ProductListHandler.TryGetByID(product, out ProductModel? model))
            {
                if (model.User.Equals(user))
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

    [HttpPost("{product_id}/version/check")]
    public IActionResult TriggerVersionCheck([FromRoute] int product_id)
    {
        if (ProductListHandler.TryGetByID(product_id, out ProductModel? model))
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

    [HttpPatch("{product}/version/{id}")]
    public async Task<IActionResult> UpdateVersion([FromRoute] int id, [FromRoute] int product, [FromForm] string name, [FromForm] string changelog, [FromForm] ReleaseType type)
    {
        if (IsLoggedIn(Request, out UserModel? user))
        {
            if (ProductListHandler.TryGetByID(product, out ProductModel? model))
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
    [HttpPost("{product_id}/version/{version_id}/asset"), DisableRequestSizeLimit]
    public async Task<IActionResult> UploadAsset([FromRoute] int product_id, [FromRoute] int version_id, [FromQuery] Platform platform, [FromForm] IFormFile file)
    {
        if (IsLoggedIn(Request, out UserModel user))
        {
            if (ProductListHandler.TryGetByID(product_id, out ProductModel product))
            {
                try
                {
                    if (await GitHandler.UploadReleaseAsset(user.GitCredentials, file.OpenReadStream(), product, platform, version_id))
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

    [HttpGet("{product_id}/reviews")]
    public IActionResult GetReviews([FromRoute] int product_id)
    {
        if (ProductListHandler.TryGetByID(product_id, out ProductModel? product))
        {
            return new JsonResult(ReviewListHandler.GetProductReviews(product));
        }

        return BadRequest(new
        {
            message = $"Unable to find product with id of {product_id}"
        });
    }

    [HttpPost("{product_id}/reviews")]
    public IActionResult CreateReview([FromRoute] int product_id, [FromForm] string summery, [FromForm] string body, [FromForm] byte rating)
    {
        if (IsLoggedIn(Request, out UserModel? user))
        {
            if (ProductListHandler.TryGetByID(product_id, out ProductModel? product))
            {
                if (user.Equals(product.User))
                {
                    return BadRequest(new
                    {
                        message = $"You cannot leave a review on your own product!"
                    });
                }
                if (user.OwnedProducts.ContainsKey(product_id))
                {
                    ReviewListHandler.CreateReview(user, product, rating, summery, body);
                    return Ok(new
                    {
                        message = "Review successfully submitted"
                    });
                }

                return BadRequest(new
                {
                    message = $"User is not a verified owner of '{product.Name}' and therefore can not leave a review"
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

    [HttpGet("tags")]
    public IActionResult GetTags()
    {

        return new JsonResult(OpenDSM.Core.Models.Tags.GetTags());
    }

    #endregion Public Methods
}