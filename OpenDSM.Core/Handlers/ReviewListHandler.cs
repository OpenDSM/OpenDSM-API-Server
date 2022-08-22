using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenDSM.Core.Models;

namespace OpenDSM.Core.Handlers;

public record RatingDenominations(byte five, byte four, byte three, byte two, byte one, byte zero, byte average, int count);
public static class ReviewListHandler
{

    #region Public Methods
    public static RatingDenominations GetProductAverageRating(ProductModel product)
    {
        ReviewModel[] reviews = GetProductReviews(product);
        if (!reviews.Any())
        {
            return new(0, 0, 0, 0, 0, 0, 0, 0);
        }
        int ratings = 0;
        byte five = 0;
        byte four = 0;
        byte three = 0;
        byte two = 0;
        byte one = 0;
        byte zero = 0;
        foreach (ReviewModel review in reviews)
        {
            ratings += review.Rating;
            if (review.Rating >= 50)
            {
                five++;
            }
            else if (review.Rating >= 40)
            {
                four++;
            }
            else if (review.Rating >= 30)
            {
                three++;
            }
            else if (review.Rating >= 20)
            {
                two++;
            }
            else if (review.Rating >= 10)
            {
                one++;
            }
            else
            {
                zero++;
            }
        }

        byte rating = (byte)(Math.Round(ratings / reviews.Length / 5.0) * 5);
        rating = rating > 50 ? (byte)50 : rating;
        return new((byte)(five / reviews.Length * 100), (byte)(four / reviews.Length * 100), (byte)(three / reviews.Length * 100), (byte)(two / reviews.Length * 100), (byte)(one / reviews.Length * 100), (byte)(zero / reviews.Length * 100), rating, reviews.Length);
    }

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

    public static void CreateReview(UserModel user, ProductModel product, byte rating, string summery, string body)
    {
        SQL.Reviews.CreateReview(product.Id, rating, summery, body, user.Id);
    }

    #endregion Public Methods

}
