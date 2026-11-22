using System.Reflection;
using Core;
using Core.Configuration;
using Core.Domain;
using Core.Storage;
using Microsoft.Extensions.Configuration;

var envName = Environment.GetEnvironmentVariable("CROSSAPP_ENVIRONMENT") ?? "Development";

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{envName}.json", optional: true)
    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
    .AddEnvironmentVariables(prefix: "CROSSAPP_")
    .AddCommandLine(args)
    .Build();

var options = config.Get<AppOptions>() ?? new AppOptions();
var legacyPath = Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");

var storageOptions = new StorageOptions
{
    DataPath = string.IsNullOrWhiteSpace(legacyPath) ? options.Storage.DataPath : legacyPath,
    FileName = options.Storage.FileName
};

var dataFilePath = storageOptions.ResolveFullPath(AppContext.BaseDirectory);
var apiKey = config["Integration:ApiKey"];

Console.WriteLine($"Середовище: {options.EnvironmentName} (Log: {options.Logging.MinimumLevel})");
Console.WriteLine($"Файл даних: {dataFilePath}");
Console.WriteLine($"Одиниця виміру за замовчуванням: {options.Warehouse.DefaultUnit}");
Console.WriteLine(apiKey is null
    ? "ApiKey: не заданий"
    : $"ApiKey: присутній, довжина {apiKey.Length}");

    var store = new FileCatalogStore(dataFilePath);
var command = args.FirstOrDefault(a => !a.StartsWith("--"))?.ToLowerInvariant() ?? "export";

try
{
    switch (command)
    {
        case "export":
            var defaultUnit = options.Warehouse.DefaultUnit;
            var itemsToExport = new[]
            {
                Product.Create("a1b2c3d4", "A-100", "Цемент М400", "мішок", 40),
                Product.Create("b2c3d4e5", "B-200", "Гвинт М6", defaultUnit, 150),
                Product.Create("c3d4e5f6", "C-300", "Кабель ВВГ 3x2.5", "м", 85)
            };

            foreach (var item in itemsToExport)
            {
                store.Update(item); // записує або оновлює товар і атомарно зберігає JSON
            }

            Console.WriteLine($"Експортовано {itemsToExport.Length} записи.");
            break;

        case "import":
            var loaded = store.List();
            Console.WriteLine($"Завантажено {loaded.Count} записів із JSON:");
            foreach (var item in loaded)
            {
                Console.WriteLine($" - [{item.Sku}] {item.Name}: {item.Quantity} {item.Unit}");
            }
            break;

        default:
            Console.WriteLine($"Невідома команда '{command}'. Доступні команди: export, import");
            break;
    }
}
catch (DomainException ex)
{
    Console.WriteLine($"Помилка даних: {ex.Message}");
}
catch (IOException ex)
{
    Console.WriteLine($"Помилка файлової системи: {ex.Message}");
}