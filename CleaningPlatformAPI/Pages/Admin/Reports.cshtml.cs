using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Controllers;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Pages.Admin;

[Authorize(Policy = PermissionKeys.PagesReports)]
public class ReportsModel : PageModel
{
    private readonly ReportingManager _reportingManager;

    public ReportsModel(ReportingManager reportingManager) => _reportingManager = reportingManager;

    public List<MonthlyRevenueResponse> Revenue { get; set; } = [];
    public List<TopClientResponse> TopClients { get; set; } = [];
    public List<EmployeeUtilizationResponse> Utilization { get; set; } = [];
    public List<JobCompletionRateResponse> CompletionRates { get; set; } = [];
    public OverdueInvoiceSummaryResponse Overdue { get; set; } = new(0, 0, 0, 0);

    public async Task OnGetAsync(CancellationToken ct) => await LoadAsync(ct);

    public async Task<IActionResult> OnPostExportAsync(CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.ActionsReportsExport))
            return Forbid();

        var rows = await _reportingManager.GetInvoiceExportDataAsync(ct);
        return File(ReportingController.BuildInvoiceWorkbook(rows), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"invoice-export-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        Revenue = await _reportingManager.GetMonthlyRevenueAsync(ct);
        TopClients = await _reportingManager.GetTopClientsAsync(ct);
        Utilization = await _reportingManager.GetEmployeeUtilizationAsync(ct);
        CompletionRates = await _reportingManager.GetJobCompletionRateAsync(ct);
        Overdue = await _reportingManager.GetOverdueInvoiceSummaryAsync(ct);
    }
}
