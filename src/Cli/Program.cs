using Core.Abstractions;
using Core.Services;
using Core.Storage;

Console.WriteLine("============================================================");
Console.WriteLine("                ЛАБОРАТОРНА РОБОТА №5                       ");
Console.WriteLine("============================================================\n");

// 1. Створення сховища через фабрику (Додаткове завдання 3)
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");
ICatalogStore store = StoreFactory.Create(args, dataPath);

var service = new CatalogService(store);

Console.WriteLine($"Обране сховище: {store.GetType().Name}\n");

// 2. Демонстрація списку
Console.WriteLine("--- Поточний список товарів ---");
foreach (var p in service.All())
{
    Console.WriteLine($"  {p.Id,-8} {p.Sku,-10} {p.Name,-24} {p.Quantity,5} {p.Unit}");
}

// 3. Демонстрація пошуку за предикатом Func<Product, bool> (Додаткове завдання 2)
Console.WriteLine("\n--- Пошук товарів із залишком > 40 (Func<Product, bool>) ---");
var filtered = service.Search(p => p.Quantity > 40);
foreach (var p in filtered)
{
    Console.WriteLine($"  [Знайдено] {p.Name,-24} | Залишок: {p.Quantity} {p.Unit}");
}

// 4. Сценарії відмови
Console.WriteLine("\n--- Сценарії відмови ---");
TryDo("Прихід для неіснуючого id", () => service.Receive("NON-EXISTENT-ID", 10));

return 0;

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  [!] {title}: виняток НЕ спрацював!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  [OK] {title}: {ex.GetType().Name} — {ex.Message}");
    }
}