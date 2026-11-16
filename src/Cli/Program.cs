using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Core.Import;
using Core.Services;
using Core.Storage;

Console.WriteLine("============================================================");
Console.WriteLine("                ЛАБОРАТОРНА РОБОТА №8                       ");
Console.WriteLine("============================================================\n");

// 1. Ініціалізація сховища та сервісу
var store = new InMemoryCatalogStore(); 
var service = new CatalogService(store, Array.Empty<Core.Abstractions.INotificationSink>());

// 2. Налаштування токенів скасування
using var userCts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => 
{ 
    e.Cancel = true; 
    Console.WriteLine("\nОтримано сигнал скасування (Ctrl+C)...");
    userCts.Cancel(); 
};

// Токен для таймауту (скасує операцію автоматично через 5 секунд)
using var timeoutCts = new CancellationTokenSource();
timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

// Додаткове завдання 2: Зв'язані токени скасування (об'єднуємо Ctrl+C та таймаут)
using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

// 3. Шлях до файлу імпорту
// Вказуємо шлях до JSON файлу. Зверніть увагу, папка data має бути поруч із виконуваним файлом
string importPath = Path.Combine(Environment.CurrentDirectory, "Data", "sample.json");

// Додаткове завдання 3: Обробник прогресу
var progress = new Progress<int>(count => 
{
    Console.Write($"\rІмпортовано записів: {count}...");
});

// 4. Виконання асинхронного імпорту з обробкою винятків
try
{
    var added = await service.ImportAsync(new JsonCatalogImporter(), importPath, progress, linkedCts.Token);
    Console.WriteLine($"\nУспішно завершено. Всього імпортовано: {added}");
}
catch (OperationCanceledException) 
{ 
    // Обробка скасування має йти ПЕРЕД Exception
    Console.WriteLine("\nІмпорт скасовано (через Ctrl+C або таймаут)."); 
}
catch (DomainException ex) 
{ 
    // Бізнес-помилки
    Console.WriteLine($"\nДані відхилено: {ex.Message}"); 
}
catch (FileNotFoundException) 
{ 
    // Якщо файл не знайдено
    Console.WriteLine($"\nФайл не знайдено: {importPath}"); 
}
catch (Exception ex)
{
    // Будь-які інші неочікувані помилки
    Console.WriteLine($"\nНеочікувана помилка: {ex.Message}");
}

// Завершення роботи програми
return 0;