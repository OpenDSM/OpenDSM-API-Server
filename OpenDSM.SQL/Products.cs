// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using MySql.Data.MySqlClient;
using System.Text;

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

    #region Public Methods

    public static void AddPageView(int id)
    {

        MySqlCommand cmd;
        int page_views = 0;
        using MySqlConnection conn = GetConnection();
        using (cmd = new($"select `page_views` from `products` where `id` = {id} limit 1", conn))
        {
            using MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                page_views = reader.GetInt32("page_views");
            }
        }
        using (cmd = new($"UPDATE `products` SET `page_views` = {page_views + 1} WHERE `id` = {id} LIMIT 1", conn))
        {
            cmd.ExecuteNonQuery();
        }
    }

    public static bool CheckProductExists(int id)
    {
        try
        {
            using MySqlConnection conn = GetConnection();
            MySqlCommand cmd = new($"select * from `products` where `id` = '{id}'", conn);
            return cmd.ExecuteReader().HasRows;
        }
        catch
        {
            return false;
        }
    }

    public static bool Create(int user_id, string gitRepoName, string name, string yt_key, bool subscription, bool use_git_readme, int price, string[] keywords, int[] tags, out int product_id)
    {
        product_id = -1;
        try
        {
            StringBuilder builder = new();
            StringBuilder s_keyword = new();
            foreach (string word in keywords)
            {
                s_keyword.Append(word);
                s_keyword.Append(";");
            }
            StringBuilder s_tag = new();
            foreach (int tag in tags)
            {
                s_tag.Append(tag);
                s_tag.Append(";");
            }
            using MySqlConnection conn = GetConnection();
            MySqlCommand cmd = new($"INSERT INTO `products`( `user_id`, `git_repo_name`, `name`, `use_git_readme`, `youtube_key`, `price`, `subscription`, `tags`, `keywords`) VALUES ('{user_id}', '{gitRepoName}','{name}', '{(use_git_readme ? 1 : 0)}','{yt_key}','{price}','{(subscription ? 1 : 0)}','{s_tag}','{s_keyword}')", conn);
            if (cmd.ExecuteNonQuery() > 0)
            {
                MySqlCommand cmdf = new($"select id from products where user_id = '{user_id}' and name = '{name}'", conn);
                MySqlDataReader reader = cmdf.ExecuteReader();
                if (reader.Read())
                {
                    product_id = reader.GetInt32("id");
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

    public static int[] GetAllProductsWithTags(params int[] tags)
    {
        List<int> products = new();
        StringBuilder tagBuilder = new();
        for (int i = 0; i < tags.Length; i++)
        {
            if (i != 0)
            {
                tagBuilder.Append(" AND ");
            }
            tagBuilder.Append($"where tags contains {tags[i]}");
        }

        using MySqlConnection conn = GetConnection();
        using MySqlCommand cmd = new($"select * from `products` {tagBuilder}", conn);
        MySqlDataReader reader = cmd.ExecuteReader();
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                products.Add(reader.GetInt32("id"));
            }
        }

        return products.ToArray();
    }

    public static int[] GetLatestProducts(int page, int count)
    {
        Dictionary<int, DateTime> products = new();
        using MySqlConnection conn = GetConnection();
        using MySqlCommand cmd = new($"select id, posted from products order by posted offset {page * count} rows;", conn);
        using MySqlDataReader reader = cmd.ExecuteReader();
        if (reader.HasRows)
        {
            int current = 0;
            while (reader.Read() && current <= count)
            {
                products.Add(reader.GetInt32(0), reader.GetDateTime(1));
                current++;
            }
        }

        return products.OrderByDescending(i => i.Value).ToDictionary(i => i.Key, i => i.Value).Keys.ToArray();
    }

    public static bool GetProductFromID(int id, out string name, out string gitRepoName, out string summery, out bool useGitReadme, out bool subscription, out int[] tags, out string[] keywords, out int price, out string yt_key, out int owner_id, out int pageViews, out DateTime posted)
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

        using MySqlConnection conn = GetConnection();
        MySqlCommand cmd = new($"select * from products where id = '{id}'", conn);
        MySqlDataReader reader = cmd.ExecuteReader();
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
            yt_key = reader.GetString("youtube_key");
            price = reader.GetInt32("price");
            return true;
        }
        conn.Close();
        return false;
    }

    public static int[] GetProductsByOwner(int id)
    {
        List<int> products = new();

        using MySqlConnection conn = GetConnection();
        MySqlCommand cmd = new($"select id from products where user_id = '{id}'", conn);
        MySqlDataReader reader = cmd.ExecuteReader();
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                products.Add(reader.GetInt32("id"));
            }
        }
        return products.ToArray();
    }

    public static int[] GetProductsFromQuery(string query, int max, params int[] tags)
    {
        Dictionary<int, int> products = new();
        StringBuilder tagBuilder = new();
        for (int i = 0; i < tags.Length; i++)
        {
            if (i != 0)
            {
                tagBuilder.Append(" AND ");
            }
            tagBuilder.Append($"where tags contains {tags[i]}");
        }

        string[] keywords = query.ToLower().Split(' ');
        using MySqlConnection conn = GetConnection();
        using MySqlCommand cmd = new($"select * from `products` {tagBuilder}", conn);
        MySqlDataReader reader = cmd.ExecuteReader();
        if (reader.HasRows)
        {
            int count = 0;
            while (reader.Read() && (count <= max && max != -1))
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