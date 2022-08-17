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
        ProductModel[] products = new ProductModel[count];

        return products;
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

        foreach(int id in productIds)
        {
            if(ProductModel.TryGetByID(id, out ProductModel model))
            {
                products.Add(model);
            }
        }

        return products.ToArray();
    }

    #endregion Public Methods
}
