using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Mapping;

namespace CleaningPlatformAPI.Managers;

public class SopManager
{
    private readonly AppDbContext _db;

    public SopManager(AppDbContext db) => _db = db;

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
            return OperationResult<SopTemplateResponse>.Fail("SOP name is required.");

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

        template.ServiceCatalogId = dto.ServiceCatalogId;
        template.Name = dto.Name.Trim();
        template.ServiceType = string.IsNullOrWhiteSpace(dto.ServiceType) ? "Generic" : dto.ServiceType.Trim();
        template.Description = dto.Description?.Trim();
        template.IsActive = dto.IsActive;
        template.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return OperationResult<SopTemplateResponse>.Ok((await GetTemplateByIdAsync(id, ct)).Data!);
    }

    public async Task<OperationResult<string>> DeleteTemplateAsync(int id, CancellationToken ct = default)
    {
        var template = await _db.SopTemplates
            .Include(t => t.ChecklistItems)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (template is null)
            return OperationResult<string>.Fail($"SOP template #{id} was not found.");

        var hasAssignments = await _db.BookingSopAssignments.AnyAsync(a => a.SopTemplateId == id, ct);

        if (hasAssignments)
        {
            template.IsActive = false;
            template.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return OperationResult<string>.Ok("SOP template deactivated — it is linked to existing bookings and cannot be fully removed.");
        }

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var itemIds = template.ChecklistItems.Select(i => i.Id).ToList();
            if (itemIds.Count > 0)
            {
                var responses = await _db.ChecklistResponses
                    .Where(r => itemIds.Contains(r.ChecklistItemId))
                    .ToListAsync(ct);
                _db.ChecklistResponses.RemoveRange(responses);
            }

            _db.ChecklistItems.RemoveRange(template.ChecklistItems);
            _db.SopTemplates.Remove(template);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        return OperationResult<string>.Ok("SOP template deleted successfully.");
    }

    public async Task<OperationResult<ChecklistItemResponse>> AddChecklistItemAsync(int templateId, UpsertChecklistItemRequest dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.ItemText))
            return OperationResult<ChecklistItemResponse>.Fail("Checklist item text is required.");

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
            return OperationResult<ChecklistItemResponse>.Fail("Checklist item text is required.");

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

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var responses = await _db.ChecklistResponses
                .Where(r => r.ChecklistItemId == itemId)
                .ToListAsync(ct);

            if (responses.Count > 0)
                _db.ChecklistResponses.RemoveRange(responses);

            _db.ChecklistItems.Remove(item);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

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
            .FirstAsync(a => a.Id == assignment.Id, ct);

        return OperationResult<BookingSopAssignmentResponse>.Ok(SopMapper.ToAssignmentResponse(loaded));
    }

    public async Task<List<BookingSopAssignmentResponse>> GetBookingSopsAsync(int bookingId, CancellationToken ct = default)
    {
        var assignments = await _db.BookingSopAssignments
            .Include(a => a.SopTemplate)
            .ThenInclude(t => t.ChecklistItems)
            .Where(a => a.BookingId == bookingId)
            .ToListAsync(ct);

        var bookingAssignmentIds = await _db.BookingAssignments
            .Where(a => a.BookingId == bookingId)
            .Select(a => a.Id)
            .ToListAsync(ct);

        var responses = await _db.ChecklistResponses
            .Where(r => bookingAssignmentIds.Contains(r.BookingAssignmentId))
            .Include(r => r.ChecklistItem)
            .ToListAsync(ct);

        var completed = responses
            .Where(r => r.IsCompleted)
            .GroupBy(r => r.ChecklistItem.SopTemplateId)
            .ToDictionary(g => g.Key, g => g.Count());

        var primaryAssignmentId = bookingAssignmentIds.OrderBy(id => id).FirstOrDefault();
        var responsesByItem = primaryAssignmentId == 0
            ? new Dictionary<int, ChecklistResponse>()
            : responses
                .Where(r => r.BookingAssignmentId == primaryAssignmentId)
                .GroupBy(r => r.ChecklistItemId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.CompletedAt).First());

        return assignments.Select(a =>
        {
            var items = a.SopTemplate.ChecklistItems
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Id)
                .Select(i =>
                {
                    responsesByItem.TryGetValue(i.Id, out var response);
                    return SopMapper.ToChecklistResponseResponse(i, response);
                })
                .ToList();

            completed.TryGetValue(a.SopTemplateId, out var count);
            return SopMapper.ToAssignmentResponse(a, count, items);
        }).ToList();
    }

    public async Task<List<ChecklistResponseResponse>> GetChecklistForAssignmentAsync(int bookingAssignmentId, CancellationToken ct = default)
    {
        var assignment = await _db.BookingAssignments
            .Include(a => a.Booking)
            .FirstOrDefaultAsync(a => a.Id == bookingAssignmentId, ct);
        if (assignment is null) return [];

        var sopIds = await _db.BookingSopAssignments
            .Where(a => a.BookingId == assignment.BookingId)
            .Select(a => a.SopTemplateId)
            .ToListAsync(ct);

        var items = await _db.ChecklistItems
            .Where(i => sopIds.Contains(i.SopTemplateId))
            .OrderBy(i => i.SortOrder)
            .ToListAsync(ct);

        var responses = await _db.ChecklistResponses
            .Where(r => r.BookingAssignmentId == bookingAssignmentId)
            .ToDictionaryAsync(r => r.ChecklistItemId, ct);

        return items.Select(i =>
            SopMapper.ToChecklistResponseResponse(i, responses.TryGetValue(i.Id, out var r) ? r : null))
            .ToList();
    }

    public async Task<OperationResult<ChecklistResponseResponse>> CompleteChecklistItemAsync(int bookingAssignmentId, int checklistItemId, CompleteChecklistItemRequest dto, CancellationToken ct = default)
    {
        var assignmentExists = await _db.BookingAssignments.AnyAsync(a => a.Id == bookingAssignmentId, ct);
        if (!assignmentExists)
            return OperationResult<ChecklistResponseResponse>.Fail($"Booking assignment #{bookingAssignmentId} was not found.");

        var item = await _db.ChecklistItems.FindAsync([checklistItemId], ct);
        if (item is null)
            return OperationResult<ChecklistResponseResponse>.Fail($"Checklist item #{checklistItemId} was not found.");

        var response = await _db.ChecklistResponses
            .FirstOrDefaultAsync(r => r.BookingAssignmentId == bookingAssignmentId && r.ChecklistItemId == checklistItemId, ct);

        if (response is null)
        {
            response = new ChecklistResponse
            {
                BookingAssignmentId = bookingAssignmentId,
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