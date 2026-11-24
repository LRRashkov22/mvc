using WebApplication1.Models.Reports;
using WebApplication1.Repositories.Contracts;
using WebApplication1.Services.Contracts;
using WebApplication1.Services.Implementations.ReportExport;

namespace WebApplication1.Services.Implementations;

public sealed class ReportsService : IReportsService
{
    private readonly IReportsRepository _reports;
    private readonly ReportExportStrategyFactory _exportFactory;

    public ReportsService(
        IReportsRepository reports,
        ReportExportStrategyFactory exportFactory)
    {
        _reports = reports;
        _exportFactory = exportFactory;
    }

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
        return _exportFactory.GetStrategy("pdf").Export(report);
    }

    public async Task<byte[]> ExportExcelAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default)
    {
        var report = await GetDashboardAsync(from, to, ct);
        return _exportFactory.GetStrategy("excel").Export(report);
    }

    private static void ValidatePeriod(DateOnly from, DateOnly to)
    {
        if (from > to)
            throw new ArgumentException("The start date must be before or equal to the end date.");

        if (to.DayNumber - from.DayNumber > 366 * 5)
            throw new ArgumentException("The report period cannot exceed five years.");
    }
}
