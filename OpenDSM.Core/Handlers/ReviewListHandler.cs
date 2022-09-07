using OpenDSM.Core.Models;
using Org.BouncyCastle.Utilities;

namespace OpenDSM.Core.Handlers;

/// <summary>
/// The denomination of review ratings
/// </summary>
/// <param name="five">The number of five star ratings</param>
/// <param name="four">The number of four star ratings</param>
/// <param name="three">The number of three star ratings</param>
/// <param name="two">The number of two star ratings</param>
/// <param name="one">The number of one star ratings</param>
/// <param name="average">The average rating</param>
/// <param name="count">The number of ratings stored</param>
/// <returns></returns>
public record RatingDenominations(byte five, byte four, byte three, byte two, byte one, byte average, int count);
public static class ReviewListHandler
{

    #region Public Methods

    /// <summary>
    /// Returns the denomination of review ratings
    /// </summary>
    /// <param name="product">The product</param>
    /// <returns></returns>
    public static RatingDenominations GetProductRatingDenomination(ProductModel product)
    {
        ReviewModel[] reviews = GetProductReviews(product, int.MaxValue, 0).Values.ToArray();
        if (!reviews.Any())
        {
            return new(0, 0, 0, 0, 0, 0, 0);
        }
        int ratings = 0;
        byte five = 0;
        byte four = 0;
        byte three = 0;
        byte two = 0;
        byte one = 0;
        foreach (ReviewModel review in reviews)
        {
            ratings += review.Rating;
            int whole = 10 * (int)Math.Round(review.Rating / 10.0);

            if (whole >= 50)
            {
                five++;
            }
            else if (whole >= 40)
            {
                four++;
            }
            else if (whole >= 30)
            {
                three++;
            }
            else if (whole >= 20)
            {
                two++;
            }
            else
            {
                one++;
            }
        }

        byte rating = (byte)(Math.Round(ratings / reviews.Length / 5.0) * 5);
        rating = rating > 50 ? (byte)50 : rating;
        five = (byte)((float)five / reviews.Length * 100);
        four = (byte)((float)four / reviews.Length * 100);
        three = (byte)((float)three / reviews.Length * 100);
        two = (byte)((float)two / reviews.Length * 100);
        one = (byte)((float)one / reviews.Length * 100);
        return new(five, four, three, two, one, rating, reviews.Length);
    }

    /// <summary>
    /// Returns a dictionary of product reviews
    /// </summary>
    /// <param name="product">The product the reviews are under</param>
    /// <param name="count">The number of reviews to return</param>
    /// <param name="page">The page offset of the list</param>
    /// <returns>A dictionary containing the id and review object</returns>
    public static Dictionary<long, ReviewModel> GetProductReviews(ProductModel product, int count, int page)
    {
        Dictionary<long, ReviewModel> Reviews = new();
        int[] review_ids = SQL.Reviews.GetReviewsByProductID(product.Id, count, page);
        foreach (long id in review_ids)
        {
            if (SQL.Reviews.GetReviewByID(id, out _, out byte rating, out string summery, out string body, out DateTime posted, out int user_id))
            {
                Reviews.Add(id, new()
                {
                    Posted = posted,
                    Product = product,
                    Rating = rating,
                    Subject = summery,
                    User = UserListHandler.GetByID(user_id),
                    Body = body
                });
            }
        }
        return Reviews;
    }
    /// <summary>
    /// Attempts to get a single product review
    /// </summary>
    /// <param name="product">The product object</param>
    /// <param name="id">The id of the review</param>
    /// <param name="review">The review object</param>
    /// <returns>If the review was found</returns>
    public static bool TryGetProductReview(ProductModel product, long id, out ReviewModel review) => (review = GetProductReview(product, id)) != null;

    /// <summary>
    /// Gets a single product review or returns null if none was found
    /// </summary>
    /// <param name="product">The product object</param>
    /// <param name="id">The id of the review</param>
    /// <returns>The review object or null if none was found</returns>
    public static ReviewModel GetProductReview(ProductModel product, long id)
    {
        if (SQL.Reviews.GetReviewByID(id, out _, out byte rating, out string summery, out string body, out DateTime posted, out int user_id))
        {
            return new()
            {
                Posted = posted,
                Product = product,
                Rating = rating,
                Subject = summery,
                User = UserListHandler.GetByID(user_id),
                Body = body
            };
        }
        return null;
    }

    /// <summary>
    /// Creates a review
    /// </summary>
    /// <param name="user">The user that created the review</param>
    /// <param name="product">The product the review is under</param>
    /// <param name="rating">The rating of the review from 0-50</param>
    /// <param name="summery">The short summery of the review</param>
    /// <param name="body">The review markdown body</param>
    public static void CreateReview(UserModel user, ProductModel product, byte rating, string summery, string body)
    {
        SQL.Reviews.CreateReview(product.Id, rating, summery, body, user.Id);
    }

    #endregion Public Methods

}
