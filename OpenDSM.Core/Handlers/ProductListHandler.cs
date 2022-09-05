using OpenDSM.Core.Models;
using OpenDSM.SQL;

namespace OpenDSM.Core.Handlers;

public enum ProductListType
{
    Latest,
    Popular,
    Tag
}

public static class ProductListHandler
{

    #region Public Methods

    public static bool TryCreateProduct(string gitRepoName, string shortSummery, UserModel user, string name, string yt_key, bool subscription, bool use_git_readme, int price, string[] keywords, int[] tags, out ProductModel model)
    {
        model = null;
        if (Products.Create(user.Id, gitRepoName, shortSummery, name, yt_key, subscription, use_git_readme, price, keywords, tags, out int product_id) && TryGetByID(product_id, out model))
        {
            GitHandler.CreateWebHook(user.GitCredentials, model);
            model.PopulateVersionsFromGit();
            return true;
        }
        return false;
    }
    public static bool TryGetByID(int id, out ProductModel product)
    {
        return (product = GetByID(id)) != null;
    }

    public static ProductModel? GetByID(int id)
    {

        try
        {
            if (Products.GetProductFromID(id, out string name, out string gitRepoName, out string summery, out bool useGitReadme, out bool subscription, out int[] tags, out string[] keywords, out int price, out string yt_key, out int owner_id, out int pageViews, out DateTime posted))
            {
                return new ProductModel(id, owner_id, gitRepoName, name, summery, useGitReadme, yt_key, (uint)price, tags, keywords, subscription, pageViews, posted);
            }
        }
        catch { }
        return null;
    }

    public static ProductModel[] GetProducts(int count = 20, int page = 0, ProductListType type = ProductListType.Latest)
    {
        switch (type)
        {
            case ProductListType.Latest:
                return GetLatestProducts(page, count);
            case ProductListType.Popular:
                return GetPopularProducts(page, count);
            default:
                return GetLatestProducts(page, count);
        }
    }
    public static ProductModel[] GetProducts(int tag, int count = 20, int page = 0)
    {
        return GetPopularProducts(page, count);
    }

    public static ProductModel[] GetPopularProducts(int page = 0, int count = 20)
    {
        List<ProductModel> list = new();

        return list.ToArray();
    }
    public static ProductModel[] GetLatestProducts(int count = 0, int page = 20)
    {
        List<ProductModel> list = new();
        int[] ids = SQL.Products.GetLatestProducts(page, count);
        Parallel.ForEach(ids, id =>
        {
            if (TryGetByID(id, out ProductModel product))
            {
                if (product != null)
                    list.Add(product);
            }
        });
        return list.ToArray();
    }
    public static ProductModel[] GetProductsByTag(params Tag[] tags)
    {
        List<ProductModel> products = new();
        int[] tagIds = new int[tags.Length];
        for (int i = 0; i < tags.Length; i++)
        {
            tagIds[i] = tags[i].id;
        }
        int[] productIds = Products.GetAllProductsWithTags(tagIds);

        foreach (int id in productIds)
        {
            if (TryGetByID(id, out ProductModel model))
            {
                if (model != null)
                    products.Add(model);
            }
        }

        return products.ToArray();
    }

    public static ProductModel[] GetProductsFromPartial(int maxSize, string query, params int[] tags)
    {
        List<ProductModel> products = new();
        int[] ids = SQL.Products.GetProductsFromQuery(query, maxSize, tags);

        Parallel.ForEach(ids, id =>
        {
            if (TryGetByID(id, out ProductModel model))
            {
                if (model != null)
                    products.Add(model);
            }
        });

        return products.ToArray();
    }

    #endregion Public Methods

}
