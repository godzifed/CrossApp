using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.Tests;

public sealed class ProductsApiTests
{
    [Fact]
    public async Task Post_CreatesProduct_And_GetByLocation_Returns200()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var request = new SaveProductRequest("A-100", "Цемент М400", "мішок", 40);

        var postResponse = await client.PostAsJsonAsync("/api/products", request);

        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        Assert.NotNull(postResponse.Headers.Location);

        var getResponse = await client.GetAsync(postResponse.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var product = await getResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(product);
        Assert.Equal("A-100", product.Sku);
        Assert.Equal("Цемент М400", product.Name);
        Assert.Equal("мішок", product.Unit);
        Assert.Equal(40, product.Quantity);
    }

    [Fact]
    public async Task Post_InvalidPayload_Returns400_WithValidationErrors()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var invalid = new SaveProductRequest("", "X", "", -1);

        var response = await client.PostAsJsonAsync("/api/products", invalid);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errors = doc.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("Sku", out _));
        Assert.True(errors.TryGetProperty("Unit", out _));
        Assert.True(errors.TryGetProperty("Quantity", out _));
    }

    [Fact]
    public async Task Post_DuplicateSku_Returns409Conflict_WithoutExceptionDetails()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var request = new SaveProductRequest("DUP-1", "Гвинт М6", "шт", 10);

        var first = await client.PostAsJsonAsync("/api/products", request);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var duplicate = await client.PostAsJsonAsync("/api/products", request);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var body = await duplicate.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Exception", body);
    }

    [Fact]
    public async Task Get_WithPaginationAndSearch_ReturnsPagedResult()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/products", new SaveProductRequest("C-01", "Кабель ВВГ", "м", 50));
        await client.PostAsJsonAsync("/api/products", new SaveProductRequest("C-02", "Кабель ПВС", "м", 30));
        await client.PostAsJsonAsync("/api/products", new SaveProductRequest("B-01", "Гвинт М8", "шт", 100));

        var response = await client.GetAsync("/api/products?page=1&pageSize=1&search=Кабель");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>();
        Assert.NotNull(page);
        Assert.Equal(2, page.Total);
        Assert.Single(page.Items);
        Assert.Equal("C-01", page.Items[0].Sku);
    }

    [Fact]
    public async Task Put_ThenDelete_ThenGet_Returns404()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var created = await (await client.PostAsJsonAsync(
            "/api/products", new SaveProductRequest("T-1", "Дюбель", "шт", 20)))
            .Content.ReadFromJsonAsync<ProductDto>();

        var putResponse = await client.PutAsJsonAsync(
            $"/api/products/{created!.Id}",
            new SaveProductRequest("T-1", "Дюбель 8x60", "шт", 3));
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/products/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDelete = await client.GetAsync($"/api/products/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }
}