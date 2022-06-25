using OpenDSM.Lib.Auth;

namespace OpenDSM.Lib.Objects;

public class Products
{
    #region Fields

    public static Products Instance = Instance ??= new();
    public Dictionary<long, Product> _products;

    #endregion Fields

    #region Protected Constructors

    protected Products()
    {
        _products = new();
    }

    #endregion Protected Constructors

    #region Public Methods

    public void Create()
    {
    }

    public Product[] GetFromUser(User user)
    {
        Product[] products = new Product[user.OwnedProducts.Length];
        bool hadNull = false;
        for (int i = 0; i < products.Length; i++)
        {
            long productID = user.OwnedProducts[i];
            if (_products.ContainsKey(productID))
            {
                products[i] = _products[user.OwnedProducts[i]];
            }
            else
            {
                products[i] = null;
                hadNull = true;
            }
        }
        return hadNull ? products.Where(i => i != null).ToArray() : products;
    }

    public void Load()
    {
        string[] files = Directory.GetFiles(ProductsDirectory, "*.json", SearchOption.TopDirectoryOnly);
        Parallel.ForEach(files, file =>
        {
            FileInfo fileInfo = new(file);
            if (int.TryParse(fileInfo.Name.Replace(fileInfo.Extension, ""), out int id))
            {
                _products.Add(id, new(id));
            }
        });
    }

    #endregion Public Methods
}