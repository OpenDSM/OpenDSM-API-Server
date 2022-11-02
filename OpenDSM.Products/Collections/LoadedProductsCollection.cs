
using OpenDSM.Products.Models;
using Timer = OpenDSM.Core.Handlers.TimerPlus;
namespace OpenDSM.Products.Collections;

internal class LoadedProductsCollection
{
    public readonly static LoadedProductsCollection Instance = Instance ??= new();
    private readonly Dictionary<int, ProductModel> loaded_items;
    private readonly Dictionary<int, Timer> loaded_timers;
    private readonly Dictionary<int, long> loaded_time;
    private readonly int kill_time = 5 * 1000;  /// unloads item after 5 seconds of inactivity

    private LoadedProductsCollection()
    {
        loaded_items = new();
        loaded_timers = new();
        loaded_time = new();
    }

    public void Add(ProductModel product)
    {
        loaded_timers.Add(product.ID, GetTimer(product.ID));
        loaded_items.Add(product.ID, product);
    }
    public ProductModel Get(int product_id)
    {
        if (loaded_items.ContainsKey(product_id))
        {
            loaded_timers[product_id].Push(kill_time);
            return loaded_items[product_id];
        }
        return ProductModel.Empty;
    }
    private Timer GetTimer(int product_id)
    {

        Timer timer = new(kill_time, false, false, (s, e) =>
        {
            if (loaded_items.ContainsKey(product_id))
            {
                loaded_items.Remove(product_id);
                if (loaded_timers.ContainsKey(product_id))
                {
                    loaded_timers[product_id].Dispose();
                    loaded_timers.Remove(product_id);
                }
                GC.Collect();
            }
        });
        return timer;
    }
    public object GetLoadedItemsObject()
    {
        object[] items = new object[loaded_items.Count];
        int index = 0;
        foreach (KeyValuePair<int, ProductModel> item in loaded_items)
        {
            Timer timer = loaded_timers[item.Key];
            items[index] = new
            {
                product_id = item.Key,

            };
            index++;
        }

        return new
        {
            count = items.Length,
            items,
        };
    }
}
