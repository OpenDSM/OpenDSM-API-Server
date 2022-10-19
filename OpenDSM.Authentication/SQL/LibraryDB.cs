// LFInteractive LLC. - All Rights Reserved
using MySql.Data.MySqlClient;
using OpenDSM.Authentication.Models;
using OpenDSM.SQL;

namespace OpenDSM.Authentication.SQL;

internal static class LibraryDB
{
    #region Public Methods

    public static UserLibraryItemModel? GetLibraryItem(int user_id, int product_id)
    {
        MySqlDataReader reader = Requests.Select(
            table: table,
            column: "*",
            where: new(new IndividualWhereClause[]
            {
                new("user_id", user_id, "="),
                new("product_id", product_id, "=")
            }),
            limit: 1
        );
        if (reader.Read())
        {
            float price = reader.GetFloat("purchase_price");
            DateTime purchased_date = reader.GetDateTime("purchased_date");
            DateTime last_used = reader.GetDateTime("last_used");
            long used_time = reader.GetInt64("used_time");

            return new()
            {
                ProductID = product_id,
                Price = price,
                Purchased = purchased_date,
                LastUsed = last_used,
                UseTime = new TimeSpan(used_time),
            };
        }
        return null;
    }

    public static ICollection<UserLibraryItemModel> GetLibraryItems(int user_id)
    {
        MySqlDataReader reader = Requests.Select(
            table: table,
            column: "*",
            where: new(new IndividualWhereClause[]
            {
                new("user_id", user_id, "=")
            })
        );
        ICollection<UserLibraryItemModel> collection = new List<UserLibraryItemModel>();
        while (reader.Read())
        {
            int product_id = reader.GetInt32("product_id");
            float price = reader.GetFloat("purchase_price");
            DateTime purchased_date = reader.GetDateTime("purchased_date");
            DateTime last_used = reader.GetDateTime("last_used");
            long used_time = reader.GetInt64("used_time");

            collection.Add(new()
            {
                ProductID = product_id,
                Price = price,
                Purchased = purchased_date,
                LastUsed = last_used,
                UseTime = new TimeSpan(used_time),
            });
        }
        return collection;
    }

    #endregion Public Methods

    #region Private Fields

    private static readonly string table = "user_libraries";

    #endregion Private Fields
}