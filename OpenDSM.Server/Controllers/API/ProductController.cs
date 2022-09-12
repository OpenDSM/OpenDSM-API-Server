// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Handlers;
using OpenDSM.Core.Models;
using Tags = OpenDSM.Core.Models.Tags;
namespace OpenDSM.Server.Controllers.API;

[Route("/products")]
public class ProductController : ControllerBase
{

    #region Public Methods

    /// <summary>
    /// Gets a list of all products base on parameters
    /// </summary>
    /// <param name="type">The sorting method <see cref="ProductListType" /></param>
    /// <param name="page">The page offset of the list</param>
    /// <param name="items_per_page">The number of items to show</param>
    /// <returns></returns>
    [HttpGet()]
    public IActionResult GetProducts([FromQuery] ProductListType? type, [FromQuery] int? page, [FromQuery] int? items_per_page)
    {
        ProductModel[] productModels = ProductListHandler.GetProducts(page.GetValueOrDefault(0), items_per_page.GetValueOrDefault(20), type.GetValueOrDefault(ProductListType.Latest));
        object[] products = new object[productModels.Count()];

        Parallel.For(0, products.Count(), i =>
        {
            products[i] = productModels[i].ToObject();
        });

        return new JsonResult(products);
    }

    /// <summary>
    /// Creates a product listing
    /// </summary>
    /// <param name="name">The name of the product</param>
    /// <param name="git_repo_name">The repository name</param>
    /// <param name="short_summery">A short summery of the product</param>
    /// <param name="user_id">The user creating the product</param>
    /// <param name="yt_key">The youtube key of the trailer/video</param>
    /// <param name="subscription">If the product uses a subscription payment system</param>
    /// <param name="use_git_readme">If the product should use the readme from the github repository</param>
    /// <param name="price">the overall price of the product </param>
    /// <param name="keywords">any keywords for the product separated by semi-colon, this is used primarily for search engine optimization</param>
    /// <param name="tags">The tags/categories of the product separated by semi-colon, this is used primarily for search engine optimization</param>
    /// <param name="icon">The base64 of the icon/logo image</param>
    /// <param name="banner">The base64 of the banner/hero image</param>
    /// <returns></returns>
    [HttpPost()]
    public IActionResult CreateProduct([FromForm] string name, [FromForm] string git_repo_name, [FromForm] string short_summery, [FromForm] string? yt_key, [FromForm] bool subscription, [FromForm] bool use_git_readme, [FromForm] float price, [FromForm] string keywords, [FromForm] string tags, [FromForm] string icon, [FromForm] string banner)
    {
        if (IsLoggedIn(Request, out UserModel? user))
        {
            if (user.IsDeveloperAccount)
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
                    if (ProductListHandler.TryCreateProduct(git_repo_name, short_summery, user, name, yt_key ?? "", subscription, use_git_readme, (int)(price * 100), keywords.Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), ts.ToArray(), out ProductModel model))
                    {
                        model.IconImage = icon;
                        model.BannerImage = banner;
                        return new JsonResult(model);
                    }
                }
                catch (Exception e)
                {
                    log.Error(e.Message, e);

                    return BadRequest(new
                    {
                        message = "Unable to create product",
                        error = new
                        {
                            e.Message,
                            e.StackTrace
                        }
                    });
                }
                return BadRequest(new
                {
                    message = "Unable to create product"
                });
            }
            return BadRequest(new
            {
                message = "Users developer account is not active"
            });
        }
        return BadRequest(new
        {
            message = "User is not logged in"
        });
    }

    /// <summary>
    /// Gets information on a specific product based on the product id
    /// </summary>
    /// <param name="id">The id of the product</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public IActionResult GetProduct([FromRoute] int id)
    {
        if (ProductListHandler.TryGetByID(id, out ProductModel product))
            return new JsonResult(product.ToObject());
        return BadRequest(new
        {
            message = $"No product was found with id of {id}"
        });
    }

    /// <summary>
    /// Returns a list of all acceptable tags and their corresponding id
    /// </summary>
    /// <returns></returns>
    [HttpGet("tags")]
    public IActionResult GetTags()
    {
        return new JsonResult(Tags.GetTags());
    }

    #endregion Public Methods
}