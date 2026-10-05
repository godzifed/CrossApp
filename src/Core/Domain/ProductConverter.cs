using Core.Dto;

namespace Core.Domain;

public static class ProductConverter
{
    // Приймає ImportResult тижня 3, конвертує DTO в сутності та фіксує порушення інваріантів
    public static (IReadOnlyList<Product> Entities, IReadOnlyList<string> DomainErrors) ToEntities(ImportResult<ProductDto> importResult)
    {
        var entities = new List<Product>();
        var domainErrors = new List<string>(importResult.Errors);

        foreach (ProductDto dto in importResult.Items)
        {
            try
            {
                // FromDto виконує виклик фабрики Create із перевіркою всіх інваріантів
                entities.Add(Product.FromDto(dto));
            }
            catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException)
            {
                domainErrors.Add($"DTO [{dto.Id}] '{dto.Name}' порушує інваріант: {ex.Message}");
            }
        }

        return (entities.AsReadOnly(), domainErrors.AsReadOnly());
    }
}