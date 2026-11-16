using Core.Events;
using Core.Import;
using Core.Services;
using Core.Storage;


namespace Core.Tests;

public class CatalogServiceTests
{
    // 1) Інваріант: відмова і незмінний стан
    [Fact]
    public void Receive_WithNegativeQuantity_Throws_AndKeepsState()
    {
        // Arrange
        var store = new InMemoryCatalogStore();
        var service = new CatalogService(store, Array.Empty<Core.Abstractions.INotificationSink>());
        var p = service.Add("SKU-1", "Кабель", "м", 10);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Receive(p.Id, -5));
        
        // Assert state
        Assert.Equal(10, store.GetById(p.Id)!.Quantity);
    }

    // 2) Подія як спосіб перевірки
    [Fact]
    public void Add_RaisesChangedExactlyOnce()
    {
        // Arrange
       var service = new CatalogService(new InMemoryCatalogStore(), Array.Empty<Core.Abstractions.INotificationSink>());
        var events = new List<CatalogChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);

        // Act
        service.Add("SKU-2", "Гвинт", "шт", 7);

        // Assert
        Assert.Single(events);
        Assert.Equal(CatalogChangeKind.Added, events[0].Kind);
    }

    // 5) Скасування операції
    [Fact]
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync(); // Заздалегідь скасовуємо токен
        var service = new CatalogService(new InMemoryCatalogStore(), Array.Empty<Core.Abstractions.INotificationSink>());

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(new JsonCatalogImporter(), "any.json", null, cts.Token));
    }
    
    // Додаткове завдання 2: Тест на зв'язані токени
    [Fact]
    public async Task ImportAsync_WithLinkedTokens_ThrowsWhenTimeoutOccurs()
    {
        // Arrange
        using var userCts = new CancellationTokenSource();
        using var timeoutCts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);
        
        var service = new CatalogService(new InMemoryCatalogStore(), Array.Empty<Core.Abstractions.INotificationSink>());
        
        // Act - симулюємо таймаут
        await timeoutCts.CancelAsync();

        // Assert
        var ex = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(new JsonCatalogImporter(), "any.json", null, linkedCts.Token));
            
        // Переконуємось, що токен скасовано
        Assert.True(linkedCts.IsCancellationRequested);
    }
}