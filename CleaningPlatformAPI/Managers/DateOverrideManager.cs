using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;
using Microsoft.Extensions.Localization;
using CleaningPlatformAPI;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Mapping;

namespace CleaningPlatformAPI.Managers;

public class DateOverrideManager
{
    private readonly AppDbContext _db;
    private readonly IStringLocalizer<SharedResources> _localizer;
    public DateOverrideManager(AppDbContext db, IStringLocalizer<SharedResources> localizer) { _db = db; 
            _localizer = localizer;}

    public async Task<List<DateOverrideResponse>> GetOverridesAsync(CancellationToken ct = default)
    {
        var cutoff    = DateOnly.FromDateTime(DateTime.UtcNow);
        var overrides = await _db.DateOverrides
            .Where(o => o.Date >= cutoff)
            .OrderBy(o => o.Date)
            .ToListAsync(ct);
        return overrides.Select(ScheduleMapper.ToDateOverrideResponse).ToList();
    }

    public async Task<OperationResult<DateOverrideResponse>> CreateOverrideAsync(DateOverrideRequest request, CancellationToken ct = default)
    {
        if (request.Date < DateOnly.FromDateTime(DateTime.UtcNow))
            return OperationResult<DateOverrideResponse>.Fail("DATE_OVERRIDE_PAST",
                $"Date overrides cannot be created for past dates. The selected date was {request.Date:dd MMM yyyy}.");

        var existing = await _db.DateOverrides
            .FirstOrDefaultAsync(o => o.Date == request.Date, ct);

        DateOverride entity;

        if (existing is not null)
        {
            existing.StartHour     = request.StartHour;
            existing.EndHour       = request.EndHour;
            existing.Capacity      = request.Capacity;
            existing.IsFullyClosed = request.IsFullyClosed;
            entity = existing;
        }
        else
        {
            entity = new DateOverride
            {
                Date          = request.Date,
                StartHour     = request.StartHour,
                EndHour       = request.EndHour,
                Capacity      = request.Capacity,
                IsFullyClosed = request.IsFullyClosed
            };
            _db.DateOverrides.Add(entity);
        }

        await _db.SaveChangesAsync(ct);
        return OperationResult<DateOverrideResponse>.Ok(ScheduleMapper.ToDateOverrideResponse(entity));
    }

    public async Task<OperationResult<bool>> DeleteOverrideAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.DateOverrides.FindAsync([id], ct);
        if (entity is null)
            return OperationResult<bool>.Fail("DATE_OVERRIDE_NOT_FOUND", $"Date override #{id} was not found.");

        _db.DateOverrides.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return OperationResult<bool>.Ok(true);
    }
}
