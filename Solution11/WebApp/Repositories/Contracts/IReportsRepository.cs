using WebApplication1.Models.Reports;

namespace WebApplication1.Repositories.Contracts;

public interface IReportsRepository
{
    Task<DashboardReportDto> GetDashboardAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default);
}
