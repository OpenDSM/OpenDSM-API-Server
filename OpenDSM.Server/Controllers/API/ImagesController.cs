using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Handlers;
using OpenDSM.Core.Models;

namespace OpenDSM.Server.Controllers.API;

/// <summary>
/// The type of image<br />
/// Ex: User or Product
/// </summary>
public enum ImageType
{
    User,
    Product
}

/// <summary>
/// Controller handling images
/// </summary>
[ApiController]
[Route("api/images")]
public class ImagesController : ControllerBase
{

    /// <summary>
    /// Gets image based on parameter
    /// </summary>
    /// <param name="type">The <see cref="ImageType">type</see> of the image, User/Product</param>
    /// <param name="id">The id of the User/Product</param>
    /// <param name="name">The name of the image: profile/banner/icon</param>
    /// <returns></returns>
    [HttpGet("{type}/{id}/{name}")]
    public IActionResult GetImage([FromRoute] ImageType type, [FromRoute] int id, [FromRoute] string name)
    {
        if (type == ImageType.User)
        {
            if (UserListHandler.TryGetByID(id, out UserModel? user))
            {
                if (name.ToLower().Equals("profile"))
                {
                    FileStream fs = new(user.ProfileImage, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    return new FileStreamResult(fs, "image/jpg");
                }
                else if (name.ToLower().Equals("banner"))
                {
                    FileStream fs = new(user.BannerImage, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    return new FileStreamResult(fs, "image/jpg");
                }

                return BadRequest(new
                {
                    message = $"Unknown user image name: \"{name}\""
                });
            }

            return BadRequest(new
            {
                message = $"No user with id of '{id}' was found"
            });
        }
        else if (type == ImageType.Product)
        {
            if (ProductListHandler.TryGetByID(id, out ProductModel product))
            {
                if (name.ToLower().Equals("icon"))
                {
                    FileStream fs = new(product.IconImage, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    return new FileStreamResult(fs, "image/jpg");
                }
                else if (name.ToLower().Equals("banner"))
                {
                    FileStream fs = new(product.BannerImage, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    return new FileStreamResult(fs, "image/jpg");
                }
                else
                {
                    string image = product.GetGalleryImage(name);
                    if (!string.IsNullOrWhiteSpace(image))
                    {
                        FileStream fs = new(image, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        return new FileStreamResult(fs, "image/jpg");
                    }
                }
                return BadRequest(new
                {
                    message = $"Unknown productUserModel image name: \"{name}\""
                });
            }
            return BadRequest(new
            {
                message = $"No product with id of '{id}' was found"
            });
        }

        return BadRequest(new
        {
            message = "Unknown type"
        });
    }

    /// <summary>
    /// Uploads Image based on parameters
    /// </summary>
    /// <param name="type">The <see cref="ImageType">type</see> of the image, User/Product</param>
    /// <param name="id">The id of the User/Product</param>
    /// <param name="name">The name of the image: profile/banner/icon</param>
    /// <param name="image">The base64 image /p:PublishProfile=representation</param>
    /// <returns></returns>
    [HttpPost("{type}/{id?}/{name}")]
    public IActionResult UploadImage([FromRoute] ImageType type, [FromRoute] int? id, [FromRoute] string name, [FromBody] string image)
    {
        if (string.IsNullOrWhiteSpace(image))
        {
            return BadRequest(new
            {
                message = "No image base64 was provided in body!"
            });
        }
        if (!FileHandler.IsValidBase64(image))
        {
            return BadRequest(new
            {
                message = "Body provided is not valid base64"
            });
        }
        if (type == ImageType.User)
        {
            if (IsLoggedIn(Request, out UserModel user))
            {
                if (name.ToLower().Equals("profile"))
                {
                    user.ProfileImage = image;
                }
                else if (name.ToLower().Equals("banner"))
                {
                    user.BannerImage = image;
                }
                return BadRequest(new
                {
                    message = $"Unknown image name: {name}"
                });
            }
        }
        else if (type == ImageType.Product)
        {
            if (id != null)
            {
                if (ProductListHandler.TryGetByID(id.Value, out ProductModel product))
                {
                    if (IsLoggedIn(Request, out UserModel user))
                    {
                        if (product.User.Equals(user))
                        {
                            if (name.ToLower().Equals("icon"))
                            {
                                product.IconImage = image;
                            }
                            else if (name.ToLower().Equals("banner"))
                            {
                                product.BannerImage = image;
                            }
                            else
                            {
                                try
                                {
                                    product.UploadGalleryImage(name, image);
                                }
                                catch (Exception e)
                                {
                                    return BadRequest(new
                                    {
                                        message = $"Unable to upload gallery image: {name}",
                                        error = new
                                        {
                                            e.Message,
                                            e.StackTrace
                                        }
                                    });
                                }
                            }
                        }
                        return BadRequest(new
                        {
                            message = $"You must be the products owner to modify it!"
                        });
                    }

                    return BadRequest(new
                    {
                        message = $"No authorization credentials found!"
                    });
                }
                return BadRequest(new
                {
                    message = $"No product with id of '{id.Value}' was found"
                });
            }
            return BadRequest(new
            {
                message = "Product id must be provided!"
            });
        }
        return BadRequest(new
        {
            message = "Unknown type"
        });
    }

    /// <summary>
    /// Deletes Images
    /// </summary>
    /// <param name="type">The <see cref="ImageType">type</see> of the image, User/Product</param>
    /// <param name="id">The id of the User/Product</param>
    /// <param name="name">The name of the image: profile/banner/icon</param>
    /// <returns></returns>
    [HttpDelete("{type}/{id?}/{name}")]
    public IActionResult DeleteImage([FromRoute] ImageType type, [FromRoute] int? id, [FromRoute] string name)
    {
        if (type == ImageType.User)
        {
            if (IsLoggedIn(Request, out UserModel user))
            {
                if (name.ToLower().Equals("profile"))
                {
                    if (user.HasProfileImage)
                    {
                        System.IO.File.Delete(user.ProfileImage);
                        return Ok(new
                        {
                            message = "Profile image deleted successfully"
                        });
                    }
                    return BadRequest(new
                    {
                        message = $"You don't have a custom profile image"
                    });
                }
                else if (name.ToLower().Equals("banner"))
                {
                    if (user.HasBannerImage)
                    {
                        System.IO.File.Delete(user.BannerImage);
                        return Ok(new
                        {
                            message = "Banner image deleted successfully"
                        });
                    }

                    return BadRequest(new
                    {
                        message = $"You don't have a custom banner image"
                    });
                }
                return BadRequest(new
                {
                    message = $"Unknown image name: {name}"
                });
            }
        }
        else if (type == ImageType.Product)
        {
            if (id != null)
            {
                if (ProductListHandler.TryGetByID(id.Value, out ProductModel product))
                {
                    if (IsLoggedIn(Request, out UserModel user))
                    {
                        if (product.User.Equals(user))
                        {
                            if (name.ToLower().Equals("icon") || name.ToLower().Equals("banner"))
                            {
                                return BadRequest(new { message = $"Can NOT delete product {name.ToLower()}, product must have an {name.ToLower()}" });
                            }
                            else
                            {
                                string image = product.GetGalleryImage(name);
                                if (!string.IsNullOrWhiteSpace(image))
                                {
                                    System.IO.File.Delete(image);
                                }
                            }
                        }
                        return BadRequest(new
                        {
                            message = $"You must be the products owner to modify it!"
                        });
                    }

                    return BadRequest(new
                    {
                        message = $"No authorization credentials found!"
                    });
                }
                return BadRequest(new
                {
                    message = $"No product with id of '{id.Value}' was found"
                });
            }
            return BadRequest(new
            {
                message = "Product id must be provided!"
            });
        }
        return BadRequest(new
        {
            message = "Unknown type"
        });
    }

    /// <summary>
    /// Gets additional information for products images
    /// </summary>
    /// <param name="id">the products id</param>
    /// <returns></returns>
    [HttpGet("info/product/{id}")]
    public IActionResult GetImagesInfo([FromRoute] int id)
    {
        if (ProductListHandler.TryGetByID(id, out ProductModel product))
        {

            long totalSize = 0;
            FileInfo info = new(product.IconImage);
            string name = info.Name.Replace(info.Extension, "");
            (int width, int height) = FFmpegHandler.Instance.GetSize(product.IconImage);
            long size = info.Length;
            totalSize += size;
            object icon = new
            {
                name,
                path = $"{(Request.IsHttps ? "https" : "http")}://{Request.Host}/api/images/product/{id}/{name}",
                bytes = size,
                size = CLMath.CLFileMath.AdjustedFileSize(size),
                dimensions = new
                {
                    width,
                    height
                },

            };


            info = new(product.BannerImage);
            name = info.Name.Replace(info.Extension, "");
            (width, height) = FFmpegHandler.Instance.GetSize(product.BannerImage);
            size = info.Length;
            totalSize += size;
            object banner = new
            {
                name,
                path = $"{(Request.IsHttps ? "https" : "http")}://{Request.Host}/api/images/product/{id}/{name}",
                bytes = size,
                size = CLMath.CLFileMath.AdjustedFileSize(size),
                dimensions = new
                {
                    width,
                    height
                },

            };


            string[] images = product.GalleryImages;
            object[] gallery = new object[images.Length];
            for (int i = 0; i < images.Length; i++)
            {
                info = new(images[i]);
                name = info.Name.Replace(info.Extension, "");
                (width, height) = FFmpegHandler.Instance.GetSize(images[i]);
                size = info.Length;
                totalSize += size;
                gallery[i] = new
                {
                    name,
                    path = $"{(Request.IsHttps ? "https" : "http")}://{Request.Host}/api/images/product/{id}/{name}",
                    bytes = size,
                    size = CLMath.CLFileMath.AdjustedFileSize(size),
                    dimensions = new
                    {
                        width,
                        height
                    },

                };
            }
            return new JsonResult(new
            {
                icon,
                banner,
                gallery,
                bytes = totalSize,
                size = CLMath.CLFileMath.AdjustedFileSize(totalSize),
                files = gallery.Length + 2
            });
        }
        return BadRequest(new
        {
            message = $"No product with id of '{id}' exists"
        });
    }
}