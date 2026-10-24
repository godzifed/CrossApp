using Core.Abstractions;
using Core.Events;
using Core.Services;
using Core.Storage;
using System;
using System.IO;

Console.WriteLine("============================================================");
Console.WriteLine("                ЛАБОРАТОРНА РОБОТА №6                       ");
Console.WriteLine("============================================================\n");

string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");
ICatalogStore store = StoreFactory.Create(args, dataPath);
var service = new CatalogService(store);

// --- 1. Налаштування лог-файлу ---
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

// --- 2. Локальний метод-обробник (Перший підписник) ---
void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
{
    var line = $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} -> залишок {e.QuantityAfter}";
    Console.WriteLine(line);
    File.AppendAllText(logPath, line + Environment.NewLine);
}

// Підписуємо локальний метод
service.Changed += OnCatalogChanged;

// --- 3. Другий підписник: лямбда із замиканням (замикає змінну changes) ---
int changes = 0;
service.Changed += (sender, e) => changes++;

// ================= СЦЕНАРІЇ =================

Console.WriteLine("--- Сценарій успішних операцій ---");
var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
service.Receive(created.Id, 25);
Console.WriteLine($"Подій отримано: {changes}"); // Має вивести 2

// --- 4. Відписка (Демонстрація) ---
service.Changed -= OnCatalogChanged;
service.Receive(created.Id, 5); // Ця операція "тиха" в консолі, але...
Console.WriteLine($"Подій отримано: {changes}"); // ...лябмда продовжує рахувати (має вивести 3)

Console.WriteLine("\n--- Сценарії відмови (Подій не повинно бути) ---");
TryDo("Прихід для неіснуючого id", () => service.Receive("NON-EXISTENT-ID", 10));
TryDo("Від'ємний прихід", () => service.Receive(created.Id, -10));

// --- 5. Делегат Func (Пошук) ---
Console.WriteLine("\n--- Пошук товарів із залишком < 10 (Func<Product, bool>) ---");
var filtered = service.Search(p => p.Quantity < 10);
foreach (var p in filtered)
{
    Console.WriteLine($"  [Знайдено] {p.Name,-24} | Залишок: {p.Quantity} {p.Unit}");
}

return 0;

// Твоя крута допоміжна функція
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