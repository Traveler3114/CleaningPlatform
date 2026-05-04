using Microsoft.EntityFrameworkCore;
using VechileCleaningAPI.Data;
using VechileCleaningAPI.Dtos;
using VechileCleaningAPI.Common;

namespace VechileCleaningAPI.Managers;

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
            IsClosed = s.IsClosed,
            StartHour = s.StartHour,
            EndHour = s.EndHour,
            DefaultCapacity = s.DefaultCapacity
        }).ToList();
    }

    public async Task<OperationResult<WeeklyScheduleDto>> UpdateDayAsync(int dayOfWeek, WeeklyScheduleDto dto)
    {
        var schedule = await _db.WeeklySchedules.FirstOrDefaultAsync(s => s.DayOfWeek == dayOfWeek);
        if (schedule == null)
            return OperationResult<WeeklyScheduleDto>.Fail("Schedule not found.");
        schedule.IsClosed = dto.IsClosed;
        schedule.StartHour = dto.StartHour;
        schedule.EndHour = dto.EndHour;
        schedule.DefaultCapacity = dto.DefaultCapacity;
        await _db.SaveChangesAsync();
        return OperationResult<WeeklyScheduleDto>.Ok(dto);
    }
}
