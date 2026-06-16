using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Reporting.Application.Services;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.Reporting.Interfaces.Rest;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ReportingController(IReportingService reportingService, AppDbContext db) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirst("user_id")!.Value);

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportPdf()
    {
        if (!await IsPremiumAsync(CurrentUserId))
            return StatusCode(403, new { message = "Esta función requiere una suscripción Premium." });

        var pdf = await reportingService.GeneratePdfAsync(CurrentUserId);
        var fileName = $"mindflow-report-{DateTime.UtcNow:yyyyMMdd}.pdf";
        return File(pdf, "application/pdf", fileName);
    }

    [HttpGet("export/csv")]
    public async Task<IActionResult> ExportCsv()
    {
        if (!await IsPremiumAsync(CurrentUserId))
            return StatusCode(403, new { message = "Esta función requiere una suscripción Premium." });

        var csv = await reportingService.GenerateCsvAsync(CurrentUserId);
        var fileName = $"mindflow-entries-{DateTime.UtcNow:yyyyMMdd}.csv";
        return File(csv, "text/csv", fileName);
    }

    private async Task<bool> IsPremiumAsync(int userId)
    {
        try
        {
            var result = await db.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS Value FROM subscriptions WHERE user_id = {0} AND plan = 'premium' AND status = 'active'",
                    userId)
                .FirstOrDefaultAsync();
            return result > 0;
        }
        catch
        {
            // subscriptions table not yet migrated — block access to stay safe
            return false;
        }
    }
}
