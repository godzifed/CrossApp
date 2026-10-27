using Core.Domain;

namespace Core.Abstractions;

public interface ICatalogStore
{
    IReadOnlyList<Product> List();
    IReadOnlyList<Product> Find(Func<Product, bool> predicate); 
    Product? GetById(string id);
    void Add(Product item);
    void Update(Product item);
    bool Remove(string id);
}