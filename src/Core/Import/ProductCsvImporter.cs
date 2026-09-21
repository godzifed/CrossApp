using System.Globalization;
using System.Text;
using Core.Dto;

namespace Core.Import;

public static class ProductCsvImporter
{
    private const char Separator = ';';

    public static ImportResult<object> Load(string path)
    {
        var items = new List<object>();
        var errors = new List<string>();

        // Явно задаємо UTF-8 відповідно до вимог щодо кирилиці
        string[] lines = File.ReadAllLines(path, Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            // Фільтрація порожніх рядків та коментарів
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            // Пропуск рядка заголовка
            if (number == 1 && (line.StartsWith("type", StringComparison.OrdinalIgnoreCase) || line.StartsWith("id", StringComparison.OrdinalIgnoreCase)))
                continue;

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<object>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            // Помилки кількості колонок
            { Length: < 4 } => new ParseFailed($"замало колонок: {parts.Length}"),

            // 1. Товар (P): перевірка на порожні назву чи SKU через константні патерни
            ["P" or "p", _, "", _, _, ..] or ["P" or "p", _, _, "", _, ..] 
                => new ParseFailed("SKU або назва товару порожні"),

            // 2. Товар (P): охоронна умова when для безпечного парсингу кількості
            ["P" or "p", _, _, _, _, var qty] when !int.TryParse(qty, NumberStyles.Integer, CultureInfo.InvariantCulture, out int q) || q < 0
                => new ParseFailed($"кількість '{qty}' не є невід'ємним числом"),

            // 3. Товар (P): успішний збіг рівно 6 колонок
            ["P" or "p", var id, var sku, var name, var unit, var qty]
                => new ParseOk(new ProductDto(id, sku, name, unit, int.Parse(qty, CultureInfo.InvariantCulture))),

            // 4. Склад (W): перевірка на порожні значення
            ["W" or "w", _, "", _] or ["W" or "w", _, _, ""]
                => new ParseFailed("назва або адреса складу порожні"),

            // 5. Склад (W): успішний збіг рівно 4 колонок
            ["W" or "w", var id, var name, var loc]
                => new ParseOk(new WarehouseDto(id, name, loc)),

            // Помилки форми для конкретних типів
            ["P" or "p", ..] => new ParseFailed($"для товару (P) очікую 6 колонок, отримано {parts.Length}"),
            ["W" or "w", ..] => new ParseFailed($"для складу (W) очікую 4 колонки, отримано {parts.Length}"),

            // Невідомий префікс
            [var prefix, ..] => new ParseFailed($"невідомий префікс запису '{prefix}'"),

            _ => new ParseFailed("невідомий формат рядка")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseOk(object Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}