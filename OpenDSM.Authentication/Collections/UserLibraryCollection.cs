using OpenDSM.Authentication.Models;
using OpenDSM.Authentication.SQL;

namespace OpenDSM.Authentication.Collections;

public class UserLibraryCollection
{
    private ICollection<UserLibraryItemModel> _items;
    internal UserLibraryCollection(int user_id)
    {
        _items = LibraryDB.GetLibraryItems(user_id);
    }

    public IReadOnlyCollection<UserLibraryItemModel> Items => (IReadOnlyCollection<UserLibraryItemModel>)_items;
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
}
