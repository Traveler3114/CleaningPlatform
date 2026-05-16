using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Pages.Admin;

[Authorize(Policy = PermissionKeys.PagesBookings)]
public class BookingDetailModel : PageModel
{
    private readonly BookingManager _bookingManager;
    private readonly EmployeeManager _employeeManager;
    private readonly ServiceCatalogManager _serviceCatalogManager;
    private readonly InvoiceManager _invoiceManager;
    private readonly SopManager _sopManager;

    public BookingDetailModel(BookingManager bookingManager, EmployeeManager employeeManager,
        ServiceCatalogManager serviceCatalogManager, InvoiceManager invoiceManager, SopManager sopManager)
    {
        _bookingManager = bookingManager;
        _employeeManager = employeeManager;
        _serviceCatalogManager = serviceCatalogManager;
        _invoiceManager = invoiceManager;
        _sopManager = sopManager;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public BookingResponse? Booking { get; set; }
    public List<EmployeeSimpleResponse> ActiveEmployees { get; set; } = [];
    public List<ServiceCatalogResponse> ServiceCatalog { get; set; } = [];
    public List<SopTemplateResponse> SopTemplates { get; set; } = [];
    public List<BookingSopAssignmentResponse> BookingSops { get; set; } = [];

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        Booking = await _bookingManager.GetBookingDetailByIdAsync(Id, ct);
        ActiveEmployees = await _employeeManager.GetActiveEmployeesAsync(ct);
        ServiceCatalog = await _serviceCatalogManager.GetAllAsync(ct);
        SopTemplates = await _sopManager.GetAllTemplatesAsync(ct);
        BookingSops = await _sopManager.GetBookingSopsAsync(Id, ct);
        return Booking == null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.BookingsEdit)) return Forbid();
        var result = await _bookingManager.UpdateStatusAsync(id, status, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAddAssignmentAsync(int id, int employeeId, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.BookingsEdit)) return Forbid();
        var result = await _bookingManager.AddAssignmentAsync(id, employeeId, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRemoveAssignmentAsync(int id, int assignmentId, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.BookingsEdit)) return Forbid();
        var result = await _bookingManager.RemoveAssignmentAsync(id, assignmentId, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAddServiceAsync(int id, int serviceCatalogId, decimal quantity, decimal? estimatedPrice, string? notes, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.BookingsEdit)) return Forbid();
        var result = await _bookingManager.AddServiceAsync(id, serviceCatalogId, estimatedPrice, quantity, null, notes, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRemoveServiceAsync(int id, int serviceId, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.BookingsEdit)) return Forbid();
        var result = await _bookingManager.RemoveServiceAsync(id, serviceId, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostUpdateServicePriceAsync(int id, int serviceId, decimal? finalPrice, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.BookingsEdit)) return Forbid();
        var result = await _bookingManager.UpdateServicePriceAsync(id, serviceId, finalPrice, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAssignSopAsync(int id, int sopTemplateId, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.BookingsEdit)) return Forbid();
        var result = await _sopManager.AssignSopToBookingAsync(id, new AssignSopRequest { SopTemplateId = sopTemplateId }, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCompleteChecklistItemAsync(int id, int assignmentId, int itemId, bool isCompleted, string? notes, CancellationToken ct)
    {
        var result = await _sopManager.CompleteChecklistItemAsync(assignmentId, itemId, new CompleteChecklistItemRequest { IsCompleted = isCompleted, Notes = notes }, ct);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostGenerateInvoiceAsync(int id, CancellationToken ct)
    {
        if (!User.HasPermission(PermissionKeys.InvoicesCreate)) return Forbid();
        var result = await _invoiceManager.CreateFromBookingAsync(id, User.GetEmployeeId(), ct);
        if (!result.Success || result.Data == null)
        {
            ErrorMessage = result.Message;
            return RedirectToPage(new { id });
        }
        return RedirectToPage("/Admin/InvoiceDetail", new { id = result.Data.Id });
    }
}
