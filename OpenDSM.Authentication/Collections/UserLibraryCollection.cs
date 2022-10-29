using OpenDSM.Authentication.Models;
using OpenDSM.Authentication.SQL;

namespace OpenDSM.Authentication.Collections;

public class UserLibraryCollection
{
    #region Public Properties

    public IReadOnlyCollection<UserLibraryItemModel> Items => (IReadOnlyCollection<UserLibraryItemModel>)_items;

    #endregion Public Properties

    #region Public Methods

    public bool AddItem(int productId, float price) => AddItem(productId, price, out _);

    public bool AddItem(int productId, float price, out UserLibraryItemModel item)
    {
        try
        {
            item = new()
            {
                ProductID = productId,
                Price = price,
                Purchased = DateTime.Now,
                UseTime = new TimeSpan(0)
            };
            _items.Add(item);
            return true;
        }
        catch
        {
        }
        item = new UserLibraryItemModel()
        {
            ProductID = -1,
            Price = -1,
        };
        return false;

    }

    #endregion Public Methods

    #region Internal Constructors

    internal UserLibraryCollection(int user_id)
    {
        _items = LibraryDB.GetLibraryItems(user_id);
    }

    #endregion Internal Constructors

    #region Private Fields

    private ICollection<UserLibraryItemModel> _items;

    #endregion Private Fields
}
