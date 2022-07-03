using OpenDSM.Lib.Auth;

namespace OpenDSM.Lib.Objects;

public class Products
{
    #region Fields

    public static Products Instance = Instance ??= new();
    private Dictionary<uint, Product> _products;

    #endregion Fields

    #region Protected Constructors

    protected Products()
    {
        _products = new();
    }

    #endregion Protected Constructors

    #region Public Methods

    public Product Create(User owner, string name, string description, string repo_url, bool isExperimental, bool showGithub, string[] keywords, string[] tags)
    {
        uint id = (uint)(_products.Count() + 1);
        Product product = new(id, owner, name, description, repo_url, isExperimental, showGithub, keywords, tags);
        _products.Add(id, product);
        return product;
    }

    public Product GetFeaturedProduct(User? user, bool force = false)
    {
        Product featured = _products[0];
        if (user == null)
        {
            // No Algorithm
        }
        else
        {
            // Attempt Algorithm
            if (force)
            {
            }
        }
        return featured;
    }

    public Product[] GetFromUser(User user)
    {
        try
        {
            if (user.OwnedProducts == null || !user.OwnedProducts.Any())
            {
                return Array.Empty<Product>();
            }
            Product[] products = new Product[user.OwnedProducts.Length];
            bool hadNull = false;
            for (uint i = 0; i < products.Length; i++)
            {
                uint productID = user.OwnedProducts[i];
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
        catch
        {
            return Array.Empty<Product>();
        }
    }

    public Product GetProduct(uint id)
    {
        return _products[id];
    }

    public void Load()
    {
        string[] files = Directory.GetFiles(ProductsDirectory, "*.json", SearchOption.TopDirectoryOnly);
        Parallel.ForEach(files, file =>
        {
            FileInfo fileInfo = new(file);
            if (uint.TryParse(fileInfo.Name.Replace(fileInfo.Extension, ""), out uint id))
            {
                _products.Add(id, new(id));
            }
        });
    }

    public Product[] Search(string query, params string[] tags)
    {
        Dictionary<int, Product> products = new();
        string[] keywords = query.Split(' ');
        foreach ((uint _, Product product) in _products)
        {
            int matches = 0;
            bool hasMatchingTags = true;
            foreach (string keyword in keywords)
            {
                if (product.Keywords.Contains(keyword))
                {
                    matches++;
                }
            }
            if (matches != 0)
            {
                foreach (string tag in tags)
                {
                    if (!product.Tags.Contains(tag))
                    {
                        hasMatchingTags = false;
                        break;
                    }
                }
                if (hasMatchingTags)
                {
                    products.Add(matches, product);
                }
            }
        }
        return ((Dictionary<uint, Product>)products.OrderBy(i => i.Key)).Values.ToArray();
    }

    #endregion Public Methods
}