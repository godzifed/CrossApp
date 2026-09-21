using Core.Dto;
using Core.Import;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

string extension = Path.GetExtension(path).ToLowerInvariant();

// Switch expression для вибору імпортера за розширенням файлу
ImportResult<object> result = extension switch
{
    ".csv" => ProductCsvImporter.Load(path),
    ".json" => ConvertJsonResult(ProductJsonImporter.Load(path)),
    _ => new ImportResult<object>([], [$"Непідтримуване розширення файлу: '{extension}'"])
};

// Вивід перших записів
Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (object item in result.Items.Take(5))
{
    string line = item switch
    {
        ProductDto p => $"[Товар] {p.Id,-6} {p.Sku,-10} {p.Name,-26} {p.Quantity,5} {p.Unit}",
        WarehouseDto w => $"[Склад] {w.Id,-6} {w.Name,-37} {w.Location}",
        _ => item.ToString() ?? string.Empty
    };
    Console.WriteLine($"  {line}");
}

// Вивід списку помилок
if (result.Errors.Count > 0)
{
    Console.WriteLine($"\nПропущено записів: {result.Errors.Count}");
    foreach (string e in result.Errors)
    {
        Console.WriteLine($"  ! {e}");
    }
}

// Додаткове завдання 3: Статистика імпорту одним рядком
int accepted = result.Items.Count;
int skipped = result.Errors.Count;
int total = accepted + skipped;
double errorPercent = total > 0 ? ((double)skipped / total) * 100 : 0;

Console.WriteLine("\n------------------------------------------------------------");
Console.WriteLine($"Статистика: Усього: {total} | Прийнято: {accepted} | Пропущено: {skipped} | Помилок: {errorPercent:F1}%");
Console.WriteLine("------------------------------------------------------------");

return 0;

static ImportResult<object> ConvertJsonResult(ImportResult<ProductDto> jsonRes) =>
    new(jsonRes.Items.Cast<object>().ToList(), jsonRes.Errors);