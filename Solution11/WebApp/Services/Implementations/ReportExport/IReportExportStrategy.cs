using WebApplication1.Models.Reports;

namespace WebApplication1.Services.Implementations.ReportExport;

/// <summary>Defines one interchangeable report output format.</summary>
public interface IReportExportStrategy
{
    string Format { get; }
    byte[] Export(DashboardReportDto report);
}
