namespace CleaningPlatformAPI.Models;

/// <summary>
/// Standard query parameters for any paginated endpoint.
/// Bind with [FromQuery] in controller actions.
///
/// Example URL: /api/bookings?page=2&pageSize=25&search=smith
/// </summary>
public class PaginationParams
{
    private const int MaxPageSize = 200;
    private const int DefaultPageSize = 50;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    /// <summary>1-based page number. Defaults to 1.</summary>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>Items per page. Capped at 200, defaults to 50.</summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? DefaultPageSize : value > MaxPageSize ? MaxPageSize : value;
    }

    /// <summary>Optional free-text search term. Managers decide what fields it applies to.</summary>
    public string? Search { get; set; }

    // Convenience helpers used inside managers
    public int Skip => (Page - 1) * PageSize;
    public int Take => PageSize;
}
