namespace CleaningPlatformAPI.Contracts;

public class PortalDashboardResponse
{
    public int UpcomingBookings { get; set; }
    public int CompletedBookings { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal OutstandingAmount { get; set; }
    public List<PortalBookingSummary> UpcomingBookingList { get; set; } = new();
    public List<PortalInvoiceSummary> RecentInvoices { get; set; } = new();
}

public class PortalBookingSummary
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int Hour { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? SiteName { get; set; }
    public string Services { get; set; } = string.Empty;
    public decimal EstimatedTotal { get; set; }
}

public class PortalBookingDetailResponse
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int Hour { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? SiteName { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PortalBookingService> Services { get; set; } = new();
    public List<PortalAssignedEmployee> AssignedEmployees { get; set; } = new();
}

public class PortalBookingService
{
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal? EstimatedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
}

public class PortalAssignedEmployee
{
    public string FullName { get; set; } = string.Empty;
}

public class PortalInvoiceSummary
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

public class PortalInvoiceDetailResponse
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal VatPct { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public List<PortalInvoiceLine> Lines { get; set; } = new();
    public List<PortalPayment> Payments { get; set; } = new();
}

public class PortalInvoiceLine
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class PortalPayment
{
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string? Reference { get; set; }
}

public class PortalProfileResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public DateTime Since { get; set; }
    public string AvatarInitial { get; set; } = string.Empty;
    public List<PortalSite> Sites { get; set; } = new();
}

public class PortalSite
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? SiteType { get; set; }
    public string? AccessNotes { get; set; }
}
