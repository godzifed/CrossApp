using System.Linq;
using Core.Reports;
using Xunit;

namespace Core.Tests;

public class ReportServiceTests
{
    // 3) Звіт на кількох наборах (Theory)
    [Theory]
    [InlineData(10, 1)] // Повинен знайти SKU-LOW (1 < 10)
    [InlineData(1, 0)]  // Нікого не знайде (ніхто не < 1)
    [InlineData(100, 3)] // Знайде всіх
    public void LowStock_CountsOnlyBelowThreshold(int threshold, int expected)
    {
        // Act
        var result = ReportService.LowStock(TestData.Items(), threshold);
        
        // Assert
        Assert.Equal(expected, result.Count);
    }

    // 4) Детермінований топ
    [Fact]
    public void TopByQuantity_IsDeterministic()
    {
        // Act
        var result = ReportService.TopByQuantity(TestData.Items(), 2);
        
        // Assert (очікуємо Товар 2 (50) та Товар 1 (10))
        Assert.Equal(new[] { "SKU-BIG", "SKU-MID" }, result.Select(r => r.Sku));
    }
}