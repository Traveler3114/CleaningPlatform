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

    public async Task<List<HourOverrideDto>> GetOverridesAsync()
    {
        var cutoff = DateTime.Today;
        var overrides = await _db.HourOverrides.Where(o => o.Date >= cutoff).ToListAsync();
        return overrides.Select(MapToDto).ToList();
    }

    public async Task<OperationResult<HourOverrideDto>> CreateOverrideAsync(HourOverrideDto dto)
    {
        // Check if an override already exists for this date and hour
        var existingOverride = await _db.HourOverrides
            .FirstOrDefaultAsync(o => o.Date.Date == dto.Date.Date && o.Hour == dto.Hour);

        HourOverride entity;

        if (existingOverride != null)
        {
            // Update existing override
            existingOverride.Capacity = dto.Capacity;
            entity = existingOverride;
        }
        else
        {
            // Create new override
            entity = new HourOverride
            {
                Date = dto.Date.Date,
                Hour = dto.Hour,
                Capacity = dto.Capacity
            };
            _db.HourOverrides.Add(entity);
        }

        await _db.SaveChangesAsync();
        return OperationResult<HourOverrideDto>.Ok(MapToDto(entity));
    }

    public async Task<OperationResult<bool>> DeleteOverrideAsync(int id)
    {
        var entity = await _db.HourOverrides.FindAsync(id);
        if (entity == null)
            return OperationResult<bool>.Fail("Override not found.");

        _db.HourOverrides.Remove(entity);
        await _db.SaveChangesAsync();
        return OperationResult<bool>.Ok(true);
    }

    // Call this separately (e.g., from a background job or admin endpoint)
    public async Task CleanupOldOverridesAsync()
    {
        var cutoff = DateTime.Today.AddDays(-30);
        var old = await _db.HourOverrides.Where(o => o.Date < cutoff).ToListAsync();
        if (old.Any())
        {
            _db.HourOverrides.RemoveRange(old);
            await _db.SaveChangesAsync();
        }
    }

    private static HourOverrideDto MapToDto(HourOverride o) => new()
    {
        Date = o.Date,
        Hour = o.Hour,
        Capacity = o.Capacity
    };
}
