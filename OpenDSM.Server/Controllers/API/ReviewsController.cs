using Microsoft.AspNetCore.Mvc;
using OpenDSM.Core.Handlers;
using OpenDSM.Core.Models;

namespace OpenDSM.Server.Controllers.API
{
    [ApiController]
    [Route("api/reviews/{product_id}")]
    public class ReviewsController : ControllerBase
    {

        /// <summary>
        /// Gets all reviews for a product
        /// </summary>
        /// <param name="product_id"></param>
        /// <returns></returns>
        [HttpGet()]
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

        /// <summary>
        /// Creates a review for a product
        /// </summary>
        /// <param name="product_id">The id of the product</param>
        /// <param name="summery">The short summery of the review</param>
        /// <param name="body">The full body of the review</param>
        /// <param name="rating">The rating from 0-50</param>
        /// <returns></returns>
        [HttpPost()]
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

    }
}