using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.Contracts;

namespace WebApplication1.Controllers;

[Authorize(Roles = "Admin")]
[Route("reports")]
public sealed class ReportsController : Controller
{
    private readonly IReportsService _reports;

    public ReportsController(IReportsService reports) => _reports = reports;

    [HttpGet("")]
    public async Task<IActionResult> Index( DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var start = from ?? new DateOnly(today.Year, 1, 1);
        var end = to ?? today;

        try
        {
            var report = await _reports.GetDashboardAsync(start, end, ct);
            return View(report);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return BadRequest(ModelState);
        }
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf( DateOnly from, DateOnly to, CancellationToken ct)
    {
        try
        {
            var content = await _reports.ExportPdfAsync(from, to, ct);
            return File(content, "application/pdf", $"warehouse-report-{from:yyyyMMdd}-{to:yyyyMMdd}.pdf");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportExcel(
        DateOnly from,
        DateOnly to,
        CancellationToken ct)
    {
        try
        {
            var content = await _reports.ExportExcelAsync(from, to, ct);
            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"warehouse-report-{from:yyyyMMdd}-{to:yyyyMMdd}.xlsx");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
