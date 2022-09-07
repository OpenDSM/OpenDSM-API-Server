using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Handlers;
using OpenDSM.Core.Models;

namespace OpenDSM.Server.Controllers.API;

[ApiController]
[Route("/api/search")]
public class SearchController : ControllerBase
{
    /// <summary>
    /// Searches for user based on a query
    /// </summary>
    /// <param name="query">The search query</param>
    /// <param name="count">The max number of results</param>
    /// <returns></returns>
    [HttpGet("users")]
    public IActionResult Users([FromQuery] string query, [FromQuery] int? count, [FromQuery] int? page)
    {
        UserModel[] users = UserListHandler.GetUserFromPartials(count.GetValueOrDefault(20), page.GetValueOrDefault(0), query.Split(" "));
        object[] usersNeutered = new object[users.Length];
        Parallel.For(0, usersNeutered.Length, i =>
        {
            UserModel user = users[i];
            usersNeutered[i] = new
            {
                user.Id,
                user.Username,
                products = user.CreatedProducts.Length,
                user.IsDeveloperAccount
            };
        });
        return new JsonResult(usersNeutered);
    }

    /// <summary>
    /// Searches for applications based on a query
    /// </summary>
    /// <param name="query">The search query</param>
    /// <param name="count">The max number of results</param>
    /// <param name="tags">Any tags to filter results</param>
    /// <returns></returns>
    [HttpGet("applications")]
    public IActionResult Applications([FromQuery] string query, [FromQuery] int? count, [FromQuery] int? page, [FromQuery] string? tags)
    {
        int[] tagsList = Array.Empty<int>();
        if (!string.IsNullOrEmpty(tags))
        {
            tagsList = Array.ConvertAll(tags.Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), i => CLMath.CLConverter.ToInt32(i));
        }
        ProductModel[] products = ProductListHandler.GetProductsFromPartial(count.GetValueOrDefault(20), page.GetValueOrDefault(0), query, tagsList);
        object[] productsNeutered = new object[products.Length];
        Parallel.For(0, productsNeutered.Length, i =>
        {
            ProductModel product = products[i];
            productsNeutered[i] = new
            {
                product.Id,
                product.Name,
                downloads = product.TotalDownloads,
                views = product.TotalPageViews,
                product.ShortSummery,
                product.Price,
                rating = ReviewListHandler.GetProductRatingDenomination(product),
                product.Platforms,
                author = new
                {
                    id = product.User.Id,
                    name = product.User.Username,
                }
            };
        });
        return new JsonResult(productsNeutered);
    }
}
