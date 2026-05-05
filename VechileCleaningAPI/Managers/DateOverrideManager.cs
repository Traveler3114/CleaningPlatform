using Microsoft.EntityFrameworkCore;
using VechileCleaningAPI.Data;
using VechileCleaningAPI.Dtos;
using VechileCleaningAPI.Entities;
using VechileCleaningAPI.Common;

namespace VechileCleaningAPI.Managers;

public class DateOverrideManager
{
    private readonly AppDbContext _db;

    public DateOverrideManager(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<DateOverrideDto>> GetOverridesAsync()
    {
        var cutoff = DateTime.Today;
        var overrides = await _db.DateOverrides.Where(o => o.Date >= cutoff).OrderBy(o => o.Date).ToListAsync();
        return overrides.Select(MapToDto).ToList();
    }

    public async Task<OperationResult<DateOverrideDto>> CreateOverrideAsync(DateOverrideDto dto)
    {
        var existing = await _db.DateOverrides.FirstOrDefaultAsync(o => o.Date.Date == dto.Date.Date);

        DateOverride entity;

        if (existing != null)
        {
            existing.StartHour = dto.StartHour;
            existing.EndHour = dto.EndHour;
            existing.Capacity = dto.Capacity;
            existing.IsFullyClosed = dto.IsFullyClosed;
            entity = existing;
        }
        else
        {
            entity = new DateOverride
            {
                Date = dto.Date.Date,
                StartHour = dto.StartHour,
                EndHour = dto.EndHour,
                Capacity = dto.Capacity,
                IsFullyClosed = dto.IsFullyClosed,
            };
            _db.DateOverrides.Add(entity);
        }

        await _db.SaveChangesAsync();
        return OperationResult<DateOverrideDto>.Ok(MapToDto(entity));
    }

    public async Task<OperationResult<bool>> DeleteOverrideAsync(int id)
    {
        var entity = await _db.DateOverrides.FindAsync(id);
        if (entity == null)
            return OperationResult<bool>.Fail("Override not found.");

        _db.DateOverrides.Remove(entity);
        await _db.SaveChangesAsync();
        return OperationResult<bool>.Ok(true);
    }

    private static DateOverrideDto MapToDto(DateOverride o) => new()
    {
        Id = o.Id,
        Date = o.Date,
        StartHour = o.StartHour,
        EndHour = o.EndHour,
        Capacity = o.Capacity,
        IsFullyClosed = o.IsFullyClosed,
    };
}
