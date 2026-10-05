namespace Core.Domain;

public sealed class Warehouse
{
    public string Id { get; }
    public string Name { get; }
    public int MaxCapacity { get; }

    private Warehouse(string id, string name, int maxCapacity)
    {
        Id = id;
        Name = name;
        MaxCapacity = maxCapacity;
    }

    public static Warehouse Create(string id, string name, int maxCapacity)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор складу обов'язковий", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва складу обов'язкова", nameof(name));

        if (maxCapacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxCapacity), maxCapacity, "Місткість складу має бути більшою за нуль");

        return new Warehouse(id.Trim(), name.Trim(), maxCapacity);
    }
}