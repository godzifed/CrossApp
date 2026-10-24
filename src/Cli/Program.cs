using Cli;
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
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");

// 1. Точка збірки: створюємо сховище
ICatalogStore store = StoreFactory.Create(args, dataPath);

// --- ДОДАТКОВЕ ЗАВДАННЯ: Створюємо Sinks для ручного DI ---
var sinks = new INotificationSink[] 
{
    new ConsoleSink(),
    new FileSink(logPath)
};

// 2. Передаємо і сховище, і "сінки" у сервіс
var service = new CatalogService(store, sinks);

// --- БАЗОВЕ ЗАВДАННЯ: Підписка через події (Event) ---
void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
{
    var line = $"[Event] {e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} -> залишок {e.QuantityAfter}";
    Console.WriteLine(line);
}
service.Changed += OnCatalogChanged;

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