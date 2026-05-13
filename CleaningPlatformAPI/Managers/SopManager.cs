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
        var templates = await _db.SopTemplates.Include(t => t.ServiceCatalog).Include(t => t.ChecklistItems).OrderBy(t => t.Name).ToListAsync(ct);
        return templates.Select(SopMapper.ToTemplateResponse).ToList();
    }

    public async Task<SopTemplateResponse?> GetTemplateByIdAsync(int id, CancellationToken ct = default)
    {
        var template = await _db.SopTemplates.Include(t => t.ServiceCatalog).Include(t => t.ChecklistItems).FirstOrDefaultAsync(t => t.Id == id, ct);
        return template is null ? null : SopMapper.ToTemplateResponse(template);
    }

    public async Task<OperationResult<SopTemplateResponse>> CreateTemplateAsync(CreateSopTemplateRequest dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return OperationResult<SopTemplateResponse>.Fail("SOP name is required.");
        var now = DateTime.UtcNow;
        var template = new SopTemplate { ServiceCatalogId = dto.ServiceCatalogId, Name = dto.Name.Trim(), ServiceType = string.IsNullOrWhiteSpace(dto.ServiceType) ? "Generic" : dto.ServiceType.Trim(), Description = dto.Description?.Trim(), IsActive = dto.IsActive, CreatedAt = now, UpdatedAt = now };
        _db.SopTemplates.Add(template);
        await _db.SaveChangesAsync(ct);
        return OperationResult<SopTemplateResponse>.Ok((await GetTemplateByIdAsync(template.Id, ct))!);
    }

    public async Task<OperationResult<SopTemplateResponse>> UpdateTemplateAsync(int id, CreateSopTemplateRequest dto, CancellationToken ct = default)
    {
        var template = await _db.SopTemplates.FindAsync([id], ct);
        if (template is null) return OperationResult<SopTemplateResponse>.Fail("SOP template not found.");
        template.ServiceCatalogId = dto.ServiceCatalogId;
        template.Name = dto.Name.Trim();
        template.ServiceType = string.IsNullOrWhiteSpace(dto.ServiceType) ? "Generic" : dto.ServiceType.Trim();
        template.Description = dto.Description?.Trim();
        template.IsActive = dto.IsActive;
        template.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return OperationResult<SopTemplateResponse>.Ok((await GetTemplateByIdAsync(id, ct))!);
    }

    public async Task<OperationResult<string>> DeleteTemplateAsync(int id, CancellationToken ct = default)
    {
        var template = await _db.SopTemplates.FindAsync([id], ct);
        if (template is null) return OperationResult<string>.Fail("SOP template not found.");
        var hasAssignments = await _db.BookingSopAssignments.AnyAsync(a => a.SopTemplateId == id, ct);
        if (hasAssignments) template.IsActive = false;
        else _db.SopTemplates.Remove(template);
        await _db.SaveChangesAsync(ct);
        return OperationResult<string>.Ok(hasAssignments ? "SOP template deactivated." : "SOP template deleted.");
    }

    public async Task<OperationResult<ChecklistItemResponse>> AddChecklistItemAsync(int templateId, UpsertChecklistItemRequest dto, CancellationToken ct = default)
    {
        if (!await _db.SopTemplates.AnyAsync(t => t.Id == templateId, ct)) return OperationResult<ChecklistItemResponse>.Fail("SOP template not found.");
        var item = new ChecklistItem { SopTemplateId = templateId, ItemText = dto.ItemText.Trim(), SortOrder = dto.SortOrder, IsRequired = dto.IsRequired };
        _db.ChecklistItems.Add(item);
        await _db.SaveChangesAsync(ct);
        return OperationResult<ChecklistItemResponse>.Ok(SopMapper.ToChecklistItemResponse(item));
    }

    public async Task<OperationResult<ChecklistItemResponse>> UpdateChecklistItemAsync(int itemId, UpsertChecklistItemRequest dto, CancellationToken ct = default)
    {
        var item = await _db.ChecklistItems.FindAsync([itemId], ct);
        if (item is null) return OperationResult<ChecklistItemResponse>.Fail("Checklist item not found.");
        item.ItemText = dto.ItemText.Trim();
        item.SortOrder = dto.SortOrder;
        item.IsRequired = dto.IsRequired;
        await _db.SaveChangesAsync(ct);
        return OperationResult<ChecklistItemResponse>.Ok(SopMapper.ToChecklistItemResponse(item));
    }

    public async Task<OperationResult<string>> DeleteChecklistItemAsync(int itemId, CancellationToken ct = default)
    {
        var item = await _db.ChecklistItems.FindAsync([itemId], ct);
        if (item is null) return OperationResult<string>.Fail("Checklist item not found.");
        _db.ChecklistItems.Remove(item);
        await _db.SaveChangesAsync(ct);
        return OperationResult<string>.Ok("Checklist item deleted.");
    }

    public async Task<List<SopTemplate>> GetDefaultTemplatesForServiceTypeAsync(string serviceType, CancellationToken ct = default) =>
        await _db.SopTemplates.Where(t => t.IsActive && (t.ServiceType == serviceType || t.ServiceType == "Generic")).ToListAsync(ct);

    public async Task<OperationResult<BookingSopAssignmentResponse>> AssignSopToBookingAsync(int bookingId, AssignSopRequest dto, CancellationToken ct = default)
    {
        var bookingExists = await _db.Bookings.AnyAsync(b => b.Id == bookingId, ct);
        if (!bookingExists) return OperationResult<BookingSopAssignmentResponse>.Fail("Booking not found.");
        var exists = await _db.BookingSopAssignments.AnyAsync(a => a.BookingId == bookingId && a.SopTemplateId == dto.SopTemplateId, ct);
        if (exists) return OperationResult<BookingSopAssignmentResponse>.Fail("SOP is already assigned to this booking.");
        var assignment = new BookingSopAssignment { BookingId = bookingId, SopTemplateId = dto.SopTemplateId, CustomInstructions = dto.CustomInstructions?.Trim(), AssignedAt = DateTime.UtcNow };
        _db.BookingSopAssignments.Add(assignment);
        await _db.SaveChangesAsync(ct);
        var loaded = await _db.BookingSopAssignments.Include(a => a.SopTemplate).ThenInclude(t => t.ChecklistItems).FirstAsync(a => a.Id == assignment.Id, ct);
        return OperationResult<BookingSopAssignmentResponse>.Ok(SopMapper.ToAssignmentResponse(loaded));
    }

    public async Task<List<BookingSopAssignmentResponse>> GetBookingSopsAsync(int bookingId, CancellationToken ct = default)
    {
        var assignments = await _db.BookingSopAssignments.Include(a => a.SopTemplate).ThenInclude(t => t.ChecklistItems).Where(a => a.BookingId == bookingId).ToListAsync(ct);
        var bookingAssignmentIds = await _db.BookingAssignments.Where(a => a.BookingId == bookingId).Select(a => a.Id).ToListAsync(ct);
        var completed = await _db.ChecklistResponses.Where(r => bookingAssignmentIds.Contains(r.BookingAssignmentId) && r.IsCompleted).GroupBy(r => r.ChecklistItem.SopTemplateId).Select(g => new { SopTemplateId = g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.SopTemplateId, x => x.Count, ct);
        return assignments.Select(a => SopMapper.ToAssignmentResponse(a, completed.TryGetValue(a.SopTemplateId, out var count) ? count : 0)).ToList();
    }

    public async Task<List<ChecklistResponseResponse>> GetChecklistForAssignmentAsync(int bookingAssignmentId, CancellationToken ct = default)
    {
        var assignment = await _db.BookingAssignments.Include(a => a.Booking).FirstOrDefaultAsync(a => a.Id == bookingAssignmentId, ct);
        if (assignment is null) return [];
        var sopIds = await _db.BookingSopAssignments.Where(a => a.BookingId == assignment.BookingId).Select(a => a.SopTemplateId).ToListAsync(ct);
        var items = await _db.ChecklistItems.Where(i => sopIds.Contains(i.SopTemplateId)).OrderBy(i => i.SortOrder).ToListAsync(ct);
        var responses = await _db.ChecklistResponses.Where(r => r.BookingAssignmentId == bookingAssignmentId).ToDictionaryAsync(r => r.ChecklistItemId, ct);
        return items.Select(i => SopMapper.ToChecklistResponseResponse(i, responses.TryGetValue(i.Id, out var response) ? response : null)).ToList();
    }

    public async Task<OperationResult<ChecklistResponseResponse>> CompleteChecklistItemAsync(int bookingAssignmentId, int checklistItemId, CompleteChecklistItemRequest dto, CancellationToken ct = default)
    {
        var item = await _db.ChecklistItems.FindAsync([checklistItemId], ct);
        if (item is null) return OperationResult<ChecklistResponseResponse>.Fail("Checklist item not found.");
        var response = await _db.ChecklistResponses.FirstOrDefaultAsync(r => r.BookingAssignmentId == bookingAssignmentId && r.ChecklistItemId == checklistItemId, ct);
        if (response is null)
        {
            response = new ChecklistResponse { BookingAssignmentId = bookingAssignmentId, ChecklistItemId = checklistItemId };
            _db.ChecklistResponses.Add(response);
        }
        response.IsCompleted = dto.IsCompleted;
        response.CompletedAt = dto.IsCompleted ? DateTime.UtcNow : null;
        response.Notes = dto.Notes?.Trim();
        await _db.SaveChangesAsync(ct);
        return OperationResult<ChecklistResponseResponse>.Ok(SopMapper.ToChecklistResponseResponse(item, response));
    }
}
