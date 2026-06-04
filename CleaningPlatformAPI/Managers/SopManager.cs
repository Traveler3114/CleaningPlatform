using Microsoft.Extensions.Localization;
using CleaningPlatformAPI;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Mapping;
using Microsoft.EntityFrameworkCore;

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

    public async Task<OperationResult<SopTemplateResponse>> GetTemplateByIdAsync(int id, CancellationToken ct = default)
    {
        var template = await _db.SopTemplates
            .Include(t => t.ServiceCatalog)
            .Include(t => t.ChecklistItems)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
        return template is null
            ? OperationResult<SopTemplateResponse>.Fail($"SOP template #{id} was not found.")
            : OperationResult<SopTemplateResponse>.Ok(SopMapper.ToTemplateResponse(template));
    }

    public async Task<OperationResult<SopTemplateResponse>> CreateTemplateAsync(CreateSopTemplateRequest dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return OperationResult<SopTemplateResponse>.Fail(_localizer["err_sop_name_required"]);

        var validServiceTypes = new[] { "Vehicle", "SiteBased", "Boat", "Generic" };
        if (!validServiceTypes.Contains(dto.ServiceType?.Trim()))
            return OperationResult<SopTemplateResponse>.Fail("ServiceType must be one of: Vehicle, SiteBased, Boat, Generic.");

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
        return OperationResult<SopTemplateResponse>.Ok((await GetTemplateByIdAsync(template.Id, ct)).Data!);
    }

    public async Task<OperationResult<SopTemplateResponse>> UpdateTemplateAsync(int id, CreateSopTemplateRequest dto, CancellationToken ct = default)
    {
        var template = await _db.SopTemplates.FindAsync([id], ct);
        if (template is null)
            return OperationResult<SopTemplateResponse>.Fail($"SOP template #{id} was not found.");

        var validServiceTypes = new[] { "Vehicle", "SiteBased", "Boat", "Generic" };
        if (!validServiceTypes.Contains(dto.ServiceType?.Trim()))
            return OperationResult<SopTemplateResponse>.Fail("ServiceType must be one of: Vehicle, SiteBased, Boat, Generic.");

        template.ServiceCatalogId = dto.ServiceCatalogId;
        template.Name = dto.Name.Trim();
        template.ServiceType = string.IsNullOrWhiteSpace(dto.ServiceType) ? "Generic" : dto.ServiceType.Trim();
        template.Description = dto.Description?.Trim();
        template.IsActive = dto.IsActive;
        template.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return OperationResult<SopTemplateResponse>.Ok((await GetTemplateByIdAsync(id, ct)).Data!);
    }

    public async Task<OperationResult<SopTemplateResponse>> ToggleActiveAsync(int id, bool isActive, CancellationToken ct = default)
    {
        var template = await _db.SopTemplates.FindAsync([id], ct);
        if (template is null)
            return OperationResult<SopTemplateResponse>.Fail($"SOP template #{id} was not found.");

        template.IsActive = isActive;
        template.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return OperationResult<SopTemplateResponse>.Ok((await GetTemplateByIdAsync(id, ct)).Data!);
    }

    public async Task<OperationResult<ChecklistItemResponse>> AddChecklistItemAsync(int templateId, UpsertChecklistItemRequest dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.ItemText))
            return OperationResult<ChecklistItemResponse>.Fail(_localizer["err_checklist_text_required"]);

        var templateExists = await _db.SopTemplates.AnyAsync(t => t.Id == templateId, ct);
        if (!templateExists)
            return OperationResult<ChecklistItemResponse>.Fail($"SOP template #{templateId} was not found.");

        var item = new ChecklistItem
        {
            SopTemplateId = templateId,
            ItemText = dto.ItemText.Trim(),
            SortOrder = dto.SortOrder,
            IsRequired = dto.IsRequired
        };
        _db.ChecklistItems.Add(item);
        await _db.SaveChangesAsync(ct);
        return OperationResult<ChecklistItemResponse>.Ok(SopMapper.ToChecklistItemResponse(item));
    }

    public async Task<OperationResult<ChecklistItemResponse>> UpdateChecklistItemAsync(int itemId, UpsertChecklistItemRequest dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.ItemText))
            return OperationResult<ChecklistItemResponse>.Fail(_localizer["err_checklist_text_required"]);

        var item = await _db.ChecklistItems.FindAsync([itemId], ct);
        if (item is null)
            return OperationResult<ChecklistItemResponse>.Fail($"Checklist item #{itemId} was not found.");

        item.ItemText = dto.ItemText.Trim();
        item.SortOrder = dto.SortOrder;
        item.IsRequired = dto.IsRequired;
        await _db.SaveChangesAsync(ct);
        return OperationResult<ChecklistItemResponse>.Ok(SopMapper.ToChecklistItemResponse(item));
    }

    public async Task<OperationResult<string>> DeleteChecklistItemAsync(int itemId, CancellationToken ct = default)
    {
        var item = await _db.ChecklistItems.FindAsync([itemId], ct);
        if (item is null)
            return OperationResult<string>.Fail($"Checklist item #{itemId} was not found.");

        var responses = await _db.ChecklistResponses
            .Where(r => r.ChecklistItemId == itemId)
            .ToListAsync(ct);

        if (responses.Count > 0)
            _db.ChecklistResponses.RemoveRange(responses);

        _db.ChecklistItems.Remove(item);
        await _db.SaveChangesAsync(ct);

        return OperationResult<string>.Ok("Checklist item deleted successfully.");
    }

    public async Task<List<SopTemplate>> GetDefaultTemplatesForServiceTypeAsync(string serviceType, CancellationToken ct = default) =>
        await _db.SopTemplates
            .Where(t => t.IsActive && (t.ServiceType == serviceType || t.ServiceType == "Generic"))
            .ToListAsync(ct);

    public async Task<OperationResult<BookingSopAssignmentResponse>> AssignSopToBookingAsync(int bookingId, AssignSopRequest dto, CancellationToken ct = default)
    {
        var bookingExists = await _db.Bookings.AnyAsync(b => b.Id == bookingId, ct);
        if (!bookingExists)
            return OperationResult<BookingSopAssignmentResponse>.Fail($"Booking #{bookingId} was not found.");

        var sopExists = await _db.SopTemplates.AnyAsync(t => t.Id == dto.SopTemplateId && t.IsActive, ct);
        if (!sopExists)
            return OperationResult<BookingSopAssignmentResponse>.Fail($"SOP template #{dto.SopTemplateId} was not found or is inactive.");

        var alreadyAssigned = await _db.BookingSopAssignments
            .AnyAsync(a => a.BookingId == bookingId && a.SopTemplateId == dto.SopTemplateId, ct);
        if (alreadyAssigned)
            return OperationResult<BookingSopAssignmentResponse>.Fail("This SOP template is already assigned to the booking.");

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

        return OperationResult<BookingSopAssignmentResponse>.Ok(SopMapper.ToAssignmentResponse(loaded));
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

    public async Task<OperationResult<ChecklistResponseResponse>> CompleteChecklistItemAsync(int bookingId, int sopTemplateId, int checklistItemId, CompleteChecklistItemRequest dto, CancellationToken ct = default)
    {
        var sopAssignmentExists = await _db.BookingSopAssignments
            .AnyAsync(a => a.BookingId == bookingId && a.SopTemplateId == sopTemplateId, ct);
        if (!sopAssignmentExists)
            return OperationResult<ChecklistResponseResponse>.Fail($"SOP assignment for booking #{bookingId}, template #{sopTemplateId} was not found.");

        var item = await _db.ChecklistItems.FindAsync([checklistItemId], ct);
        if (item is null)
            return OperationResult<ChecklistResponseResponse>.Fail($"Checklist item #{checklistItemId} was not found.");

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

        return OperationResult<ChecklistResponseResponse>.Ok(SopMapper.ToChecklistResponseResponse(item, response));
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