using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Handlers;
using OpenDSM.Core.Models;

namespace OpenDSM.Server.Controllers;

[ApiController]
[Route("/search")]
public class SearchController : ControllerBase
{
    #region Public Methods

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
        for (int i = 0; i < products.Length; i++)
        {
            ProductModel product = products[i];
            productsNeutered[i] = product.ToObject();
        }
        return new JsonResult(productsNeutered);
    }

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
        for (int i = 0; i < users.Length; i++)
        {
            UserModel user = users[i];
            usersNeutered[i] = user.ToObject();
        }
        return new JsonResult(usersNeutered);
    }

    #endregion Public Methods
}
