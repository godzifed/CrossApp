namespace Core.Domain;

public enum MovementStatus
{
    Draft,
    Confirmed,
    Cancelled
}

public sealed class StockMovement
{
    public string Id { get; }
    public string ProductId { get; }
    public int Amount { get; }
    public MovementStatus Status { get; private set; }

    private StockMovement(string id, string productId, int amount)
    {
        Id = id;
        ProductId = productId;
        Amount = amount;
        Status = MovementStatus.Draft;
    }

    public static StockMovement Create(string id, string productId, int amount)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор переміщення обов'язковий", nameof(id));

        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Ідентифікатор товару обов'язковий", nameof(productId));

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Кількість переміщення має бути більшою за нуль");

        return new StockMovement(id.Trim(), productId.Trim(), amount);
    }

    // Перевірка переходів між станами за допомогою switch expression
    public void ChangeStatus(MovementStatus newStatus)
    {
        bool isAllowed = (Status, newStatus) switch
        {
            // Дозволені переходи:
            (MovementStatus.Draft, MovementStatus.Confirmed) => true,
            (MovementStatus.Draft, MovementStatus.Cancelled) => true,

            // Перехід у той самий статус
            var (current, target) when current == target => true,

            // Усі інші переходи заборонені бізнес-правилами
            _ => false
        };

        if (!isAllowed)
        {
            throw new InvalidOperationException(
                $"Неприпустимий перехід статусу переміщення {Id}: не можна змінити '{Status}' на '{newStatus}'");
        }

        Status = newStatus;
    }
}