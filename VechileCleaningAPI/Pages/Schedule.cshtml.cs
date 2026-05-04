using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VechileCleaningAPI.Dtos;
using VechileCleaningAPI.Managers;

namespace VechileCleaningAPI.Pages;

public class ScheduleModel : PageModel
{
    private readonly ScheduleManager _schedule;

    public ScheduleModel(ScheduleManager schedule)
    {
        _schedule = schedule;
    }

    public List<WeeklyScheduleDto> Schedule { get; set; } = new();
    public string? Message { get; set; }

    public async Task OnGetAsync()
    {
        Schedule = await _schedule.GetScheduleAsync();
    }

    public async Task<IActionResult> OnPostUpdateDayAsync(int dayOfWeek, bool isClosed, int startHour, int endHour, int defaultCapacity)
    {
        var dto = new WeeklyScheduleDto
        {
            DayOfWeek = dayOfWeek,
            IsClosed = isClosed,
            StartHour = startHour,
            EndHour = endHour,
            DefaultCapacity = defaultCapacity
        };
        await _schedule.UpdateDayAsync(dayOfWeek, dto);
        Message = "Schedule updated.";
        Schedule = await _schedule.GetScheduleAsync();
        return Page();
    }
}
