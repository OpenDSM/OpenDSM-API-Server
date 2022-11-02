
using OpenDSM.Products.Models;
using OpenDSM.Products.SQL;

namespace OpenDSM.Products.Collections;

public static class ProductCollection
{

    public static ProductModel Get(int product_id)
    {
        ProductModel model = LoadedProductsCollection.Instance.Get(product_id);
        if (model.IsEmpty)
        {
            if (ProductDB.TryGetProduct(product_id, out model))
            {
                LoadedProductsCollection.Instance.Add(model);
            }
        }
        return model;
    }
    public static bool CheckSlugExists(string slug) => ProductDB.CheckIfSlugExists(slug);
}
