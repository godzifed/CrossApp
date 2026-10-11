using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IReadOnlyList<Product> Products() =>
    [
        Product.Create("P-001", "SKU-001", "Цемент М400 25кг", "шт", 100),
        Product.Create("P-002", "SKU-002", "Пісок будівельний", "т", 15),
        Product.Create("P-003", "SKU-003", "Цегла червона", "шт", 5000),
        Product.Create("P-004", "SKU-004", "Фарба біла 10л", "шт", 30),
        Product.Create("P-005", "SKU-005", "Шпаклівка 20кг", "міш", 45)
    ];
}