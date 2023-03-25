using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SOTFEdit.Model;

public class ItemList : IEnumerable<KeyValuePair<int, Item>>
{
    private readonly Dictionary<int, Item> _items;

    public ItemList(IEnumerable<Item> items)
    {
        _items = items.ToDictionary(item => item.Id, item => item);
    }

    public IEnumerator<KeyValuePair<int, Item>> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Item? GetItem(int id)
    {
        return _items.GetValueOrDefault(id);
    }
}