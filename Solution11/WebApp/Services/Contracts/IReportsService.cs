using WebApplication1.Models.Reports;

namespace WebApplication1.Services.Contracts;

public interface IReportsService
{
    Task<DashboardReportDto> GetDashboardAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default);

    Task<byte[]> ExportPdfAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default);

    Task<byte[]> ExportExcelAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default);
}
