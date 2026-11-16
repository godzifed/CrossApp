using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Core.Import;
using Xunit;

namespace Core.Tests;

public class ImportTests
{
    // 6) Тест на зіпсований файл та викидання DomainException
    [Fact]
    public async Task LoadAsync_WithInvalidJson_ThrowsDomainException()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, "not a valid json");
        var importer = new JsonCatalogImporter();

        try
        {
            // Act & Assert
            // Оскільки ми використовуємо IAsyncEnumerable, потрібно ітерувати, щоб спровокувати виконання
            var ex = await Assert.ThrowsAsync<DomainException>(async () => 
            {
                await foreach (var item in importer.LoadAsync(tempFile)) { }
            });
            
            Assert.Contains("невалідний JSON", ex.Message);
        }
        finally
        {
            // Завжди видаляємо тимчасовий файл
            File.Delete(tempFile);
        }
    }
}