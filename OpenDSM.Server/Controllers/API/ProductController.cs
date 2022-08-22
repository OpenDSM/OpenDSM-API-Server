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

    [HttpPost("AddToLibrary")]
    public async Task<IActionResult> AddToLibrary([FromQuery] int product_id, [FromQuery] string? coupon)
    {
        if (IsLoggedIn(Request.Cookies, out UserModel? user))
        {
            if (!user.OwnedProducts.ContainsKey(product_id))
            {
                if (ProductModel.TryGetByID(product_id, out ProductModel? product))
                {
                    if (product.Price == 0 || product.SalePrice == 0 || (!string.IsNullOrWhiteSpace(coupon) && product.Coupon.ContainsKey(coupon) && product.Coupon[coupon] == 0))
                    {
                        user.AddToLibrary(product);
                    }

                    return BadRequest(new
                    {
                        message = $"'{product.Name}' cost '{product.Price}' and requires payment method to be included"
                    });
                }
                return BadRequest(new
                {
                    message = $"Product with id of '{product_id}' either never existed or no longer exists!"
                });
            }

            return BadRequest(new
            {
                message = $"User already owns product with id of '{product_id}'"
            });
        }
        return BadRequest(new
        {
            message = "User must be logged in!"
        });
    }

    [HttpPost("AddToLibrary")]
    public async Task<IActionResult> AddToLibrary([FromQuery] int product_id, [FromQuery] string? coupon, [FromForm] string card, [FromForm] string date, [FromForm] string cvv)
    {
        if (IsLoggedIn(Request.Cookies, out UserModel? user))
        {
            if (!user.OwnedProducts.ContainsKey(product_id))
            {
                if (ProductModel.TryGetByID(product_id, out ProductModel? product))
                {
                    if (product.Price == 0 || product.SalePrice == 0 || (!string.IsNullOrWhiteSpace(coupon) && product.Coupon.ContainsKey(coupon) && product.Coupon[coupon] == 0))
                    {
                        user.AddToLibrary(product);
                    }

                    return BadRequest(new
                    {
                        message = $"'{product.Name}' cost '{product.Price}' and requires payment method to be included"
                    });
                }
                return BadRequest(new
                {
                    message = $"Product with id of '{product_id}' either never existed or no longer exists!"
                });
            }

            return BadRequest(new
            {
                message = $"User already owns product with id of '{product_id}'"
            });
        }
        return BadRequest(new
        {
            message = "User must be logged in!"
        });
    }

    [HttpPost()]
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

    [HttpPost("{product_id}/review")]
    public async Task<IActionResult> CreateReview([FromRoute] int product_id, [FromForm] byte rating, [FromForm] string summery, [FromForm] string body)
    {

        if (IsLoggedIn(Request.Cookies, out UserModel user))
        {
            if (ProductModel.TryGetByID(product_id, out ProductModel product))
            {
                if (user.OwnedProducts[product_id] != null)
                {

                }

                return BadRequest(new
                {
                    message = $"You must be a verified owner of '{product.Name}' to leave a review"
                });
            }
            return BadRequest(new
            {
                message = $"No product with an id of {product_id} exists!"
            });
        }

        return BadRequest(new
        {
            message = "User must be logged in to leave a review"
        });
    }

    [HttpPost("{id}/version")]
    public IActionResult CreateVersion([FromRoute] int id, [FromForm] string name, [FromForm] ReleaseType type, [FromForm] string changelog)
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

    [HttpGet("{product_id}/version/{version_id}")]
    public IActionResult DownloadVersion(int product_id, long version_id, Platform platform)
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

    [HttpGet("{id}")]
    public IActionResult GetProduct(int id)
    {
        return new JsonResult(ProductModel.GetByID(id));
    }
    [HttpDelete("{product}/version/{id}")]
    public async Task<IActionResult> RemoveVersion(int id, int product)
    {
        if (IsLoggedIn(Request.Cookies, out UserModel? user))
        {
            if (ProductModel.TryGetByID(product, out ProductModel? model))
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

    [HttpPatch("{product}/version/{id}")]
    public async Task<IActionResult> UpdateVersion([FromRoute] int id, [FromRoute] int product, [FromForm] string name, [FromForm] string changelog, [FromForm] ReleaseType type)
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
    [HttpPost("{product_id}/version/{version_id}/asset"), DisableRequestSizeLimit]
    public async Task<IActionResult> UploadAsset([FromRoute] int product_id, [FromRoute] int version_id, [FromQuery] Platform platform, [FromForm] IFormFile file)
    {
        if (IsLoggedIn(Request.Cookies, out UserModel user))
        {
            if (ProductModel.TryGetByID(product_id, out ProductModel product))
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
        if (ProductModel.TryGetByID(product_id, out ProductModel? product))
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
        if (IsLoggedIn(Request.Cookies, out UserModel? user))
        {
            if (ProductModel.TryGetByID(product_id, out ProductModel? product))
            {
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


    #endregion Public Methods
}