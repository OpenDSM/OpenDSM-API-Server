using MySql.Data.MySqlClient;
using CLMath;
namespace OpenDSM.SQL;

public static class Reviews
{

    #region Public Methods

    public static bool CreateReview(int product_id, byte rating, string summery, string body, int user_id)
    {
        using MySqlConnection conn = GetConnection();
        using MySqlCommand cmd = new($"insert into `review` (`product_id`, `rating`, `summery`, `body`, `user_id`) values ('{product_id}', '{rating}', '{summery}', '{CLConverter.EncodeBase64(body)}', '{user_id}')", conn);
        return cmd.ExecuteNonQuery() > 0;
    }

    public static bool GetReviewByID(long id, out int product_id, out byte rating, out string summery, out string body, out DateTime posted, out int user_id)
    {
        user_id = 0;
        product_id = 0;
        rating = 0;
        summery = "";
        body = "";
        posted = DateTime.Now;


        using MySqlConnection conn = GetConnection();
        using MySqlCommand cmd = new($"select * from `review` where `id`='{id}'", conn);
        using MySqlDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            rating = reader.GetByte("rating");
            product_id = reader.GetInt32("product_id");
            user_id = reader.GetInt32("user_id");
            summery = reader.GetString("summery");
            body = CLConverter.DecodeBase64(reader.GetString("body"));
            posted = reader.GetDateTime("posted");
            return true;
        }

        return false;
    }

    public static int[] GetReviewsByProductID(int product_id)
    {
        List<int> reviews = new();

        using MySqlConnection conn = GetConnection();
        using MySqlCommand cmd = new($"select `id` from `review` where `product_id`='{product_id}'", conn);
        using MySqlDataReader reader = cmd.ExecuteReader();
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                reviews.Add(reader.GetInt32(0));
            }
        }

        return reviews.ToArray();

    }

    #endregion Public Methods

}
