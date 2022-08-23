using OpenDSM.Core.Models;
using OpenDSM.SQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDSM.Core.Handlers;

public class ProductListHandler
{

    #region Public Methods

    public static ProductModel[] GetPopularProducts(int count = 20)
    {
        List<ProductModel> list = new ();

        return list.ToArray() ;
    }
    public static ProductModel[] GetLatestProducts(int count = 20)
    {
        List<ProductModel> list = new ();
        int[] ids = SQL.Products.GetLatestProducts(count);
        Parallel.ForEach(ids, id =>
        {
            if(ProductModel.TryGetByID(id, out ProductModel? porduct))
            {
                list.Add(porduct);
            }
        });
        return list.ToArray() ;
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
            if (ProductModel.TryGetByID(id, out ProductModel model))
            {
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
                products.Add(model);
        });

        return products.ToArray();
    }

    #endregion Public Methods

}
