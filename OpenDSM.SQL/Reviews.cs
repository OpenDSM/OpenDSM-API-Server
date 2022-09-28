using CLMath;
using MySql.Data.MySqlClient;
namespace OpenDSM.SQL;

public static class Reviews
{
    private static readonly string table = "review";
    #region Public Methods

    public static bool CreateReview(int product_id, byte rating, string summery, string body, int user_id)
    {
        return Insert(
            table: table,
            items: new KeyValuePair<string, dynamic>[]{
                new("product_id", product_id),
                new("rating", rating),
                new("summery", summery),
                new("body", CLConverter.EncodeBase64(body)),
                new("user_id", user_id)
            }
        );
    }

    public static bool GetReviewByID(long id, out int product_id, out byte rating, out string summery, out string body, out DateTime posted, out int user_id)
    {
        user_id = 0;
        product_id = 0;
        rating = 0;
        summery = "";
        body = "";
        posted = DateTime.Now;

        using MySqlDataReader reader = Select(
            table: table,
            column: "*",
            where: new(new IndividualWhereClause[]{
                new("id", id, "=")
            })
        );
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

    public static int[] GetReviewsByProductID(int product_id, int count, int page)
    {
        List<int> reviews = new();
        using MySqlDataReader reader = Select(
            table: table,
            column: "id",
            where: new(new IndividualWhereClause[]{
                new("product_id", product_id, "=")
            }),
            orderby: new("posted"),
            limit: count,
            offset: count * page
        );
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
