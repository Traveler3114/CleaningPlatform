using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Mapping;

public static class ReportingMapper
{
    public static MonthlyRevenueResponse ToResponse(MonthlyRevenueView v) => new(v.Year, v.Month, v.InvoiceCount, v.TotalRevenue, v.TotalVat, v.TotalDiscount);
    public static TopClientResponse ToResponse(TopClientView v) => new(v.ClientId, v.ClientName, v.InvoiceCount, v.TotalBilled, v.TotalPaid);
    public static EmployeeUtilizationResponse ToResponse(EmployeeUtilizationView v) => new(v.EmployeeId, v.EmployeeName, v.JobsAssigned, v.JobsCompleted, v.DaysActive, v.JobsAssigned == 0 ? 0 : Math.Round((decimal)v.JobsCompleted / v.JobsAssigned * 100, 1));
    public static JobCompletionRateResponse ToResponse(JobCompletionRateView v) => new(v.Year, v.Month, v.TotalJobs, v.CompletedJobs, v.CompletionRatePct);
    public static OverdueInvoiceSummaryResponse ToResponse(OverdueInvoiceSummaryView v) => new(v.TotalOverdueAmount ?? 0, v.OverdueInvoiceCount, v.AvgOverdueAmount ?? 0, v.MaxDaysOverdue ?? 0);
    public static InvoiceExportResponse ToResponse(InvoiceSummaryView v) => new(v.InvoiceId, v.InvoiceNumber, v.ClientId, v.ClientName, v.IssueDate, v.DueDate, v.SubTotal, v.DiscountAmount, v.VatAmount, v.TotalAmount, v.Status, v.AmountPaid, v.AmountOutstanding, v.IsOverdue, v.DaysOverdue, v.CreatedBy);
}
