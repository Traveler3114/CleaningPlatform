namespace CleaningPlatformAPI.Common;

/// <summary>
/// Wraps a paged list of items with metadata the client needs to render pagination controls.
/// </summary>
public class PagedResult<T>
{
    /// <summary>The items for the current page.</summary>
    public List<T> Items { get; init; } = [];

    /// <summary>Total number of items across all pages (before paging).</summary>
    public int TotalCount { get; init; }

    /// <summary>Current page number, 1-based.</summary>
    public int Page { get; init; }

    /// <summary>Number of items per page.</summary>
    public int PageSize { get; init; }

    /// <summary>Total number of pages.</summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>Whether there is a page before this one.</summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>Whether there is a page after this one.</summary>
    public bool HasNextPage => Page < TotalPages;

    public static PagedResult<T> From(List<T> items, int totalCount, int page, int pageSize) => new()
    {
        Items = items,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize
    };
}

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
