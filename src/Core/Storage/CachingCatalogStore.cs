using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingCatalogStore(ICatalogStore innerStore) : ICatalogStore
{
    private readonly ICatalogStore _innerStore = innerStore ?? throw new ArgumentNullException(nameof(innerStore));
    private IReadOnlyList<Product>? _cachedList;

    public IReadOnlyList<Product> List()
    {
        _cachedList ??= _innerStore.List();
        return _cachedList;
    }

    public Product? GetById(string id)
    {
        if (_cachedList != null)
        {
            var cachedItem = _cachedList.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (cachedItem != null) return cachedItem;
        }

        return _innerStore.GetById(id);
    }

    // НОВЕ: Реалізація пошуку за предикатом
    public IReadOnlyList<Product> Find(Func<Product, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return List().Where(predicate).ToList();
    }

    public void Add(Product item)
    {
        _innerStore.Add(item);
        InvalidateCache();
    }

    public void Update(Product item)
    {
        _innerStore.Update(item);
        InvalidateCache();
    }

    public bool Remove(string id)
    {
        var removed = _innerStore.Remove(id);
        if (removed) InvalidateCache();
        return removed;
    }

    private void InvalidateCache() => _cachedList = null;
}