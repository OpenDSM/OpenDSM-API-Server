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

    /// <summary>
    /// Attempts to create a product
    /// </summary>
    /// <param name="gitRepoName">The repository name</param>
    /// <param name="shortSummery">A short summery of the product</param>
    /// <param name="user">The author of the product</param>
    /// <param name="name">The name of the product</param>
    /// <param name="yt_key">The video trailer key</param>
    /// <param name="subscription">If the product is a subscription or not</param>
    /// <param name="use_git_readme">If the product page should use the github readme or not</param>
    /// <param name="price">The price of the product</param>
    /// <param name="keywords">Any keywords that the product might use in searching or categorizing</param>
    /// <param name="tags">Any tags that the product might use in searching or categorizing</param>
    /// <param name="model">The created product</param>
    /// <returns>If the task was successfull</returns>
    public static bool TryCreateProduct(string gitRepoName, string shortSummery, UserModel user, string name, string yt_key, bool subscription, bool use_git_readme, int price, string[] keywords, int[] tags, out ProductModel? model)
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

    /// <summary>
    /// Attempts to find product based on product id
    /// </summary>
    /// <param name="id">The product id</param>
    /// <param name="product">The product, if found, null if not</param>
    /// <returns>If the product was found or not</returns>
    public static bool TryGetByID(int id, out ProductModel product)
    {
        return (product = GetByID(id)) != null;
    }

    /// <summary>
    /// Gets the product based on the id or returns null
    /// </summary>
    /// <param name="id">The id of the product</param>
    /// <returns>The product, if found, null if not</returns>
    public static ProductModel? GetByID(int id)
    {
        try
        {
            if (Products.TryGetProductFromID(id, out string name, out string gitRepoName, out string summery, out bool useGitReadme, out bool subscription, out int[] tags, out string[] keywords, out int price, out string yt_key, out int owner_id, out int pageViews, out DateTime posted))
            {
                return new ProductModel(id, owner_id, gitRepoName, name, summery, useGitReadme, yt_key, (uint)price, tags, keywords, subscription, pageViews, posted);
            }
        }
        catch { }
        return null;
    }

    /// <summary>
    /// Gets a list of products matching the filter parameters.
    /// </summary>
    /// <param name="count">The max number of products to return</param>
    /// <param name="page">The page offset of the list</param>
    /// <param name="type">The sorting order of the list</param>
    /// <returns></returns>
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


    /// <summary>
    /// Gets a list of popular products.
    /// </summary>
    /// <param name="page">The page offset of the list</param>
    /// <param name="count">The max number of products to return</param>
    /// <returns></returns>
    public static ProductModel[] GetPopularProducts(int page = 0, int count = 20)
    {
        List<ProductModel> list = new();

        return list.ToArray();
    }

    /// <summary>
    /// Gets a list of the latest products
    /// </summary>
    /// <param name="count">The max number of products to return</param>
    /// <param name="page">The page offset of the list</param>
    /// <returns></returns>
    public static ProductModel[] GetLatestProducts(int count = 0, int page = 20)
    {
        List<ProductModel> list = new();
        int[] ids = Products.GetLatestProducts(page, count);
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

    /// <summary>
    /// Gets a list of products based on tags provided
    /// </summary>
    /// <param name="tags">Tags to filter by</param>
    /// <param name="count">The max number of products to return</param>
    /// <param name="page">The page offset of the list</param>
    /// <returns>A filtered list of products based on tags</returns>
    public static ProductModel[] GetProductsByTag(int count, int page, params int[] tags)
    {
        List<ProductModel> products = new();
        int[] tagIds = new int[tags.Length];
        for (int i = 0; i < tags.Length; i++)
        {
            tagIds[i] = tags[i];
        }

        int[] productIds = Products.GetAllProductsWithTags(count, page, tagIds);
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

    /// <summary>
    /// Gets a list of products based on query
    /// </summary>
    /// <param name="count">The number of results to return</param>
    /// <param name="page">The page offset of the list</param>
    /// <param name="query">The search query</param>
    /// <param name="tags">Tags to filter the list by</param>
    /// <returns></returns>
    public static ProductModel[] GetProductsFromPartial(int count, int page, string query, params int[] tags)
    {
        List<ProductModel> products = new();
        int[] ids = Products.GetProductsFromQuery(query, count, page, tags);

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
