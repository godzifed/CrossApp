using Core.Abstractions;
using Core.Domain;
using Core.Events;


namespace Core.Services;

public sealed class CatalogService
{
    private readonly ICatalogStore _store;
    private readonly IEnumerable<INotificationSink> _sinks; // Для додаткового завдання

    // Конструктор приймає сховище (Лаб 5) та опційну колекцію приймачів (Лаб 6 - Додаткове)
    public CatalogService(ICatalogStore store, IEnumerable<INotificationSink>? sinks = null)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _sinks = sinks ?? Array.Empty<INotificationSink>();
    }

    // --- ОСНОВНЕ ЗАВДАННЯ: Оголошення події ---
    public event EventHandler<CatalogChangedEventArgs>? Changed;

    // --- ЦЕНТРАЛІЗОВАНИЙ МЕТОД СПОВІЩЕННЯ ---
    private void OnChanged(Product p, CatalogChangeKind kind, int delta)
    {
        var args = new CatalogChangedEventArgs(p.Id, kind, delta, p.Quantity);

        // 1. Виклик через подію (Observer) - базове завдання
        Changed?.Invoke(this, args);

        // 2. Виклик через приймачі (Ручний DI) - додаткове завдання
        foreach (var sink in _sinks)
        {
            sink.Notify(args);
        }
    }

    // --- БІЗНЕС-ОПЕРАЦІЇ ---

    public Product Add(string sku, string name, string unit, int quantity)
    {
        // Створюємо товар (домен сам перевірить інваріанти на null чи від'ємний залишок)
        string newId = Guid.NewGuid().ToString("N")[..8];
        var product = Product.Create(newId, sku, name, unit, quantity);
        
        _store.Add(product);
        
        // Сповіщаємо ПІСЛЯ успішної зміни
        OnChanged(product, CatalogChangeKind.Added, quantity);
        
        return product;
    }

    public void Receive(string id, int quantity)
    {
        var product = _store.GetById(id) 
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");

        // Доменний метод (якщо quantity < 0, кине виняток і код нижче не виконається)
        product.RegisterArrival(quantity);
        _store.Update(product);

        // Сповіщаємо ПІСЛЯ успішної зміни
        OnChanged(product, CatalogChangeKind.Received, quantity);
    }

    public void Issue(string id, int quantity)
    {
        var product = _store.GetById(id) 
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");

        // Доменний метод (якщо quantity > залишок, кине виняток перевитрати)
        product.Issue(quantity);
        _store.Update(product);

        // Зверни увагу: дельта передається з мінусом, бо ми забираємо товар
        OnChanged(product, CatalogChangeKind.Issued, -quantity);
    }

    // --- ОПЕРАЦІЇ ЧИТАННЯ (без подій, бо стан не змінюється) ---

    // Використання делегата Func для динамічного пошуку
    public IReadOnlyList<Product> Search(Func<Product, bool> predicate)
    {
        return _store.List().Where(predicate).ToList();
    }

    public IReadOnlyList<Product> All()
    {
        return _store.List();
    }
}