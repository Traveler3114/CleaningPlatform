using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Dtos;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Common;

namespace CleaningPlatformAPI.Managers;

public class ScheduleManager
{
    private readonly AppDbContext _db;

    public ScheduleManager(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<WeeklyScheduleDto>> GetScheduleAsync()
    {
        var schedules = await _db.WeeklySchedules.OrderBy(s => s.DayOfWeek).ToListAsync();
        return schedules.Select(s => new WeeklyScheduleDto
        {
            DayOfWeek = s.DayOfWeek,
            StartHour = s.StartHour,
            EndHour = s.EndHour,
            Capacity = s.Capacity,
        }).ToList();
    }

    public async Task<OperationResult<WeeklyScheduleDto>> CreateDayAsync(WeeklyScheduleDto dto)
    {
        var existing = await _db.WeeklySchedules.FirstOrDefaultAsync(s => s.DayOfWeek == dto.DayOfWeek);
        if (existing != null)
            return OperationResult<WeeklyScheduleDto>.Fail("A schedule for this day already exists.");

        var schedule = new WeeklySchedule
        {
            DayOfWeek = dto.DayOfWeek,
            StartHour = dto.StartHour,
            EndHour = dto.EndHour,
            Capacity = dto.Capacity,
        };
        _db.WeeklySchedules.Add(schedule);
        await _db.SaveChangesAsync();
        return OperationResult<WeeklyScheduleDto>.Ok(dto);
    }

    public async Task<OperationResult<WeeklyScheduleDto>> UpdateDayAsync(int dayOfWeek, WeeklyScheduleDto dto)
    {
        var schedule = await _db.WeeklySchedules.FirstOrDefaultAsync(s => s.DayOfWeek == dayOfWeek);
        if (schedule == null)
            return OperationResult<WeeklyScheduleDto>.Fail("Schedule not found.");
        schedule.StartHour = dto.StartHour;
        schedule.EndHour = dto.EndHour;
        schedule.Capacity = dto.Capacity;
        await _db.SaveChangesAsync();
        return OperationResult<WeeklyScheduleDto>.Ok(dto);
    }

    public async Task<OperationResult<bool>> DeleteDayAsync(int dayOfWeek)
    {
        var schedule = await _db.WeeklySchedules.FirstOrDefaultAsync(s => s.DayOfWeek == dayOfWeek);
        if (schedule == null)
            return OperationResult<bool>.Fail("Schedule not found.");
        _db.WeeklySchedules.Remove(schedule);
        await _db.SaveChangesAsync();
        return OperationResult<bool>.Ok(true);
    }
}
