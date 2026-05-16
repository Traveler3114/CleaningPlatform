using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Pages.Admin;

[Authorize(Policy = PermissionKeys.PagesBookings)]
public class InvoiceDetailModel : PageModel
{
    private readonly InvoiceManager _invoiceManager;
    public InvoiceDetailModel(InvoiceManager invoiceManager) { _invoiceManager = invoiceManager; }

    public InvoiceDetailResponse? Invoice { get; set; }
    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [TempData] public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Invoice = await _invoiceManager.GetByIdAsync(Id);
        return Invoice == null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(string status)
    {
        if (!User.HasPermission(PermissionKeys.InvoicesEdit)) return Forbid();
        var result = await _invoiceManager.UpdateStatusAsync(Id, status);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostRecordPaymentAsync(
        DateTime paymentDate, decimal amount, string method, string? reference, string? notes)
    {
        if (!User.HasPermission(PermissionKeys.InvoicesEdit)) return Forbid();

        var result = await _invoiceManager.RecordPaymentAsync(Id, new RecordPaymentRequest
        {
            PaymentDate = paymentDate,
            Amount      = amount,
            Method      = method,
            Reference   = reference,
            Notes       = notes
        }, User.GetEmployeeId());

        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage(new { id = Id });
    }
}
