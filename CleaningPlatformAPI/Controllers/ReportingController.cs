using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Policy = PermissionKeys.ReportsView)]
public class ReportingController : ControllerBase
{
    private readonly ReportingManager _reportingManager;
    public ReportingController(ReportingManager reportingManager) { _reportingManager = reportingManager; }

    [HttpGet("revenue")]
    public async Task<ActionResult<OperationResult<List<MonthlyRevenueView>>>> Revenue(CancellationToken ct)
    {
        var data = await _reportingManager.GetMonthlyRevenueAsync(ct);
        return Ok(OperationResult<List<MonthlyRevenueView>>.Ok(data));
    }

    [HttpGet("top-clients")]
    public async Task<ActionResult<OperationResult<List<TopClientView>>>> TopClients([FromQuery] int? top, CancellationToken ct)
    {
        var data = await _reportingManager.GetTopClientsAsync(top ?? 10, ct);
        return Ok(OperationResult<List<TopClientView>>.Ok(data));
    }

    [HttpGet("utilization")]
    public async Task<ActionResult<OperationResult<List<EmployeeUtilizationView>>>> Utilization(CancellationToken ct)
    {
        var data = await _reportingManager.GetEmployeeUtilizationAsync(ct);
        return Ok(OperationResult<List<EmployeeUtilizationView>>.Ok(data));
    }

    [HttpGet("completion")]
    public async Task<ActionResult<OperationResult<List<JobCompletionRateView>>>> Completion(CancellationToken ct)
    {
        var data = await _reportingManager.GetJobCompletionRateAsync(ct);
        return Ok(OperationResult<List<JobCompletionRateView>>.Ok(data));
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<OperationResult<OverdueInvoiceSummaryView>>> Overdue(CancellationToken ct)
    {
        var data = await _reportingManager.GetOverdueInvoiceSummaryAsync(ct);
        return Ok(OperationResult<OverdueInvoiceSummaryView>.Ok(data));
    }

    [HttpGet("export")]
    [Authorize(Policy = PermissionKeys.ReportsExport)]
    public async Task<IActionResult> Export(CancellationToken ct)
    {
        var rows = await _reportingManager.GetInvoiceExportDataAsync(ct);
        return File(BuildInvoiceWorkbook(rows),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"invoice-export-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
    }

    internal static byte[] BuildInvoiceWorkbook(IEnumerable<InvoiceSummaryView> rows)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Invoices");
        var headers = new[] { "InvoiceId", "InvoiceNumber", "ClientId", "ClientName", "IssueDate", "DueDate", "SubTotal", "DiscountAmount", "VatAmount", "TotalAmount", "Status", "AmountPaid", "AmountOutstanding", "IsOverdue", "DaysOverdue", "CreatedBy" };
        for (var i = 0; i < headers.Length; i++)
            ws.Cell(1, i + 1).Value = headers[i];
        var rowNum = 2;
        foreach (var r in rows)
        {
            ws.Cell(rowNum, 1).Value = r.InvoiceId;
            ws.Cell(rowNum, 2).Value = r.InvoiceNumber;
            ws.Cell(rowNum, 3).Value = r.ClientId;
            ws.Cell(rowNum, 4).Value = r.ClientName;
            ws.Cell(rowNum, 5).Value = r.IssueDate;
            ws.Cell(rowNum, 6).Value = r.DueDate;
            ws.Cell(rowNum, 7).Value = r.SubTotal;
            ws.Cell(rowNum, 8).Value = r.DiscountAmount;
            ws.Cell(rowNum, 9).Value = r.VatAmount;
            ws.Cell(rowNum, 10).Value = r.TotalAmount;
            ws.Cell(rowNum, 11).Value = r.Status;
            ws.Cell(rowNum, 12).Value = r.AmountPaid;
            ws.Cell(rowNum, 13).Value = r.AmountOutstanding;
            ws.Cell(rowNum, 14).Value = r.IsOverdue;
            ws.Cell(rowNum, 15).Value = r.DaysOverdue;
            ws.Cell(rowNum, 16).Value = r.CreatedBy;
            rowNum++;
        }
        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<OperationResult<DashboardSummaryResponse>>> GetDashboardSummary(CancellationToken ct)
    {
        var result = await _reportingManager.GetDashboardSummaryAsync(ct);
        return Ok(OperationResult<DashboardSummaryResponse>.Ok(result));
    }
}