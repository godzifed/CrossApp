using System.Collections.Generic;
using Core.Domain;

namespace Core.Tests;

internal static class TestData
{
    public static IEnumerable<Product> Items() => new[]
    {
        Product.Create("1", "SKU-MID", "Товар 1", "шт", 10),
        Product.Create("2", "SKU-BIG", "Товар 2", "шт", 50),
        Product.Create("3", "SKU-LOW", "Товар 3", "шт", 1)
    };
}