using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Contracts;

public record DashboardSummaryResponse(MonthlyRevenueView? MonthlyRevenue, TopClientView? TopClient, JobCompletionRateView? CompletionRate, OverdueInvoiceSummaryView OverdueInvoices);
