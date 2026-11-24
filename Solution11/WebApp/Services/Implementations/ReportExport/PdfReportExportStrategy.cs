using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WebApplication1.Models.Reports;

namespace WebApplication1.Services.Implementations.ReportExport;

public sealed class PdfReportExportStrategy : IReportExportStrategy
{
    public string Format => "pdf";

    public byte[] Export(DashboardReportDto report)
    {
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

    private static void Header(TableDescriptor table, params string[] values)
    {
        foreach (var value in values)
            table.Cell().Element(CellStyle).Text(value).Bold();
    }

    private static void Cell(TableDescriptor table, string value) =>
        table.Cell().Element(CellStyle).Text(value);

    private static IContainer CellStyle(IContainer container) =>
        container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).PaddingHorizontal(3);
}
