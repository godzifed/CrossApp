using System.Security.Cryptography;
using Api.Contracts;
using Api.Filters;
using Core;
using Core.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProducts(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", Results<Ok<PagedResult<ProductDto>>, ValidationProblem>
            (ICatalogStore store, int page = 1, int pageSize = 20, string? search = null) =>
        {
            var errors = new Dictionary<string, string[]>();
            if (page < 1)
                errors["page"] = ["Номер сторінки має бути не меншим за 1."];
            if (pageSize is < 1 or > 100)
                errors["pageSize"] = ["Розмір сторінки має бути в межах від 1 до 100."];

            if (errors.Count > 0)
                return TypedResults.ValidationProblem(errors);

            IEnumerable<Product> query = store.List();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(p =>
                    p.Sku.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            var ordered = query
                .OrderBy(p => p.Sku, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.Id, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var items = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ProductDto.FromEntity)
                .ToList();

            return TypedResults.Ok(new PagedResult<ProductDto>(items, ordered.Count));
        });

        group.MapGet("/low-stock", Results<Ok<List<ProductDto>>, ValidationProblem>
            (ICatalogStore store, int threshold = 5) =>
        {
            if (threshold < 0)
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["threshold"] = ["Поріг залишку не може бути від'ємним."]
                });
            }

            var low = store.List()
                .Where(p => p.Quantity < threshold)
                .OrderBy(p => p.Quantity)
                .ThenBy(p => p.Sku, StringComparer.OrdinalIgnoreCase)
                .Select(ProductDto.FromEntity)
                .ToList();

            return TypedResults.Ok(low);
        });

        group.MapGet("/{id}", Results<Ok<ProductDto>, NotFound>
            (string id, ICatalogStore store) =>
                store.GetById(id) is { } found
                    ? TypedResults.Ok(ProductDto.FromEntity(found))
                    : TypedResults.NotFound());

        group.MapPost("/", (SaveProductRequest request, ICatalogStore store) =>
        {
            var sku = request.Sku.Trim();
            if (store.List().Any(p => p.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase)))
                throw new DomainException($"Товар з артикулом '{sku}' уже існує.");

            var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
            var created = Product.Create(
                id, sku, request.Name.Trim(), request.Unit.Trim(), request.Quantity);

            store.Add(created);
            return TypedResults.Created($"/api/products/{created.Id}", ProductDto.FromEntity(created));
        })
        .AddEndpointFilter<ValidationFilter<SaveProductRequest>>()
        .Produces<ProductDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id}", Results<Ok<ProductDto>, NotFound>
            (string id, SaveProductRequest request, ICatalogStore store) =>
        {
            if (store.GetById(id) is null)
                return TypedResults.NotFound();

            var sku = request.Sku.Trim();
            var duplicate = store.List().Any(p =>
                !p.Id.Equals(id, StringComparison.OrdinalIgnoreCase) &&
                p.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase));

            if (duplicate)
                throw new DomainException($"Товар з артикулом '{sku}' уже існує.");

            var replacement = Product.Create(
                id, sku, request.Name.Trim(), request.Unit.Trim(), request.Quantity);

            store.Update(replacement);
            return TypedResults.Ok(ProductDto.FromEntity(replacement));
        })
        .AddEndpointFilter<ValidationFilter<SaveProductRequest>>()
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id}", Results<NoContent, NotFound>
            (string id, ICatalogStore store) =>
                store.Remove(id)
                    ? TypedResults.NoContent()
                    : TypedResults.NotFound());
    }
}