using Microsoft.Extensions.Localization;
using CleaningPlatformAPI;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Mapping;
using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Common;

namespace CleaningPlatformAPI.Managers;

public class SopManager
{
    private readonly AppDbContext _db;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public SopManager(AppDbContext db, IStringLocalizer<SharedResources> localizer) { _db = db; 
            _localizer = localizer;}

    public async Task<List<SopTemplateResponse>> GetAllTemplatesAsync(CancellationToken ct = default)
    {
        var templates = await _db.SopTemplates
            .Include(t => t.ServiceCatalog)
            .Include(t => t.ChecklistItems)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
        return templates.Select(SopMapper.ToTemplateResponse).ToList();
    }

    public async Task<SopTemplateResponse> GetTemplateByIdAsync(int id, CancellationToken ct = default)
    {
        var template = await _db.SopTemplates
            .Include(t => t.ServiceCatalog)
            .Include(t => t.ChecklistItems)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
        return template is null
            ? throw new AppException("SOP_NOT_FOUND", $"SOP template #{id} was not found.", 404)
            : SopMapper.ToTemplateResponse(template);
    }

    public async Task<SopTemplateResponse> CreateTemplateAsync(CreateSopTemplateRequest dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new AppException("SOP_NAME_REQUIRED", _localizer["err_sop_name_required"], 422);

        var validServiceTypes = new[] { "Vehicle", "SiteBased", "Boat", "Generic" };
        if (!validServiceTypes.Contains(dto.ServiceType?.Trim()))
            throw new AppException("INVALID_SERVICE_TYPE", "ServiceType must be one of: Vehicle, SiteBased, Boat, Generic.", 422);

        var now = DateTime.UtcNow;
        var template = new SopTemplate
        {
            ServiceCatalogId = dto.ServiceCatalogId,
            Name = dto.Name.Trim(),
            ServiceType = string.IsNullOrWhiteSpace(dto.ServiceType) ? "Generic" : dto.ServiceType.Trim(),
            Description = dto.Description?.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.SopTemplates.Add(template);
        await _db.SaveChangesAsync(ct);
        return await GetTemplateByIdAsync(template.Id, ct);
    }

    public async Task<SopTemplateResponse> UpdateTemplateAsync(int id, CreateSopTemplateRequest dto, CancellationToken ct = default)
    {
        var template = await _db.SopTemplates.FindAsync([id], ct);
        if (template is null)
            throw new AppException("SOP_NOT_FOUND", $"SOP template #{id} was not found.", 404);

        var validServiceTypes = new[] { "Vehicle", "SiteBased", "Boat", "Generic" };
        if (!validServiceTypes.Contains(dto.ServiceType?.Trim()))
            throw new AppException("INVALID_SERVICE_TYPE", "ServiceType must be one of: Vehicle, SiteBased, Boat, Generic.", 422);

        template.ServiceCatalogId = dto.ServiceCatalogId;
        template.Name = dto.Name.Trim();
        template.ServiceType = string.IsNullOrWhiteSpace(dto.ServiceType) ? "Generic" : dto.ServiceType.Trim();
        template.Description = dto.Description?.Trim();
        template.IsActive = dto.IsActive;
        template.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return await GetTemplateByIdAsync(id, ct);
    }

    public async Task<SopTemplateResponse> ToggleActiveAsync(int id, bool isActive, CancellationToken ct = default)
    {
        var template = await _db.SopTemplates.FindAsync([id], ct);
        if (template is null)
            throw new AppException("SOP_NOT_FOUND", $"SOP template #{id} was not found.", 404);

        template.IsActive = isActive;
        template.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return await GetTemplateByIdAsync(id, ct);
    }

    public async Task<ChecklistItemResponse> AddChecklistItemAsync(int templateId, UpsertChecklistItemRequest dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.ItemText))
            throw new AppException("CHECKLIST_TEXT_REQUIRED", _localizer["err_checklist_text_required"], 422);

        var templateExists = await _db.SopTemplates.AnyAsync(t => t.Id == templateId, ct);
        if (!templateExists)
            throw new AppException("SOP_NOT_FOUND", $"SOP template #{templateId} was not found.", 404);

        var item = new ChecklistItem
        {
            SopTemplateId = templateId,
            ItemText = dto.ItemText.Trim(),
            SortOrder = dto.SortOrder,
            IsRequired = dto.IsRequired
        };
        _db.ChecklistItems.Add(item);
        await _db.SaveChangesAsync(ct);
        return SopMapper.ToChecklistItemResponse(item);
    }

    public async Task<ChecklistItemResponse> UpdateChecklistItemAsync(int itemId, UpsertChecklistItemRequest dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.ItemText))
            throw new AppException("CHECKLIST_TEXT_REQUIRED", _localizer["err_checklist_text_required"], 422);

        var item = await _db.ChecklistItems.FindAsync([itemId], ct);
        if (item is null)
            throw new AppException("CHECKLIST_ITEM_NOT_FOUND", $"Checklist item #{itemId} was not found.", 404);

        item.ItemText = dto.ItemText.Trim();
        item.SortOrder = dto.SortOrder;
        item.IsRequired = dto.IsRequired;
        await _db.SaveChangesAsync(ct);
        return SopMapper.ToChecklistItemResponse(item);
    }

    public async Task DeleteChecklistItemAsync(int itemId, CancellationToken ct = default)
    {
        var item = await _db.ChecklistItems.FindAsync([itemId], ct);
        if (item is null)
            throw new AppException("CHECKLIST_ITEM_NOT_FOUND", $"Checklist item #{itemId} was not found.", 404);

        var responses = await _db.ChecklistResponses
            .Where(r => r.ChecklistItemId == itemId)
            .ToListAsync(ct);

        if (responses.Count > 0)
            _db.ChecklistResponses.RemoveRange(responses);

        _db.ChecklistItems.Remove(item);
        await _db.SaveChangesAsync(ct);

        return;
    }

    public async Task<List<SopTemplate>> GetDefaultTemplatesForServiceTypeAsync(string serviceType, CancellationToken ct = default) =>
        await _db.SopTemplates
            .Where(t => t.IsActive && (t.ServiceType == serviceType || t.ServiceType == "Generic"))
            .ToListAsync(ct);

    public async Task<BookingSopAssignmentResponse> AssignSopToBookingAsync(int bookingId, AssignSopRequest dto, CancellationToken ct = default)
    {
        var bookingExists = await _db.Bookings.AnyAsync(b => b.Id == bookingId, ct);
        if (!bookingExists)
            throw new AppException("BOOKING_NOT_FOUND", $"Booking #{bookingId} was not found.", 404);

        var sopExists = await _db.SopTemplates.AnyAsync(t => t.Id == dto.SopTemplateId && t.IsActive, ct);
        if (!sopExists)
            throw new AppException("SOP_NOT_FOUND_INACTIVE", $"SOP template #{dto.SopTemplateId} was not found or is inactive.", 404);

        var alreadyAssigned = await _db.BookingSopAssignments
            .AnyAsync(a => a.BookingId == bookingId && a.SopTemplateId == dto.SopTemplateId, ct);
        if (alreadyAssigned)
            throw new AppException("SOP_ALREADY_ASSIGNED", "This SOP template is already assigned to the booking.", 409);

        var assignment = new BookingSopAssignment
        {
            BookingId = bookingId,
            SopTemplateId = dto.SopTemplateId,
            CustomInstructions = dto.CustomInstructions?.Trim(),
            AssignedAt = DateTime.UtcNow
        };
        _db.BookingSopAssignments.Add(assignment);
        await _db.SaveChangesAsync(ct);

        var loaded = await _db.BookingSopAssignments
            .Include(a => a.SopTemplate)
            .ThenInclude(t => t.ChecklistItems)
            .FirstAsync(a => a.BookingId == bookingId && a.SopTemplateId == dto.SopTemplateId, ct);

        return SopMapper.ToAssignmentResponse(loaded);
    }

    public async Task<List<BookingSopAssignmentResponse>> GetBookingSopsAsync(int bookingId, CancellationToken ct = default)
    {
        var assignments = await _db.BookingSopAssignments
            .Include(a => a.SopTemplate)
            .ThenInclude(t => t.ChecklistItems)
            .Where(a => a.BookingId == bookingId)
            .ToListAsync(ct);

        var responses = await _db.ChecklistResponses
            .Where(r => r.BookingId == bookingId)
            .ToListAsync(ct);

        return assignments.Select(a =>
        {
            var assignmentResponses = responses
                .Where(r => r.SopTemplateId == a.SopTemplateId)
                .ToList();

            var completedCount = assignmentResponses.Count(r => r.IsCompleted);

            var items = a.SopTemplate.ChecklistItems
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Id)
                .Select(i =>
                {
                    var response = assignmentResponses.FirstOrDefault(r => r.ChecklistItemId == i.Id);
                    return SopMapper.ToChecklistResponseResponse(i, response);
                })
                .ToList();

            return SopMapper.ToAssignmentResponse(a, completedCount, items);
        }).ToList();
    }

    public async Task<List<ChecklistResponseResponse>> GetChecklistForSopAssignmentAsync(int bookingId, int sopTemplateId, CancellationToken ct = default)
    {
        var items = await _db.ChecklistItems
            .Where(i => i.SopTemplateId == sopTemplateId)
            .OrderBy(i => i.SortOrder)
            .ToListAsync(ct);

        var responses = await _db.ChecklistResponses
            .Where(r => r.BookingId == bookingId && r.SopTemplateId == sopTemplateId)
            .ToDictionaryAsync(r => r.ChecklistItemId, ct);

        return items.Select(i =>
            SopMapper.ToChecklistResponseResponse(i, responses.TryGetValue(i.Id, out var r) ? r : null))
            .ToList();
    }

    public async Task<ChecklistResponseResponse> CompleteChecklistItemAsync(int bookingId, int sopTemplateId, int checklistItemId, CompleteChecklistItemRequest dto, CancellationToken ct = default)
    {
        var sopAssignmentExists = await _db.BookingSopAssignments
            .AnyAsync(a => a.BookingId == bookingId && a.SopTemplateId == sopTemplateId, ct);
        if (!sopAssignmentExists)
            throw new AppException("SOP_ASSIGNMENT_NOT_FOUND", $"SOP assignment for booking #{bookingId}, template #{sopTemplateId} was not found.", 404);

        var item = await _db.ChecklistItems.FindAsync([checklistItemId], ct);
        if (item is null)
            throw new AppException("CHECKLIST_ITEM_NOT_FOUND", $"Checklist item #{checklistItemId} was not found.", 404);

        var response = await _db.ChecklistResponses
            .FirstOrDefaultAsync(r => r.BookingId == bookingId && r.SopTemplateId == sopTemplateId && r.ChecklistItemId == checklistItemId, ct);

        if (response is null)
        {
            response = new ChecklistResponse
            {
                BookingId = bookingId,
                SopTemplateId = sopTemplateId,
                ChecklistItemId = checklistItemId
            };
            _db.ChecklistResponses.Add(response);
        }

        response.IsCompleted = dto.IsCompleted;
        response.CompletedAt = dto.IsCompleted ? DateTime.UtcNow : null;
        response.Notes = dto.Notes?.Trim();
        await _db.SaveChangesAsync(ct);

        return SopMapper.ToChecklistResponseResponse(item, response);
    }

    public async Task EnsureServiceSopsAssignedAsync(int bookingId, CancellationToken ct = default)
    {
        var serviceCatalogIds = await _db.BookingServices
            .Where(bs => bs.BookingId == bookingId)
            .Select(bs => bs.ServiceCatalogId)
            .ToListAsync(ct);

        if (!serviceCatalogIds.Any()) return;

        var linkedTemplateIds = await _db.SopTemplates
            .Where(t => t.IsActive
                     && t.ServiceCatalogId != null
                     && serviceCatalogIds.Contains(t.ServiceCatalogId.Value))
            .Select(t => t.Id)
            .ToListAsync(ct);

        var alreadyAssignedIds = await _db.BookingSopAssignments
            .Where(a => a.BookingId == bookingId)
            .Select(a => a.SopTemplateId)
            .ToListAsync(ct);

        var toAssign = linkedTemplateIds.Except(alreadyAssignedIds).ToList();
        if (!toAssign.Any()) return;

        foreach (var templateId in toAssign)
        {
            _db.BookingSopAssignments.Add(new BookingSopAssignment
            {
                BookingId = bookingId,
                SopTemplateId = templateId,
                AssignedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync(ct);
    }

}