using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VechileCleaningAPI.Dtos;
using VechileCleaningAPI.Managers;

namespace VechileCleaningAPI.Pages;

public class BookingsModel : PageModel
{
    private readonly BookingManager _booking;

    public BookingsModel(BookingManager booking)
    {
        _booking = booking;
    }

    public DateTime SelectedDate { get; set; } = DateTime.Today;
    public bool ShowAll { get; set; }
    public List<BookingDto> Bookings { get; set; } = new();
    public string? Message { get; set; }

    public async Task OnGetAsync(string? date, string? all)
    {
        ShowAll = all == "true";
        SelectedDate = DateTime.TryParse(date, out var parsed) ? parsed : DateTime.Today;
        Bookings = ShowAll
            ? await _booking.GetAllBookingsAsync()
            : await _booking.GetBookingsAsync(SelectedDate);
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status, string? date, string? all)
    {
        await _booking.UpdateStatusAsync(id, status);
        Message = "Status updated.";
        ShowAll = all == "true";
        SelectedDate = DateTime.TryParse(date, out var parsed) ? parsed : DateTime.Today;
        Bookings = ShowAll
            ? await _booking.GetAllBookingsAsync()
            : await _booking.GetBookingsAsync(SelectedDate);
        return Page();
    }
}
