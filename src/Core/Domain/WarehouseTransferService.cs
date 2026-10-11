namespace Core.Domain;

public static class WarehouseTransferService
{
    // Інваріант, що охоплює дві сутності: Warehouse і Product
    public static void PlaceProduct(Warehouse warehouse, Product product, int currentWarehouseTotalQuantity)
    {
        if (currentWarehouseTotalQuantity + product.Quantity > warehouse.MaxCapacity)
        {
            throw new InvalidOperationException(
                $"Склад '{warehouse.Name}' переповнений: сумарний залишок після розміщення ({currentWarehouseTotalQuantity + product.Quantity}) перевищить місткість ({warehouse.MaxCapacity})");
        }
    }
}