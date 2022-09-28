// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using MySql.Data.MySqlClient;

namespace OpenDSM.SQL;

public enum Platform
{
    Windows,
    Mac,
    Linux,
    WindowsARM,
    MacARM,
    LinuxARM,
    Android,
    Java
}

public static class Products
{
    private static readonly string table = "products";
    #region Public Methods

    public static void AddPageView(int id)
    {
        using MySqlDataReader reader = Select(
            table: table,
            column: "page_views",
            new(new IndividualWhereClause[]{
                new("id", id, "=")
            }),
            limit: 1
        );
        if (reader.Read())
        {
            int page_views = reader.GetInt32(0);
            Update(
                table: table,
                new KeyValuePair<string, dynamic>[]{
                    new("page_views", page_views + 1)
                },
                where: new(new IndividualWhereClause[]{
                    new("id", id, "=")
                }),
                limit: 1
            );
        }
    }

    public static bool CheckProductExists(int id)
    {
        return Select(
            table: table,
            column: "id",
            where: new(new IndividualWhereClause[]{
                    new("id", id, "=")
            }),
            limit: 1,
            orderby: new("posted")
        ).HasRows;
    }

    public static bool Create(int user_id, string gitRepoName, string shortSummery, string name, string yt_key, bool subscription, bool use_git_readme, int price, string[] keywords, int[] tags, out int product_id)
    {
        product_id = -1;
        try
        {

            bool success = Insert(
                table: table,
                items: new KeyValuePair<string, dynamic>[]
                {
                    new("id", user_id),
                    new("git_repo_name", gitRepoName),
                    new("name", name),
                    new("youtube_key", yt_key),
                    new("price", price),
                    new("subscription", subscription),
                    new("keywords", string.Join(';',keywords)),
                    new("tags", string.Join(';',tags)),
                    new("use_git_readme", use_git_readme),
                    new("short_summery", shortSummery),
                }
            );
            if (success)
            {
                using MySqlDataReader reader = Select(
                    table: table,
                    column: "id",
                    where: new(new IndividualWhereClause[]{
                        new("user_id", user_id, "="),
                        new("name", name, "="),
                    }),
                    limit: 1
                );
                if (reader.Read())
                {
                    product_id = reader.GetInt32(0);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
        }
        return false;
    }

    public static int[] GetAllProductsWithTags(int count, int page, params int[] tags)
    {
        List<int> products = new();
        IndividualWhereClause[] clauses = new IndividualWhereClause[tags.Length];
        for (int i = 0; i < tags.Length; i++)
        {
            clauses[i] = new("tags", tags[i], "CONTAINS");
        }
        using MySqlDataReader reader = Select(
            table: table,
            column: "id",
            limit: count,
            offset: page * count,
            where: new(clauses)
        );
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                products.Add(reader.GetInt32(0));
            }
        }

        return products.ToArray();
    }

    public static int[] GetLatestProducts(int page, int count)
    {
        Dictionary<int, DateTime> products = new();
        using MySqlDataReader reader = Select(
            table: table,
            column: "id",
            limit: count,
            offset: page * count,
            orderby: new("posted")
        );
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                products.Add(reader.GetInt32(0), reader.GetDateTime(1));
            }
        }

        return products.Keys.ToArray();
    }

    public static bool TryGetProductFromID(int id, out string name, out string gitRepoName, out string summery, out bool useGitReadme, out bool subscription, out int[] tags, out string[] keywords, out int price, out string yt_key, out int owner_id, out int pageViews, out DateTime posted)
    {
        gitRepoName = "";
        name = "";
        summery = "";
        posted = DateTime.Now;
        pageViews = 0;
        useGitReadme = false;
        subscription = false;
        tags = Array.Empty<int>();
        keywords = Array.Empty<string>();
        price = 0;
        yt_key = "";
        owner_id = 0;

        using MySqlDataReader reader = Select(
            table: table,
            column: "*",
            where: new(new IndividualWhereClause[]{
                new("id", id, "=")
            })
        );
        if (reader.Read())
        {
            owner_id = reader.GetInt32("user_id");
            name = reader.GetString("name");
            posted = reader.GetDateTime("posted");
            summery = reader.GetString("short_summery");
            pageViews = reader.GetInt32("page_views");
            useGitReadme = reader.GetBoolean("use_git_readme");
            gitRepoName = reader.GetString("git_repo_name");
            subscription = reader.GetBoolean("subscription");
            tags = Array.ConvertAll(reader.GetString("tags").Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), tag => int.Parse(tag));
            keywords = reader.GetString("keywords").Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            try
            { yt_key = reader.GetString("youtube_key"); }
            catch { yt_key = ""; }

            price = reader.GetInt32("price");
            return true;
        }
        return false;
    }

    public static int[] GetProductsByOwner(int id, int count, int page)
    {
        List<int> products = new();

        using MySqlDataReader reader = Select(
            table: table,
            column: "id",
            where: new(new IndividualWhereClause[]{
                new("user_id", id, "=")
            }),
            orderby: new("posted"),
            limit: count,
            offset: count * page
        );
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                products.Add(reader.GetInt32("id"));
            }
        }
        return products.ToArray();
    }

    public static int[] GetProductsFromQuery(string query, int count, int page, params int[] tags)
    {
        Dictionary<int, int> products = new();
        IndividualWhereClause[] clauses = new IndividualWhereClause[tags.Length];
        for (int i = 0; i < tags.Length; i++)
        {
            clauses[i] = new("tags", tags[i], "CONTAINS");
        }

        string[] keywords = query.ToLower().Split(' ');

        using MySqlDataReader reader = Select(
            table: table,
            column: "*",
            offset: count * page,
            where: new(clauses)
        );
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                int matches = 0;
                string[] key = reader.GetString("keywords").Split(";", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in keywords)
                {
                    foreach (string k in key)
                    {
                        if (k.StartsWith(s))
                            matches++;
                    }
                }
                if (matches > 0)
                {
                    products.Add(reader.GetInt32("id"), matches);
                }
            }
        }
        products = products.OrderBy(i => i.Value).ToDictionary(i => i.Key, i => i.Value);
        return products.Keys.ToArray();
    }

    #endregion Public Methods

}