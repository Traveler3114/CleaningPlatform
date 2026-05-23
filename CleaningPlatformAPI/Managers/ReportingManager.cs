using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Managers;

public class ReportingManager
{
    private readonly AppDbContext _db;

    public ReportingManager(AppDbContext db) { _db = db; }

    public async Task<List<MonthlyRevenueView>> GetMonthlyRevenueAsync(CancellationToken ct = default)
    {
        return await _db.MonthlyRevenueViews.AsNoTracking().OrderByDescending(v => v.Year).ThenByDescending(v => v.Month).ToListAsync(ct);
    }

    public async Task<List<TopClientView>> GetTopClientsAsync(int count = 10, CancellationToken ct = default)
    {
        return await _db.TopClientViews.AsNoTracking().OrderByDescending(v => v.TotalBilled).Take(count).ToListAsync(ct);
    }

    public async Task<List<EmployeeUtilizationView>> GetEmployeeUtilizationAsync(CancellationToken ct = default)
    {
        return await _db.EmployeeUtilizationViews.AsNoTracking().OrderBy(v => v.EmployeeName).ToListAsync(ct);
    }

    public async Task<List<JobCompletionRateView>> GetJobCompletionRateAsync(CancellationToken ct = default)
    {
        return await _db.JobCompletionRateViews.AsNoTracking().OrderByDescending(v => v.Year).ThenByDescending(v => v.Month).ToListAsync(ct);
    }

    public async Task<OverdueInvoiceSummaryView> GetOverdueInvoiceSummaryAsync(CancellationToken ct = default)
    {
        return (await _db.OverdueInvoiceSummaryViews.AsNoTracking().FirstOrDefaultAsync(ct))!;
    }

    public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(CancellationToken ct = default)
    {
        var revenue = (await GetMonthlyRevenueAsync(ct)).FirstOrDefault();
        var topClient = (await GetTopClientsAsync(count: 10, ct)).FirstOrDefault();
        var completion = (await GetJobCompletionRateAsync(ct)).FirstOrDefault();
        var overdue = await _db.OverdueInvoiceSummaryViews.AsNoTracking().FirstOrDefaultAsync(ct) ?? new();
        return new DashboardSummaryResponse(revenue, topClient, completion, overdue);
    }

    public async Task<List<InvoiceSummaryView>> GetInvoiceExportDataAsync(CancellationToken ct = default)
    {
        return await _db.InvoiceSummaryViews.AsNoTracking().OrderByDescending(v => v.IssueDate).ToListAsync(ct);
    }
}
