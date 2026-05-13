using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Mapping;

namespace CleaningPlatformAPI.Managers;

public class ReportingManager
{
    private readonly AppDbContext _db;

    public ReportingManager(AppDbContext db) => _db = db;

    public async Task<List<MonthlyRevenueResponse>> GetMonthlyRevenueAsync(CancellationToken ct = default) =>
        (await _db.MonthlyRevenueViews.AsNoTracking().OrderByDescending(v => v.Year).ThenByDescending(v => v.Month).ToListAsync(ct)).Select(ReportingMapper.ToResponse).ToList();

    public async Task<List<TopClientResponse>> GetTopClientsAsync(CancellationToken ct = default) =>
        (await _db.TopClientViews.AsNoTracking().ToListAsync(ct)).Select(ReportingMapper.ToResponse).ToList();

    public async Task<List<EmployeeUtilizationResponse>> GetEmployeeUtilizationAsync(CancellationToken ct = default) =>
        (await _db.EmployeeUtilizationViews.AsNoTracking().OrderBy(v => v.EmployeeName).ToListAsync(ct)).Select(ReportingMapper.ToResponse).ToList();

    public async Task<List<JobCompletionRateResponse>> GetJobCompletionRateAsync(CancellationToken ct = default) =>
        (await _db.JobCompletionRateViews.AsNoTracking().OrderByDescending(v => v.Year).ThenByDescending(v => v.Month).ToListAsync(ct)).Select(ReportingMapper.ToResponse).ToList();

    public async Task<OverdueInvoiceSummaryResponse> GetOverdueInvoiceSummaryAsync(CancellationToken ct = default)
    {
        var view = await _db.OverdueInvoiceSummaryViews.AsNoTracking().FirstOrDefaultAsync(ct);
        return view is null ? new OverdueInvoiceSummaryResponse(0, 0, 0, 0) : ReportingMapper.ToResponse(view);
    }

    public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(CancellationToken ct = default)
    {
        var revenue = (await GetMonthlyRevenueAsync(ct)).FirstOrDefault();
        var topClient = (await GetTopClientsAsync(ct)).FirstOrDefault();
        var completion = (await GetJobCompletionRateAsync(ct)).FirstOrDefault();
        var overdue = await GetOverdueInvoiceSummaryAsync(ct);
        return new DashboardSummaryResponse(revenue, topClient, completion, overdue);
    }

    public async Task<List<InvoiceExportResponse>> GetInvoiceExportDataAsync(CancellationToken ct = default) =>
        (await _db.InvoiceSummaryViews.AsNoTracking().OrderByDescending(v => v.IssueDate).ToListAsync(ct)).Select(ReportingMapper.ToResponse).ToList();
}
