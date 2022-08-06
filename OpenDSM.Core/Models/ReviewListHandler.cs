using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDSM.Core.Models;

public static class ReviewListHandler
{
    public static ReviewModel[] GetProductReviews(ProductModel product)
    {
        List<ReviewModel> Reviews = new();
        int[] review_ids = SQL.Reviews.GetReviewsByProductID(product.Id);
        foreach (int id in review_ids)
        {
            if (SQL.Reviews.GetReviewByID(id, out _, out byte rating, out string summery, out string body, out DateTime posted, out int user_id))
            {
                Reviews.Add(new()
                {
                    Posted = posted,
                    Product = product,
                    Rating = rating,
                    Summery = summery,
                    User = UserModel.GetByID(user_id)
                });
            }
        }
        return Reviews.ToArray();
    }
    public static byte GetProductAverageRating(ProductModel product)
    {
        ReviewModel[] reviews = GetProductReviews(product);
        int ratings = 0;
        foreach (ReviewModel review in reviews)
        {
            ratings += review.Rating;
        }
        byte rating = (byte)(Math.Round((ratings / reviews.Length) / 5.0) * 5);
        return rating > 50 ? (byte)50 : rating;
    }
}
