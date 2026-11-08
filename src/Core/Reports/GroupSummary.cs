namespace Core.Reports;

// DTO для згрупованого звіту (Крок 1 з методички)
public sealed record GroupSummary(string Key, int Count, int TotalQuantity);