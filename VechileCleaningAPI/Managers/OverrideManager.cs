using Microsoft.EntityFrameworkCore;
using VechileCleaningAPI.Data;
using VechileCleaningAPI.Dtos;
using VechileCleaningAPI.Entities;
using VechileCleaningAPI.Common;

namespace VechileCleaningAPI.Managers;

public class OverrideManager
{
    private readonly AppDbContext _db;

    public OverrideManager(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<SlotOverrideDto>> GetOverridesAsync()
    {
        var cutoff = DateTime.Today;
        var overrides = await _db.SlotOverrides.Where(o => o.Date >= cutoff).ToListAsync();
        return overrides.Select(MapToDto).ToList();
    }

    public async Task<OperationResult<SlotOverrideDto>> CreateOverrideAsync(SlotOverrideDto dto)
    {
        var entity = new SlotOverride
        {
            Date = dto.Date.Date,
            Hour = dto.Hour,
            IsClosed = dto.IsClosed,
            Capacity = dto.Capacity
        };
        _db.SlotOverrides.Add(entity);
        await _db.SaveChangesAsync();
        return OperationResult<SlotOverrideDto>.Ok(MapToDto(entity));
    }

    public async Task<OperationResult<bool>> DeleteOverrideAsync(int id)
    {
        var entity = await _db.SlotOverrides.FindAsync(id);
        if (entity == null)
            return OperationResult<bool>.Fail("Override not found.");
        _db.SlotOverrides.Remove(entity);
        await _db.SaveChangesAsync();
        // Nightly cleanup: delete overrides older than 30 days
        var cutoff = DateTime.Today.AddDays(-30);
        var old = _db.SlotOverrides.Where(o => o.Date < cutoff);
        _db.SlotOverrides.RemoveRange(old);
        await _db.SaveChangesAsync();
        return OperationResult<bool>.Ok(true);
    }

    private static SlotOverrideDto MapToDto(SlotOverride o) => new()
    {
        Date = o.Date,
        Hour = o.Hour,
        IsClosed = o.IsClosed,
        Capacity = o.Capacity
    };
}
