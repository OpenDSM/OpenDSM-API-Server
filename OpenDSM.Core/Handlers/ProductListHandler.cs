using OpenDSM.Core.Models;
using OpenDSM.SQL;

namespace OpenDSM.Core.Handlers;

public enum ProductListType
{
    Latest,
    Popular,
    Tag
}

public class ProductListHandler
{

    #region Public Methods

    public static ProductModel[] GetProducts(int count = 20, int page = 0, ProductListType type = ProductListType.Latest)
    {
        switch (type)
        {
            case ProductListType.Latest:
                return GetLatestProducts(page, count);
            case ProductListType.Popular:
                return GetPopularProducts(page, count);
            case ProductListType.Tag:
                return GetPopularProducts(page, count);
            default:
                return GetLatestProducts(page, count);
        }
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
            if (ProductModel.TryGetByID(id, out ProductModel? product))
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
            if (ProductModel.TryGetByID(id, out ProductModel? model))
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
            if (ProductModel.TryGetByID(id, out ProductModel? model))
            {
                if (model != null)
                    products.Add(model);
            }
        });

        return products.ToArray();
    }

    #endregion Public Methods

}
