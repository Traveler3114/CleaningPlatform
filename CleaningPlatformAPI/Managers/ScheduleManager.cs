using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;
using Microsoft.Extensions.Localization;
using CleaningPlatformAPI;
using CleaningPlatformAPI.Mapping;
using CleaningPlatformAPI.Common;

namespace CleaningPlatformAPI.Managers;

public class ScheduleManager
{
    private readonly AppDbContext _db;
    private readonly IStringLocalizer<SharedResources> _localizer;
    public ScheduleManager(AppDbContext db, IStringLocalizer<SharedResources> localizer) { _db = db; 
            _localizer = localizer;}

    public async Task<List<WeeklyScheduleResponse>> GetScheduleAsync(CancellationToken ct = default)
    {
        return await _db.WeeklySchedules
            .OrderBy(s => s.DayOfWeek)
            .Select(ScheduleMapper.WeeklyProjection)
            .ToListAsync(ct);
    }

    public async Task<WeeklyScheduleResponse> CreateDayAsync(WeeklyScheduleRequest request, CancellationToken ct = default)
    {
        var err = ValidateScheduleRequest(request.DayOfWeek, request.StartHour, request.EndHour, request.Capacity, true);
        if (err is not null) throw new AppException("SCHEDULE_ERROR", err, 422);

        var dayName  = ((DayOfWeek)request.DayOfWeek).ToString();
        var existing = await _db.WeeklySchedules.FirstOrDefaultAsync(s => s.DayOfWeek == request.DayOfWeek, ct);
        if (existing is not null)
            throw new AppException("SCHEDULE_ENTRY_EXISTS",
                $"A schedule entry for {dayName} already exists. Click the row to edit the existing entry instead.", 409);

        var schedule = new WeeklySchedule
        {
            DayOfWeek = request.DayOfWeek,
            StartHour = request.StartHour,
            EndHour   = request.EndHour,
            Capacity  = request.Capacity
        };
        _db.WeeklySchedules.Add(schedule);
        await _db.SaveChangesAsync(ct);
        return ScheduleMapper.ToWeeklyResponse(schedule);
    }

    public async Task<WeeklyScheduleResponse> UpdateDayAsync(int dayOfWeek, UpdateWeeklyScheduleRequest request, CancellationToken ct = default)
    {
        var err = ValidateScheduleRequest(dayOfWeek, request.StartHour, request.EndHour, request.Capacity, false);
        if (err is not null) throw new AppException("SCHEDULE_ERROR", err, 422);

        var schedule = await _db.WeeklySchedules.FirstOrDefaultAsync(s => s.DayOfWeek == dayOfWeek, ct);
        if (schedule is null)
            throw new AppException("SCHEDULE_ENTRY_NOT_FOUND",
                $"No schedule entry found for {((DayOfWeek)dayOfWeek)}.", 404);

        schedule.StartHour = request.StartHour;
        schedule.EndHour   = request.EndHour;
        schedule.Capacity  = request.Capacity;
        await _db.SaveChangesAsync(ct);
        return ScheduleMapper.ToWeeklyResponse(schedule);
    }

    public async Task DeleteDayAsync(int dayOfWeek, CancellationToken ct = default)
    {
        var schedule = await _db.WeeklySchedules.FirstOrDefaultAsync(s => s.DayOfWeek == dayOfWeek, ct);
        if (schedule is null)
            throw new AppException("SCHEDULE_ENTRY_NOT_FOUND", $"No schedule entry found for {((DayOfWeek)dayOfWeek)}.", 404);

        _db.WeeklySchedules.Remove(schedule);
        await _db.SaveChangesAsync(ct);
        return;
    }

    private static string? ValidateScheduleRequest(int dayOfWeek, int startHour, int endHour, int capacity, bool validateDayOfWeek)
    {
        if (validateDayOfWeek && (dayOfWeek < 0 || dayOfWeek > 6))
            return "Day of week must be between 0 (Sunday) and 6 (Saturday).";

        if (startHour < 0 || endHour > 24)
            return "Start hour must be ≥ 0 and end hour must be ≤ 24.";

        if (startHour >= endHour && !(startHour == 0 && endHour == 0))
            return $"Start hour ({startHour}) must be earlier than end hour ({endHour}).";

        if (capacity < 0)
            return "Capacity cannot be negative.";

        return null;
    }
}
