using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Handlers;
using OpenDSM.Core.Models;

namespace OpenDSM.Server.Controllers.API;
/// <summary>
/// Controls all product releases based on the specified product id
/// </summary>
[ApiController]
[Route("api/products/{product_id}/releases")]
public class ReleaseController : ControllerBase
{
    /// <summary>
    /// Gets all releases for a product
    /// </summary>
    /// <param name="product_id">The id of the product</param>
    /// <returns></returns>
    [HttpGet()]
    public IActionResult GetReleases([FromRoute] int product_id)
    {
        if (ProductListHandler.TryGetByID(product_id, out ProductModel product))
        {
            if (product.Price == 0 || (IsLoggedIn(Request, out UserModel user) && user.OwnedProducts.ContainsKey(product_id)))
            {
                return new JsonResult(product.Versions);
            }

            return BadRequest(new
            {
                message = $"User is not authorized to view '{product.Name}' releases"
            });
        }
        return BadRequest(new
        {
            message = $"Unable to find product with id of '{product_id}'"
        });
    }

    /// <summary>
    /// Gets a specific release based on product id and release id
    /// </summary>
    /// <param name="product_id">The id of the product</param>
    /// <param name="id">The id of the release</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public IActionResult GetRelease([FromRoute] int product_id, [FromRoute] long id)
    {
        if (ProductListHandler.TryGetByID(product_id, out ProductModel product))
        {
            if (product.Price == 0 || (IsLoggedIn(Request, out UserModel user) && user.OwnedProducts.ContainsKey(product_id)))
            {
                if (product.Versions.TryGetValue(id, out VersionModel version))
                {
                    return new JsonResult(version);
                }
                return BadRequest(new
                {
                    message = $"'{product.Name}' does not contain version with id of '{id}'"
                });
            }

            return BadRequest(new
            {
                message = $"User is not authorized to view '{product.Name}' releases"
            });
        }
        return BadRequest(new
        {
            message = $"Unable to find product with id of '{product_id}'"
        });
    }

    /// <summary>
    /// Creates a release for the product
    /// </summary>
    /// <param name="product_id">The id of the product</param>
    /// <param name="name">The name of the release. Ex: <i><b><u>v0.0.1</u></b></i></param>
    /// <param name="type">The <see cref="ReleaseType">release type</see></param>
    /// <param name="changelog">The changelog as base64</param>
    /// <returns></returns>
    [HttpPost()]
    public IActionResult CreateRelease([FromRoute] int product_id, [FromForm] string name, [FromForm] ReleaseType type, [FromForm] string changelog)
    {
        if (IsLoggedIn(Request, out UserModel user))
        {
            if (ProductListHandler.TryGetByID(product_id, out ProductModel? model))
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
                message = $"Couldn't get product from id: {product_id}",
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



    /// <summary>
    /// Deletes a release based on product id and release id
    /// </summary>
    /// <param name="product_id">The id of the product</param>
    /// <param name="id">The id of the release</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRelease([FromRoute] int product_id, [FromRoute] long id)
    {
        if (IsLoggedIn(Request, out UserModel? user))
        {
            if (ProductListHandler.TryGetByID(product_id, out ProductModel? model))
            {
                if (model.User.Equals(user))
                {
                    if (model.Versions.TryGetValue(id, out VersionModel version))
                    {
                        if (await GitHandler.DeleteVersion(user.GitCredentials, model, version))
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
                message = $"Could not find product with an id of {product_id}"
            });
        }
        return BadRequest(new
        {
            message = "User not logged in!"
        });

    }

    /// <summary>
    /// Updates the release information
    /// </summary>
    /// <param name="product_id">The id of the product</param>
    /// <param name="id">The id of the release</param>
    /// <param name="name">The updated name of the release</param>
    /// <param name="changelog">The updated changelog of the release</param>
    /// <param name="type">The updated <see cref="ReleaseType">release type</see></param>
    /// <returns></returns>
    [HttpPatch()]
    public async Task<IActionResult> UpdateRelease([FromRoute] int product_id, [FromRoute] int id, [FromForm] string name, [FromForm] string changelog, [FromForm] ReleaseType type)
    {
        if (IsLoggedIn(Request, out UserModel? user))
        {
            if (ProductListHandler.TryGetByID(product_id, out ProductModel? model))
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
                message = $"Could not find product with an id of {product_id}"
            });
        }
        return BadRequest(new
        {
            message = "User not logged in!"
        });
    }

    /// <summary>
    /// Forces the release list to update and fetch github's release list.
    /// This is run by the github webhook via the POST method
    /// </summary>
    /// <param name="product_id">The id of the product</param>
    /// <returns></returns>
    [HttpGet("scan")]
    [HttpPost("scan")]
    public IActionResult ScanForReleaseChanges([FromRoute] int product_id)
    {
        if (ProductListHandler.TryGetByID(product_id, out ProductModel? model))
        {
            int updated_elements = model.PopulateVersionsFromGit();
            return Ok(new
            {
                message = $"Successfully updated {updated_elements} releases"
            });
        }
        return BadRequest(new
        {
            message = "Product Doesn't Exist"
        });
    }
}