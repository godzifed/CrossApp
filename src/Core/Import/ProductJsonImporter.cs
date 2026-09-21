using System.Text;
using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ImportResult<ProductDto> Load(string path)
    {
        var items = new List<ProductDto>();
        var errors = new List<string>();

        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            List<ProductDto> parsed = JsonSerializer.Deserialize<List<ProductDto>>(json, Options) ?? [];

            for (int i = 0; i < parsed.Count; i++)
            {
                ProductDto item = parsed[i];
                if (string.IsNullOrWhiteSpace(item.Id) || string.IsNullOrWhiteSpace(item.Name))
                {
                    errors.Add($"елемент {i + 1}: Id або назва порожні");
                }
                else if (item.Quantity < 0)
                {
                    errors.Add($"елемент {i + 1}: кількість '{item.Quantity}' не може бути від'ємною");
                }
                else
                {
                    items.Add(item);
                }
            }
        }
        catch (JsonException ex)
        {
            errors.Add($"Помилка структури JSON: {ex.Message}");
        }

        return new ImportResult<ProductDto>(items, errors);
    }
}