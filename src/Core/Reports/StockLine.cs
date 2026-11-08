namespace Core.Reports;

// DTO для рядка звіту (Крок 1 з методички)
public sealed record StockLine(string Sku, string Name, int Quantity, string Unit);