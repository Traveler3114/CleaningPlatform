using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Mapping;

namespace CleaningPlatformAPI.Managers;

public class ScheduleManager
{
    private readonly AppDbContext _db;
    public ScheduleManager(AppDbContext db) { _db = db; }

    public async Task<List<WeeklyScheduleResponse>> GetScheduleAsync(CancellationToken ct = default)
    {
        var schedules = await _db.WeeklySchedules.OrderBy(s => s.DayOfWeek).ToListAsync(ct);
        return schedules.Select(ScheduleMapper.ToWeeklyResponse).ToList();
    }

    public async Task<OperationResult<WeeklyScheduleResponse>> CreateDayAsync(WeeklyScheduleRequest request, CancellationToken ct = default)
    {
        var err = ValidateScheduleRequest(request.DayOfWeek, request.StartHour, request.EndHour, request.Capacity, true);
        if (err != null) return OperationResult<WeeklyScheduleResponse>.Fail(err);

        var dayName  = ((DayOfWeek)request.DayOfWeek).ToString();
        var existing = await _db.WeeklySchedules.FirstOrDefaultAsync(s => s.DayOfWeek == request.DayOfWeek, ct);
        if (existing != null)
            return OperationResult<WeeklyScheduleResponse>.Fail(
                $"A schedule entry for {dayName} already exists. Click the row to edit the existing entry instead.");

        var schedule = new WeeklySchedule
        {
            DayOfWeek = request.DayOfWeek,
            StartHour = request.StartHour,
            EndHour   = request.EndHour,
            Capacity  = request.Capacity
        };
        _db.WeeklySchedules.Add(schedule);
        await _db.SaveChangesAsync(ct);
        return OperationResult<WeeklyScheduleResponse>.Ok(ScheduleMapper.ToWeeklyResponse(schedule));
    }

    public async Task<OperationResult<WeeklyScheduleResponse>> UpdateDayAsync(int dayOfWeek, UpdateWeeklyScheduleRequest request, CancellationToken ct = default)
    {
        var err = ValidateScheduleRequest(dayOfWeek, request.StartHour, request.EndHour, request.Capacity, false);
        if (err != null) return OperationResult<WeeklyScheduleResponse>.Fail(err);

        var schedule = await _db.WeeklySchedules.FirstOrDefaultAsync(s => s.DayOfWeek == dayOfWeek, ct);
        if (schedule == null)
            return OperationResult<WeeklyScheduleResponse>.Fail(
                $"No schedule entry found for {((DayOfWeek)dayOfWeek)}.");

        schedule.StartHour = request.StartHour;
        schedule.EndHour   = request.EndHour;
        schedule.Capacity  = request.Capacity;
        await _db.SaveChangesAsync(ct);
        return OperationResult<WeeklyScheduleResponse>.Ok(ScheduleMapper.ToWeeklyResponse(schedule));
    }

    public async Task<OperationResult<bool>> DeleteDayAsync(int dayOfWeek, CancellationToken ct = default)
    {
        var schedule = await _db.WeeklySchedules.FirstOrDefaultAsync(s => s.DayOfWeek == dayOfWeek, ct);
        if (schedule == null)
            return OperationResult<bool>.Fail($"No schedule entry found for {((DayOfWeek)dayOfWeek)}.");

        _db.WeeklySchedules.Remove(schedule);
        await _db.SaveChangesAsync(ct);
        return OperationResult<bool>.Ok(true);
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
