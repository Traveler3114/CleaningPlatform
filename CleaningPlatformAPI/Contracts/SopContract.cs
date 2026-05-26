namespace CleaningPlatformAPI.Contracts;

public record SopTemplateResponse(int Id, int? ServiceCatalogId, string? ServiceCatalogName, string Name, string ServiceType, string? Description, bool IsActive, List<ChecklistItemResponse> ChecklistItems);
public record ChecklistItemResponse(int Id, int SopTemplateId, string ItemText, int SortOrder, bool IsRequired);
public record BookingSopAssignmentResponse(int Id, int BookingId, int SopTemplateId, string SopName, string? CustomInstructions, int TotalItems, int CompletedItems, List<ChecklistResponseResponse> ChecklistItems);
public record ChecklistResponseResponse(int Id, int ChecklistItemId, string ItemText, bool IsRequired, bool IsCompleted, DateTime? CompletedAt, string? Notes);
public record CreateSopTemplateRequest
{
    public int? ServiceCatalogId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ServiceType { get; set; } = "Generic";
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public record UpsertChecklistItemRequest
{
    public string ItemText { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsRequired { get; set; } = true;
}

public record AssignSopRequest
{
    public int SopTemplateId { get; set; }
    public string? CustomInstructions { get; set; }
}

public record CompleteChecklistItemRequest
{
    public bool IsCompleted { get; set; }
    public string? Notes { get; set; }
}

public record ToggleSopActiveRequest
{
    public bool IsActive { get; set; }
}
