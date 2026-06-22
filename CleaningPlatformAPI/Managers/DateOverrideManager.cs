using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;
using Microsoft.Extensions.Localization;
using CleaningPlatformAPI;
using CleaningPlatformAPI.Mapping;
using CleaningPlatformAPI.Common;

namespace CleaningPlatformAPI.Managers;

public class DateOverrideManager
{
    private readonly AppDbContext _db;
    private readonly IStringLocalizer<SharedResources> _localizer;
    public DateOverrideManager(AppDbContext db, IStringLocalizer<SharedResources> localizer) { _db = db; 
            _localizer = localizer;}

    public async Task<List<DateOverrideResponse>> GetOverridesAsync(CancellationToken ct = default)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _db.DateOverrides
            .Where(o => o.Date >= cutoff)
            .OrderBy(o => o.Date)
            .Select(ScheduleMapper.DateOverrideProjection)
            .ToListAsync(ct);
    }

    public async Task<DateOverrideResponse> CreateOverrideAsync(DateOverrideRequest request, CancellationToken ct = default)
    {
        if (request.Date < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new AppException("DATE_OVERRIDE_PAST",
                $"Date overrides cannot be created for past dates. The selected date was {request.Date:dd MMM yyyy}.", 422);

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
        return ScheduleMapper.ToDateOverrideResponse(entity);
    }

    public async Task DeleteOverrideAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.DateOverrides.FindAsync([id], ct);
        if (entity is null)
            throw new AppException("DATE_OVERRIDE_NOT_FOUND", $"Date override #{id} was not found.", 404);

        _db.DateOverrides.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return;
    }
}
