namespace CleaningPlatformAPI.Contracts;

public class PortalDashboardResponse
{
    public int UpcomingBookings { get; set; }
    public int CompletedBookings { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal OutstandingAmount { get; set; }
    public List<BookingResponse> UpcomingBookingList { get; set; } = new();
    public List<InvoiceResponse> RecentInvoices { get; set; } = new();
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
