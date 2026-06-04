using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Mapping;

public static class SopMapper
{
    public static SopTemplateResponse ToTemplateResponse(SopTemplate t) => new(
        t.Id,
        t.ServiceCatalogId,
        t.ServiceCatalog?.Name,
        t.Name,
        t.ServiceType,
        t.Description,
        t.IsActive,
        t.ChecklistItems.OrderBy(i => i.SortOrder).ThenBy(i => i.Id).Select(ToChecklistItemResponse).ToList());

    public static ChecklistItemResponse ToChecklistItemResponse(ChecklistItem i) => new(i.Id, i.SopTemplateId, i.ItemText, i.SortOrder, i.IsRequired);

    public static BookingSopAssignmentResponse ToAssignmentResponse(BookingSopAssignment a, int completedItems = 0, List<ChecklistResponseResponse>? checklistItems = null) => new(
        a.BookingId,
        a.SopTemplateId,
        a.SopTemplate.Name,
        a.CustomInstructions,
        a.SopTemplate.ChecklistItems.Count,
        completedItems,
        checklistItems ?? []);

    public static ChecklistResponseResponse ToChecklistResponseResponse(ChecklistItem item, ChecklistResponse? response) => new(
        item.Id,
        item.ItemText,
        item.IsRequired,
        response?.IsCompleted ?? false,
        response?.CompletedAt,
        response?.Notes);
}
