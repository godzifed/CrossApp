using System.Collections.Concurrent;
using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryCatalogStore : ICatalogStore
{
    private readonly ConcurrentDictionary<string, Product> _items = new(StringComparer.OrdinalIgnoreCase);

    public InMemoryCatalogStore(IEnumerable<Product>? initial = null)
    {
        if (initial is null) return;
        foreach (var item in initial)
        {
            _items[item.Id] = item;
        }
    }

    public IReadOnlyList<Product> List() => _items.Values.ToList();

    public Product? GetById(string id) => _items.GetValueOrDefault(id);

    public IReadOnlyList<Product> Find(Func<Product, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _items.Values.Where(predicate).ToList();
    }

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (!_items.TryAdd(item.Id, item))
            throw new InvalidOperationException($"Запис з id={item.Id} уже існує.");
    }

    public void Update(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items[item.Id] = item;
    }

    public bool Remove(string id) => _items.TryRemove(id, out _);
}