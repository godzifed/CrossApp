<<<<<<< Updated upstream
﻿using Cli;
using Core.Abstractions;
using Core.Events;
=======
﻿using Core.Abstractions;
using Core.Domain;
>>>>>>> Stashed changes
using Core.Services;
using Core.Storage;
using System;
using System.IO;
using System.Linq;


Console.WriteLine("============================================================");
Console.WriteLine("                ЛАБОРАТОРНА РОБОТА №6                       ");
Console.WriteLine("============================================================\n");

<<<<<<< Updated upstream
=======
// 1. Точка збірки та DI
>>>>>>> Stashed changes
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");

// 1. Точка збірки: створюємо сховище
ICatalogStore store = StoreFactory.Create(args, dataPath);
<<<<<<< Updated upstream

// --- ДОДАТКОВЕ ЗАВДАННЯ: Створюємо Sinks для ручного DI ---
var sinks = new INotificationSink[] 
=======
var service = new CatalogService(store);

Console.WriteLine($"Обране сховище: {store.GetType().Name}\n");

// 2. Демонстрація списку ДО зміни
Console.WriteLine("--- Поточний список товарів (ДО ЗМІНИ) ---");
foreach (var p in service.All())
>>>>>>> Stashed changes
{
    new ConsoleSink(),
    new FileSink(logPath)
};

<<<<<<< Updated upstream
// 2. Передаємо і сховище, і "сінки" у сервіс
var service = new CatalogService(store, sinks);

// --- БАЗОВЕ ЗАВДАННЯ: Підписка через події (Event) ---
void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
=======
// 3. Змінюємо кількість товару
var targetProduct = service.All().FirstOrDefault();
if (targetProduct != null)
{
    Console.WriteLine($"\n[!] Виконуємо Receive: додаємо 10 шт до товару {targetProduct.Sku}...\n");
    service.Receive(targetProduct.Id, 10);
}

// 4. Демонстрація списку ПІСЛЯ зміни
Console.WriteLine("--- Поточний список товарів (ПІСЛЯ ЗМІНИ) ---");
foreach (var p in service.All())
{
    // Тепер ви на власні очі побачите, що кількість зросла на 10
    Console.WriteLine($"  {p.Id,-8} {p.Sku,-10} {p.Name,-24} {p.Quantity,5} {p.Unit}");
}

// 5. Пошук за предикатом (Додаткове завдання 2)
Console.WriteLine("\n--- Пошук товарів із залишком > 40 (Func<Product, bool>) ---");
var filtered = service.Search(p => p.Quantity > 40);
foreach (var p in filtered)
>>>>>>> Stashed changes
{
    var line = $"[Event] {e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} -> залишок {e.QuantityAfter}";
    Console.WriteLine(line);
}
service.Changed += OnCatalogChanged;

<<<<<<< Updated upstream
int changes = 0;
service.Changed += (sender, e) => changes++;


// ================= СЦЕНАРІЇ =================

Console.WriteLine("--- Сценарій успішних операцій ---");
var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
service.Receive(created.Id, 25);

Console.WriteLine($"\nПодій отримано через Event (лямбда): {changes}");

// Демонструємо, що при відписці Event мовчить, але Sinks продовжують працювати
Console.WriteLine("\n--- Демонстрація відписки Event ---");
service.Changed -= OnCatalogChanged;
service.Receive(created.Id, 5); 

Console.WriteLine($"\nПодій отримано через Event (лямбда): {changes}");

Console.WriteLine("\n--- Сценарії відмови (Подій не повинно бути) ---");
=======
// 6. Сценарії відмови
Console.WriteLine("\n--- Сценарії відмови ---");
>>>>>>> Stashed changes
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