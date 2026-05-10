using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Dtos;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Pages.Admin;

[Authorize(Policy = PermissionKeys.PagesBookings)]
public class InvoicesModel : PageModel
{
    private readonly InvoiceManager _invoiceManager;

    public InvoicesModel(InvoiceManager invoiceManager)
    {
        _invoiceManager = invoiceManager;
    }

    public List<InvoiceSummaryDto> Invoices { get; set; } = [];

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        Invoices = await _invoiceManager.GetAllAsync();
    }

    public async Task<IActionResult> OnPostGenerateFromBookingAsync(int bookingId)
    {
        if (!User.HasPermission(PermissionKeys.ActionsBookingUpdateStatus))
            return Forbid();

        var result = await _invoiceManager.CreateFromBookingAsync(bookingId, User.GetEmployeeId());
        if (!result.Success || result.Data == null)
        {
            ErrorMessage = result.Message;
            return RedirectToPage();
        }

        return RedirectToPage("/Admin/InvoiceDetail", new { id = result.Data.Id });
    }
}
