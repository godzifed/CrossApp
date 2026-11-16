
using Core.Domain;

namespace Core.Abstractions;

public interface ICatalogImporter
{
    // Додаткове завдання 1: Потоковий імпорт через IAsyncEnumerable
    IAsyncEnumerable<Product> LoadAsync(string path, CancellationToken ct = default);
}