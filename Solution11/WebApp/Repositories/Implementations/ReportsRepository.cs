using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models.Reports;
using WebApplication1.Repositories.Contracts;

namespace WebApplication1.Repositories.Implementations;

public sealed class ReportsRepository : IReportsRepository
{
    private readonly WarehouseDbContext _db;

    public ReportsRepository(WarehouseDbContext db) => _db = db;

    public async Task<DashboardReportDto> GetDashboardAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default)
    {
        var fromDate = from.ToDateTime(TimeOnly.MinValue);
        var toExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var deliveries = await _db.Deliveries
            .AsNoTracking()
            .Where(d => d.DeliveryDate >= fromDate && d.DeliveryDate < toExclusive)
            .Select(d => new
            {
                d.DeliveryDate,
                d.Quantity,
                d.ProductId,
                ProductName = d.Product == null ? null : d.Product.Name
            })
            .ToListAsync(ct);

        var inventory = await _db.Inventory
            .AsNoTracking()
            .Select(i => new
            {
                i.WarehouseId,
                WarehouseName = i.Warehouse == null ? $"Warehouse #{i.WarehouseId}" : i.Warehouse.Name,
                i.ProductId,
                i.Quantity,
                UnitPrice = i.Product == null ? 0m : i.Product.Price
            })
            .ToListAsync(ct);

        var monthlyGroups = deliveries
            .GroupBy(d => new { d.DeliveryDate.Year, d.DeliveryDate.Month })
            .ToDictionary(g => (g.Key.Year, g.Key.Month));

        var monthly = new List<MonthlyDeliveryDto>();
        var month = new DateOnly(from.Year, from.Month, 1);
        var lastMonth = new DateOnly(to.Year, to.Month, 1);

        while (month <= lastMonth)
        {
            monthlyGroups.TryGetValue((month.Year, month.Month), out var group);
            monthly.Add(new MonthlyDeliveryDto
            {
                Year = month.Year,
                Month = month.Month,
                DeliveryCount = group?.Count() ?? 0,
                Quantity = group?.Sum(x => (long)x.Quantity) ?? 0
            });
            month = month.AddMonths(1);
        }

        var topProducts = deliveries
            .GroupBy(d => d.ProductId)
            .Select(g => new TopProductDto
            {
                ProductId = g.Key,
                ProductName = g.Select(x => x.ProductName).FirstOrDefault(x => x != null)
                    ?? $"Product #{g.Key}",
                DeliveredQuantity = g.Sum(x => (long)x.Quantity),
                DeliveryCount = g.Count()
            })
            .OrderByDescending(x => x.DeliveredQuantity)
            .ThenBy(x => x.ProductName)
            .Take(10)
            .ToList();

        var warehouseStocks = inventory
            .GroupBy(i => new { i.WarehouseId, i.WarehouseName })
            .Select(g => new WarehouseStockDto
            {
                WarehouseId = g.Key.WarehouseId,
                WarehouseName = g.Key.WarehouseName,
                TotalUnits = g.Sum(x => (long)x.Quantity),
                ProductCount = g.Select(x => x.ProductId).Distinct().Count(),
                StockValue = g.Sum(x => x.UnitPrice * x.Quantity)
            })
            .OrderByDescending(x => x.TotalUnits)
            .ToList();

        return new DashboardReportDto
        {
            From = from,
            To = to,
            TotalProducts = await _db.Products.CountAsync(ct),
            TotalCategories = await _db.Categories.CountAsync(ct),
            TotalWarehouses = await _db.Warehouses.CountAsync(ct),
            TotalDeliveries = deliveries.Count,
            TotalDeliveredUnits = deliveries.Sum(x => (long)x.Quantity),
            TotalStockUnits = inventory.Sum(x => (long)x.Quantity),
            TotalStockValue = inventory.Sum(x => x.UnitPrice * x.Quantity),
            MonthlyDeliveries = monthly,
            TopProducts = topProducts,
            WarehouseStocks = warehouseStocks
        };
    }
}
