using OpenDSM.Products.Models;
using OpenDSM.SQL;
using MySql.Data.MySqlClient;
using CLMath;

namespace OpenDSM.Products.SQL;

internal static class ProductDB
{
    public static readonly string table = "products";
    public static bool TryGetProduct(int product_id, out ProductModel product)
    {
        using (MySqlDataReader reader = Requests.Select(
            table: table,
            column: "*",
            where: new(new IndividualWhereClause[]
            {
                new("id", product_id, "=")
            }),
            limit: 1
        ))
        {
            if (reader.Read())
            {
                string name = reader.GetString("name");
                string short_description = CLConverter.DecodeBase64(reader.GetString("short_description") + "==");
                string slug = reader.GetString("slug");
                int owner_id = reader.GetInt32("owner_id");

                product = new ProductModel(product_id, name, short_description, slug, owner_id);
                return true;
            }
        }

        product = ProductModel.Empty;
        return false;
    }
    public static bool TryGetProduct(string slug, out ProductModel product)
    {
        using (MySqlDataReader reader = Requests.Select(
            table: table,
            column: "*",
            where: new(new IndividualWhereClause[]
            {
                new("slug", slug, "=")
            }),
            limit: 1
        ))
        {
            if (reader.Read())
            {
                int product_id = reader.GetInt32("id");
                string name = reader.GetString("name");
                string short_description = CLConverter.DecodeBase64(reader.GetString("short_description") + "==");
                int owner_id = reader.GetInt32("owner_id");

                product = new ProductModel(product_id, name, short_description, slug, owner_id);
                return true;
            }
        }

        product = ProductModel.Empty;
        return false;
    }
    public static ICollection<ProductModel> TryGetProductsFromUser(int user_id, int max_results, int page_offset)
    {
        List<ProductModel> models = new();
        using (MySqlDataReader reader = Requests.Select(
            table: table,
            column: "*",
            where: new(new IndividualWhereClause[]
            {
                new("owner_id", user_id, "=")
            }),
            limit: max_results,
            offset: page_offset * max_results,
            orderby: new("creation_date")
        ))
        {
            while (reader.Read())
            {
                int product_id = reader.GetInt32("id");
                string name = reader.GetString("name");
                string slug = reader.GetString("slug");
                string short_description = CLConverter.DecodeBase64(reader.GetString("short_description") + "==");
                models.Add(new ProductModel(product_id, name, short_description, slug, user_id));
            }
        }

        return models;

    }
    public static bool CheckIfSlugExists(string slug)
    {
        using MySqlDataReader reader = Requests.Select(
            table: table,
            column: "*",
            where: new(new IndividualWhereClause[]
            {
                new("slug", slug, "=")
            }),
            limit: 1
        );
        return reader.HasRows;

    }
}
