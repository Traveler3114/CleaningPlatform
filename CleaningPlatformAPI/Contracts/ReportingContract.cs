namespace CleaningPlatformAPI.Contracts;

public record MonthlyRevenueResponse(int Year, int Month, int InvoiceCount, decimal TotalRevenue, decimal TotalVat, decimal TotalDiscount);
public record TopClientResponse(int ClientId, string ClientName, int InvoiceCount, decimal TotalBilled, decimal TotalPaid);
public record EmployeeUtilizationResponse(int EmployeeId, string EmployeeName, int JobsAssigned, int JobsCompleted, int DaysActive, decimal CompletionRatePct);
public record JobCompletionRateResponse(int Year, int Month, int TotalJobs, int CompletedJobs, double? CompletionRatePct);
public record OverdueInvoiceSummaryResponse(decimal TotalOverdueAmount, int OverdueInvoiceCount, decimal AvgOverdueAmount, int MaxDaysOverdue);
public record InvoiceExportResponse(int InvoiceId, string InvoiceNumber, int ClientId, string ClientName, DateTime IssueDate, DateTime DueDate, decimal SubTotal, decimal DiscountAmount, decimal VatAmount, decimal TotalAmount, string Status, decimal AmountPaid, decimal AmountOutstanding, bool IsOverdue, int DaysOverdue, string? CreatedBy);
public record DashboardSummaryResponse(MonthlyRevenueResponse? MonthlyRevenue, TopClientResponse? TopClient, JobCompletionRateResponse? CompletionRate, OverdueInvoiceSummaryResponse OverdueInvoices);
