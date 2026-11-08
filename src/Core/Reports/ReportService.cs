using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain;

namespace Core.Reports;

public static class ReportService
{
    // 1) Фільтр + сортування + проєкція
    public static IReadOnlyList<StockLine> LowStock(IEnumerable<Product> source, int threshold) =>
        source.Where(p => p.Quantity < threshold)
              .OrderBy(p => p.Quantity)
              .ThenBy(p => p.Sku, StringComparer.Ordinal)
              .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
              .ToList();

    // 2) Топ-N зі стабільним порядком (Тут використовується додаткове завдання 2: метод TopBy)
    public static IReadOnlyList<StockLine> TopByQuantity(IEnumerable<Product> source, int n) =>
        source.TopBy(p => p.Quantity, n)
              .OrderByDescending(p => p.Quantity)
              .ThenBy(p => p.Sku, StringComparer.Ordinal) // стабільний порядок
              .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
              .ToList();

    // 3) Групування з підрахунком і сумою (СИНТАКСИС ЗАПИТІВ - Додаткове завдання 3)
    public static IReadOnlyList<GroupSummary> ByUnit(IEnumerable<Product> source)
    {
        // Використання query syntax (from ... group ... into ... select)
        var query = from p in source
                    group p by p.Unit.ToLowerInvariant() into g
                    orderby g.Sum(x => x.Quantity) descending
                    select new GroupSummary(g.Key, g.Count(), g.Sum(x => x.Quantity));

        return query.ToList();
    }

    // 4) Агрегати з обробкою порожньої послідовності
    public static (int Items, int Total, double Average, int Max) Totals(IEnumerable<Product> source)
    {
        var list = source.ToList();
        return list.Count == 0
            ? (0, 0, 0d, 0)
            : (list.Count, list.Sum(p => p.Quantity), list.Average(p => p.Quantity), list.Max(p => p.Quantity));
    }

    // 5) ДОДАТКОВЕ ЗАВДАННЯ 1: Join двох колекцій
    // З'єднуємо товари з їхніми "рухами" (фейковими для демо)
    public static IReadOnlyList<MovementLine> GetMovements(IEnumerable<Product> products, IEnumerable<StockMovement> movements)
    {
        return products.Join(movements,
            product => product.Id,
            movement => movement.ProductId, // Вважаємо, що StockMovement має поле ProductId
            (product, movement) => new MovementLine(product.Sku, movement.Delta))
            .ToList();
    }
}