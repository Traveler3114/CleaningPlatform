using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VechileCleaningAPI.Dtos;
using VechileCleaningAPI.Managers;

namespace VechileCleaningAPI.Pages;

public class IndexModel : PageModel
{
    private readonly AvailabilityManager _availability;
    private readonly BookingManager _booking;
    private readonly OverrideManager _override;

    public IndexModel(AvailabilityManager availability, BookingManager booking, OverrideManager @override)
    {
        _availability = availability;
        _booking = booking;
        _override = @override;
    }

    public DateTime SelectedDate { get; set; } = DateTime.Today;
    public List<AvailabilityDto> Slots { get; set; } = new();
    public List<BookingDto> Bookings { get; set; } = new();

    public async Task OnGetAsync(string? date)
    {
        SelectedDate = DateTime.TryParse(date, out var parsed) ? parsed : DateTime.Today;
        Slots = await _availability.GetSlotsAsync(SelectedDate);
        Bookings = await _booking.GetBookingsAsync(SelectedDate);
    }

    public async Task<IActionResult> OnPostOverrideAsync(string date, int hour, int? capacity, bool isClosed)
    {
        if (DateTime.TryParse(date, out var parsedDate))
        {
            await _override.CreateOverrideAsync(new SlotOverrideDto
            {
                Date = parsedDate,
                Hour = hour,
                IsClosed = isClosed,
                Capacity = capacity
            });
        }
        return RedirectToPage(new { date });
    }
}
