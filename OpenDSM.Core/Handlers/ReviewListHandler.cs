using OpenDSM.Core.Models;

namespace OpenDSM.Core.Handlers;

public record RatingDenominations(byte five, byte four, byte three, byte two, byte one, byte average, int count);
public static class ReviewListHandler
{

    #region Public Methods
    public static RatingDenominations GetProductAverageRating(ProductModel product)
    {
        ReviewModel[] reviews = GetProductReviews(product).Values.ToArray();
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

    public static Dictionary<long, ReviewModel> GetProductReviews(ProductModel product)
    {
        Dictionary<long, ReviewModel> Reviews = new();
        int[] review_ids = SQL.Reviews.GetReviewsByProductID(product.Id);
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
        return Reviews.OrderByDescending(x => x.Value.Posted).ToDictionary(i => i.Key, i => i.Value);
    }

    public static void CreateReview(UserModel user, ProductModel product, byte rating, string summery, string body)
    {
        SQL.Reviews.CreateReview(product.Id, rating, summery, body, user.Id);
    }

    #endregion Public Methods

}
