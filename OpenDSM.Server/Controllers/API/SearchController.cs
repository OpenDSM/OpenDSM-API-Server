using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Handlers;
using OpenDSM.Core.Models;

namespace OpenDSM.Server.Controllers.API;

[ApiController]
[Route("/api/search")]
public class SearchController : ControllerBase
{
    [HttpGet("users")]
    public IActionResult Users([FromQuery] string query, [FromQuery] int? maxSize)
    {
        UserModel[] users = UserListHandler.GetUserFromPartials(maxSize.GetValueOrDefault(-1), query.Split(" "));
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

    [HttpGet("applications")]

    public IActionResult Applications([FromQuery] string query, [FromQuery] int? maxSize, [FromQuery] string? tags)
    {
        int[] tagsList = Array.Empty<int>();
        if (!string.IsNullOrEmpty(tags))
        {
            tagsList = Array.ConvertAll(tags.Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), i=>CLMath.CLConverter.ToInt32(i));
        }
        ProductModel[] products = ProductListHandler.GetProductsFromPartial(maxSize.GetValueOrDefault(-1), query, tagsList);
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
                rating = ReviewListHandler.GetProductAverageRating(product),
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
