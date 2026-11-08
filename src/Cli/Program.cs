using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Core.Domain;
using Core.Reports;
using Core;

Console.WriteLine("============================================================");
Console.WriteLine("                ЛАБОРАТОРНА РОБОТА №7                       ");
Console.WriteLine("============================================================\n");

// Отримуємо розширений набір даних
var items = SampleData.GetProducts().ToList();

// --- 1. ДЕМОНСТРАЦІЯ ВІДКЛАДЕНОГО ВИКОНАННЯ ---
Console.WriteLine("--- Демо відкладеного виконання ---");
var query = items.Where(p => p.Quantity < 10); // Запит побудовано, але не виконано
Console.WriteLine($"Дефіцитних до змін: {query.Count()}");

// Змінюємо джерело даних
items.Add(Product.Create("tmp00001", "SKU-999", "Новий Дефіцит", "шт", 3));
Console.WriteLine($"Дефіцитних після змін: {query.Count()}"); // Число зміниться, бо Count() запускає запит наново

var snapshot = query.ToList(); // Фіксуємо результат у пам'яті
items.Clear();
Console.WriteLine($"У знімку лишилось: {snapshot.Count}\n"); // Не зміниться, бо читаємо властивість Count зі списку

items = SampleData.GetProducts().ToList();


// --- 2. ПІДКОМАНДИ LINQ-ЗВІТІВ ---
// (Якщо аргумент не передано, беремо "report-top")
var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-top";

Console.WriteLine($"--- Виконання команди: {command} ---");
switch (command)
{
    case "report-top":
        Print(ReportService.TopByQuantity(items, 5)); 
        break;
    case "report-low": 
        Print(ReportService.LowStock(items, 10)); 
        break;
    case "report-groups":
        foreach (var g in ReportService.ByUnit(items))
            Console.WriteLine($" {g.Key,-6} позицій: {g.Count,3} сумарно: {g.TotalQuantity,6}");
        break;
    case "report-sum":
        var t = ReportService.Totals(items);
        Console.WriteLine($" позицій {t.Items}, всього {t.Total}, середнє {t.Average:F2}, макс {t.Max}");
        break;
    default: 
        Console.WriteLine("Доступні: report-top | report-low | report-groups | report-sum"); 
        break;
}

// --- 3. ДОДАТКОВЕ ЗАВДАННЯ 4: Вимірювання часу (Stopwatch) ---
Console.WriteLine("\n--- Додаткове завдання 4 (Вимірювання Stopwatch) ---");

int recordCount = 100_000;
Console.WriteLine($"[1] Генеруємо {recordCount:N0} фейкових записів у пам'яті...");

var largeData = Enumerable.Range(1, recordCount)
    .Select(i => Product.Create(Guid.NewGuid().ToString("N")[..8], $"SKU-{i}", $"Назва {i}", "шт", i % 100))
    .ToList();

Console.WriteLine($"    Згенеровано успішно. Починаємо фільтрацію та сортування...\n");

var sw = Stopwatch.StartNew();
// Варіант 1: Без проміжного ToList() (виконується швидше, менше пам'яті)
var count1 = largeData.Where(p => p.Quantity > 50).OrderBy(p => p.Sku).Count();
sw.Stop();
Console.WriteLine($"[Варіант 1] БЕЗ ToList() посередині:");
Console.WriteLine($"    Знайдено {count1:N0} товарів. Час виконання: {sw.ElapsedMilliseconds} мс\n");

sw.Restart();
// Варіант 2: З проміжним ToList() (виділяє пам'ять під новий список, працює повільніше)
var count2 = largeData.Where(p => p.Quantity > 50).ToList().OrderBy(p => p.Sku).Count();
sw.Stop();
Console.WriteLine($"[Варіант 2] З проміжним ToList():");
Console.WriteLine($"    Знайдено {count2:N0} товарів. Час виконання: {sw.ElapsedMilliseconds} мс");


return 0;

static void Print(IReadOnlyList<StockLine> lines)
{
    foreach (var l in lines)
        Console.WriteLine($" {l.Sku,-10} {l.Name,-22} {l.Quantity,6} {l.Unit}");
}