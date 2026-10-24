using Core.Abstractions;
using Core.Domain;
using Core.Events; // ДОДАНО
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Services;

public sealed class CatalogService(ICatalogStore store)
{
    private readonly ICatalogStore _store = store ?? throw new ArgumentNullException(nameof(store));

    // --- НОВЕ: Оголошення події ---
    public event EventHandler<CatalogChangedEventArgs>? Changed;

    // --- НОВЕ: Метод підняття події ---
    private void OnChanged(Product p, CatalogChangeKind kind, int delta) =>
        Changed?.Invoke(this, new CatalogChangedEventArgs(p.Id, kind, delta, p.Quantity));

    // Твій метод Add (можливо, він у тебе трохи інакший, головне — додати OnChanged)
    public Product Add(string sku, string name, string unit, int quantity)
    {
        var product = Product.Create(Guid.NewGuid().ToString("N")[..8], sku, name, unit, quantity);
        _store.Add(product);
        
        // Викликаємо подію ПІСЛЯ успішної зміни
        OnChanged(product, CatalogChangeKind.Added, quantity); 
        return product;
    }

    // Твій метод Receive
    public void Receive(string id, int quantity)
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");
            
        product.RegisterArrival(quantity);
        _store.Update(product);
        
        // Якщо верхній рядок кине виняток - події не буде (так і треба)
        OnChanged(product, CatalogChangeKind.Received, quantity);
    }
    
    // Якщо маєш метод Issue, додай туди OnChanged(product, CatalogChangeKind.Issued, -quantity);

    // --- НОВЕ (або оновлене): Метод пошуку з Func ---
    public IReadOnlyList<Product> Search(Func<Product, bool> predicate) =>
        _store.List().Where(predicate).ToList();
        
    public IReadOnlyList<Product> All() => _store.List();
}