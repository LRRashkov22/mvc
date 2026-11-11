using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WebApplication1.Models.Reports;
using WebApplication1.Repositories.Contracts;
using WebApplication1.Services.Contracts;

namespace WebApplication1.Services.Implementations;

public sealed class ReportsService : IReportsService
{
    private readonly IReportsRepository _reports;

    public ReportsService(IReportsRepository reports) => _reports = reports;

    public Task<DashboardReportDto> GetDashboardAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default)
    {
        ValidatePeriod(from, to);
        return _reports.GetDashboardAsync(from, to, ct);
    }

    public async Task<byte[]> ExportPdfAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default)
    {
        var report = await GetDashboardAsync(from, to, ct);

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(column =>
                {
                    column.Item().Text("Stockroom - Warehouse Report")
                        .FontSize(20).Bold();
                    column.Item().Text($"Period: {report.From:dd.MM.yyyy} - {report.To:dd.MM.yyyy}")
                        .FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingVertical(15).Column(column =>
                {
                    column.Spacing(12);
                    column.Item().Text("Summary").FontSize(14).Bold();
                    column.Item().Text(
                        $"Products: {report.TotalProducts} | Categories: {report.TotalCategories} | Warehouses: {report.TotalWarehouses}");
                    column.Item().Text(
                        $"Deliveries in period: {report.TotalDeliveries} | Delivered units: {report.TotalDeliveredUnits:N0}");
                    column.Item().Text(
                        $"Current stock: {report.TotalStockUnits:N0} units | Estimated stock value: {report.TotalStockValue:N2}");

                    column.Item().Text("Monthly deliveries").FontSize(14).Bold();
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });
                        Header(table, "Month", "Deliveries", "Quantity");
                        foreach (var row in report.MonthlyDeliveries)
                        {
                            Cell(table, row.Label);
                            Cell(table, row.DeliveryCount.ToString("N0"));
                            Cell(table, row.Quantity.ToString("N0"));
                        }
                    });

                    column.Item().Text("Top delivered products").FontSize(14).Bold();
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });
                        Header(table, "ID", "Product", "Quantity", "Deliveries");
                        foreach (var row in report.TopProducts)
                        {
                            Cell(table, row.ProductId.ToString());
                            Cell(table, row.ProductName);
                            Cell(table, row.DeliveredQuantity.ToString("N0"));
                            Cell(table, row.DeliveryCount.ToString("N0"));
                        }
                    });

                    column.Item().Text("Stock by warehouse").FontSize(14).Bold();
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });
                        Header(table, "Warehouse", "Units", "Products", "Stock value");
                        foreach (var row in report.WarehouseStocks)
                        {
                            Cell(table, row.WarehouseName);
                            Cell(table, row.TotalUnits.ToString("N0"));
                            Cell(table, row.ProductCount.ToString("N0"));
                            Cell(table, row.StockValue.ToString("N2"));
                        }
                    });
                });

                page.Footer().AlignRight().Text(text =>
                {
                    text.Span("Generated: ");
                    text.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                    text.Span(" | Page ");
                    text.CurrentPageNumber();
                });
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> ExportExcelAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default)
    {
        var report = await GetDashboardAsync(from, to, ct);
        using var workbook = new XLWorkbook();

        var summary = workbook.Worksheets.Add("Summary");
        summary.Cell("A1").Value = "Stockroom - Warehouse Report";
        summary.Cell("A1").Style.Font.Bold = true;
        summary.Cell("A1").Style.Font.FontSize = 16;
        summary.Cell("A2").Value = "From";
        summary.Cell("B2").Value = report.From.ToDateTime(TimeOnly.MinValue);
        summary.Cell("A3").Value = "To";
        summary.Cell("B3").Value = report.To.ToDateTime(TimeOnly.MinValue);
        summary.Cell("A5").Value = "Metric";
        summary.Cell("B5").Value = "Value";
        summary.Cell("A6").Value = "Products";
        summary.Cell("B6").Value = report.TotalProducts;
        summary.Cell("A7").Value = "Categories";
        summary.Cell("B7").Value = report.TotalCategories;
        summary.Cell("A8").Value = "Warehouses";
        summary.Cell("B8").Value = report.TotalWarehouses;
        summary.Cell("A9").Value = "Deliveries in period";
        summary.Cell("B9").Value = report.TotalDeliveries;
        summary.Cell("A10").Value = "Delivered units in period";
        summary.Cell("B10").Value = report.TotalDeliveredUnits;
        summary.Cell("A11").Value = "Current stock units";
        summary.Cell("B11").Value = report.TotalStockUnits;
        summary.Cell("A12").Value = "Current stock value";
        summary.Cell("B12").Value = report.TotalStockValue;
        StyleHeader(summary.Range("A5:B5"));
        summary.Columns().AdjustToContents();
        summary.SheetView.FreezeRows(5);

        var monthly = workbook.Worksheets.Add("Monthly deliveries");
        monthly.Cell(1, 1).Value = "Month";
        monthly.Cell(1, 2).Value = "Deliveries";
        monthly.Cell(1, 3).Value = "Quantity";
        for (var i = 0; i < report.MonthlyDeliveries.Count; i++)
        {
            var row = i + 2;
            var item = report.MonthlyDeliveries[i];
            monthly.Cell(row, 1).Value = item.Label;
            monthly.Cell(row, 2).Value = item.DeliveryCount;
            monthly.Cell(row, 3).Value = item.Quantity;
        }
        StyleHeader(monthly.Range("A1:C1"));
        monthly.Columns().AdjustToContents();
        if (report.MonthlyDeliveries.Count > 0)
            monthly.RangeUsed()!.SetAutoFilter();

        var products = workbook.Worksheets.Add("Top products");
        products.Cell(1, 1).Value = "Product ID";
        products.Cell(1, 2).Value = "Product";
        products.Cell(1, 3).Value = "Delivered quantity";
        products.Cell(1, 4).Value = "Delivery count";
        for (var i = 0; i < report.TopProducts.Count; i++)
        {
            var row = i + 2;
            var item = report.TopProducts[i];
            products.Cell(row, 1).Value = item.ProductId;
            products.Cell(row, 2).Value = item.ProductName;
            products.Cell(row, 3).Value = item.DeliveredQuantity;
            products.Cell(row, 4).Value = item.DeliveryCount;
        }
        StyleHeader(products.Range("A1:D1"));
        products.Columns().AdjustToContents();
        if (report.TopProducts.Count > 0)
            products.RangeUsed()!.SetAutoFilter();

        var warehouses = workbook.Worksheets.Add("Warehouse stock");
        warehouses.Cell(1, 1).Value = "Warehouse ID";
        warehouses.Cell(1, 2).Value = "Warehouse";
        warehouses.Cell(1, 3).Value = "Stock units";
        warehouses.Cell(1, 4).Value = "Products";
        warehouses.Cell(1, 5).Value = "Stock value";
        for (var i = 0; i < report.WarehouseStocks.Count; i++)
        {
            var row = i + 2;
            var item = report.WarehouseStocks[i];
            warehouses.Cell(row, 1).Value = item.WarehouseId;
            warehouses.Cell(row, 2).Value = item.WarehouseName;
            warehouses.Cell(row, 3).Value = item.TotalUnits;
            warehouses.Cell(row, 4).Value = item.ProductCount;
            warehouses.Cell(row, 5).Value = item.StockValue;
        }
        StyleHeader(warehouses.Range("A1:E1"));
        warehouses.Columns().AdjustToContents();
        if (report.WarehouseStocks.Count > 0)
            warehouses.RangeUsed()!.SetAutoFilter();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void ValidatePeriod(DateOnly from, DateOnly to)
    {
        if (from > to)
            throw new ArgumentException("The start date must be before or equal to the end date.");

        if (to.DayNumber - from.DayNumber > 366 * 5)
            throw new ArgumentException("The report period cannot exceed five years.");
    }

    private static void Header(TableDescriptor table, params string[] values)
    {
        foreach (var value in values)
            table.Cell().Element(CellStyle).Text(value).Bold();
    }

    private static void Cell(TableDescriptor table, string value) =>
        table.Cell().Element(CellStyle).Text(value);

    private static IContainer CellStyle(IContainer container) =>
        container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).PaddingHorizontal(3);

    private static void StyleHeader(IXLRange range)
    {
        range.Style.Font.Bold = true;
        range.Style.Fill.BackgroundColor = XLColor.LightGray;
    }
}
