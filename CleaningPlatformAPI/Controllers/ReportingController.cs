using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Policy = PermissionKeys.PagesReports)]
public class ReportingController : ControllerBase
{
    private readonly ReportingManager _reportingManager;

    public ReportingController(ReportingManager reportingManager) => _reportingManager = reportingManager;

    [HttpGet("revenue")]
    public async Task<OperationResult<List<MonthlyRevenueResponse>>> Revenue(CancellationToken ct) => OperationResult<List<MonthlyRevenueResponse>>.Ok(await _reportingManager.GetMonthlyRevenueAsync(ct));

    [HttpGet("top-clients")]
    public async Task<OperationResult<List<TopClientResponse>>> TopClients(CancellationToken ct) => OperationResult<List<TopClientResponse>>.Ok(await _reportingManager.GetTopClientsAsync(ct));

    [HttpGet("utilization")]
    public async Task<OperationResult<List<EmployeeUtilizationResponse>>> Utilization(CancellationToken ct) => OperationResult<List<EmployeeUtilizationResponse>>.Ok(await _reportingManager.GetEmployeeUtilizationAsync(ct));

    [HttpGet("completion")]
    public async Task<OperationResult<List<JobCompletionRateResponse>>> Completion(CancellationToken ct) => OperationResult<List<JobCompletionRateResponse>>.Ok(await _reportingManager.GetJobCompletionRateAsync(ct));

    [HttpGet("overdue")]
    public async Task<OperationResult<OverdueInvoiceSummaryResponse>> Overdue(CancellationToken ct) => OperationResult<OverdueInvoiceSummaryResponse>.Ok(await _reportingManager.GetOverdueInvoiceSummaryAsync(ct));

    [HttpGet("export")]
    [Authorize(Policy = PermissionKeys.ActionsReportsExport)]
    public async Task<IActionResult> Export(CancellationToken ct)
    {
        var rows = await _reportingManager.GetInvoiceExportDataAsync(ct);
        return File(BuildInvoiceWorkbook(rows), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"invoice-export-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
    }

    internal static byte[] BuildInvoiceWorkbook(IEnumerable<InvoiceExportResponse> rows)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Invoices");
        var headers = new[] { "InvoiceId", "InvoiceNumber", "ClientId", "ClientName", "IssueDate", "DueDate", "SubTotal", "DiscountAmount", "VatAmount", "TotalAmount", "Status", "AmountPaid", "AmountOutstanding", "IsOverdue", "DaysOverdue", "CreatedBy" };
        for (var i = 0; i < headers.Length; i++)
            worksheet.Cell(1, i + 1).Value = headers[i];

        var rowNumber = 2;
        foreach (var r in rows)
        {
            worksheet.Cell(rowNumber, 1).Value = r.InvoiceId;
            worksheet.Cell(rowNumber, 2).Value = r.InvoiceNumber;
            worksheet.Cell(rowNumber, 3).Value = r.ClientId;
            worksheet.Cell(rowNumber, 4).Value = r.ClientName;
            worksheet.Cell(rowNumber, 5).Value = r.IssueDate;
            worksheet.Cell(rowNumber, 6).Value = r.DueDate;
            worksheet.Cell(rowNumber, 7).Value = r.SubTotal;
            worksheet.Cell(rowNumber, 8).Value = r.DiscountAmount;
            worksheet.Cell(rowNumber, 9).Value = r.VatAmount;
            worksheet.Cell(rowNumber, 10).Value = r.TotalAmount;
            worksheet.Cell(rowNumber, 11).Value = r.Status;
            worksheet.Cell(rowNumber, 12).Value = r.AmountPaid;
            worksheet.Cell(rowNumber, 13).Value = r.AmountOutstanding;
            worksheet.Cell(rowNumber, 14).Value = r.IsOverdue;
            worksheet.Cell(rowNumber, 15).Value = r.DaysOverdue;
            worksheet.Cell(rowNumber, 16).Value = r.CreatedBy;
            rowNumber++;
        }

        worksheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
