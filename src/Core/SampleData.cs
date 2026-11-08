using System.Collections.Generic;
using Core.Domain;
using System;

namespace Core;

public static class SampleData
{
    public static IEnumerable<Product> GetProducts()
    {
        return new List<Product>
        {
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-001", "Кабель UTP", "м", 500),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-002", "Розетка", "шт", 50),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-003", "Вимикач", "шт", 15),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-004", "Лампа LED", "шт", 8), // Дефіцит
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-005", "Ізоляційна стрічка", "шт", 120),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-006", "Кабель живлення", "м", 300),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-007", "Коробка монтажна", "шт", 5), // Дефіцит
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-008", "Шуруп", "кг", 20),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-009", "Цвях", "кг", 15),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-010", "Дюбель", "шт", 1000),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-011", "Свердло 6мм", "шт", 2), // Дефіцит
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-012", "Свердло 8мм", "шт", 10),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-013", "Молоток", "шт", 5), // Дефіцит
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-014", "Рулетка 5м", "шт", 12),
            Product.Create(Guid.NewGuid().ToString("N")[..8], "SKU-015", "Кабель FTP", "м", 150)
        };
    }
}