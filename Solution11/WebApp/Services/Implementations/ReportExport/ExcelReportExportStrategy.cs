using ClosedXML.Excel;
using WebApplication1.Models.Reports;

namespace WebApplication1.Services.Implementations.ReportExport;

public sealed class ExcelReportExportStrategy : IReportExportStrategy
{
    public string Format => "excel";

    public byte[] Export(DashboardReportDto report)
    {
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

    private static void StyleHeader(IXLRange range)
    {
        range.Style.Font.Bold = true;
        range.Style.Fill.BackgroundColor = XLColor.LightGray;
    }
}
