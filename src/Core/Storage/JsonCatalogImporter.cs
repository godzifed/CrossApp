using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using Core.Abstractions;
using Core.Domain;

namespace Core.Import;

public sealed class JsonCatalogImporter : ICatalogImporter
{
    // Додаткове завдання 1: використання IAsyncEnumerable та [EnumeratorCancellation]
    public async IAsyncEnumerable<Product> LoadAsync(
        string path, 
        [EnumeratorCancellation] CancellationToken ct = default)

    {

        ct.ThrowIfCancellationRequested();
        // Асинхронне читання через FileStream з useAsync: true
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        
        List<ProductDto>? dtos;
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            dtos = await JsonSerializer.DeserializeAsync<List<ProductDto>>(stream, options, cancellationToken: ct);
        }
        catch (JsonException ex)
        {
             // Загортаємо технічну помилку в доменну, зберігаючи inner exception
             throw new DomainException($"Файл {Path.GetFileName(path)} містить невалідний JSON.", ex);
        }

        if (dtos == null)
            throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку.");

        for (var i = 0; i < dtos.Count; i++)
        {
            ct.ThrowIfCancellationRequested(); // Перевірка скасування в циклі
            Product product;
            try 
            { 
                product = Product.FromDto(dtos[i]); 
            }
            catch (ArgumentException ex)
            { 
                throw new DomainException($"Запис {i + 1}: {ex.Message}", ex); 
            }
            
            yield return product; // Потокове повернення елементів
        }
    }
}