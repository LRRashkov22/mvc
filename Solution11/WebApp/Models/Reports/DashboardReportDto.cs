namespace WebApplication1.Models.Reports;

public sealed class DashboardReportDto
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalWarehouses { get; set; }
    public int TotalDeliveries { get; set; }
    public long TotalDeliveredUnits { get; set; }
    public long TotalStockUnits { get; set; }
    public decimal TotalStockValue { get; set; }
    public IReadOnlyList<MonthlyDeliveryDto> MonthlyDeliveries { get; set; } = [];
    public IReadOnlyList<TopProductDto> TopProducts { get; set; } = [];
    public IReadOnlyList<WarehouseStockDto> WarehouseStocks { get; set; } = [];
}

public sealed class MonthlyDeliveryDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int DeliveryCount { get; set; }
    public long Quantity { get; set; }
    public string Label => $"{Year}-{Month:D2}";
}

public sealed class TopProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public long DeliveredQuantity { get; set; }
    public int DeliveryCount { get; set; }
}

public sealed class WarehouseStockDto
{
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public long TotalUnits { get; set; }
    public int ProductCount { get; set; }
    public decimal StockValue { get; set; }
}
