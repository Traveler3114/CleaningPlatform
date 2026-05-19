using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Extensions;
using CleaningPlatformAPI.Managers;

namespace CleaningPlatformAPI.Pages.Admin;

[Authorize]
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
        if (Booking == null)
            return NotFound();

        if (User.IsInRole(RoleNames.Employee))
        {
            var currentEmployeeId = User.GetEmployeeId();
            bool isAssigned = Booking.AssignedEmployees.Any(e => e.EmployeeId == currentEmployeeId);
            if (!isAssigned)
                return Forbid();
        }

        ActiveEmployees = await _employeeManager.GetActiveEmployeesAsync(ct);
        ServiceCatalog = await _serviceCatalogManager.GetAllAsync(ct);
        SopTemplates = await _sopManager.GetAllTemplatesAsync(ct);
        await _sopManager.EnsureServiceSopsAssignedAsync(Id, ct);
        BookingSops = await _sopManager.GetBookingSopsAsync(Id, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status, CancellationToken ct)
    {
        var canEdit = User.HasPermission(PermissionKeys.BookingsEdit);
        var canProgress = User.HasPermission(PermissionKeys.BookingsProgress);

        if (!canEdit && !canProgress)
            return Forbid();

        if (canProgress && !canEdit)
        {
            var booking = await _bookingManager.GetBookingDetailByIdAsync(id, ct);
            if (booking == null)
                return NotFound();

            var currentEmployeeId = User.GetEmployeeId();
            if (!booking.AssignedEmployees.Any(e => e.EmployeeId == currentEmployeeId))
                return Forbid();

            var allowedTransitions = new Dictionary<string, string>
            {
                ["Confirmed"] = "InProgress",
                ["InProgress"] = "Completed"
            };

            if (!allowedTransitions.TryGetValue(booking.Status, out var expectedNext) || expectedNext != status)
            {
                ErrorMessage = "You can only progress a booking from Confirmed to InProgress or from InProgress to Completed.";
                return RedirectToPage(new { id });
            }
        }

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
        if (!User.HasPermission(PermissionKeys.BookingsEdit) && !User.HasPermission(PermissionKeys.BookingsProgress))
            return Forbid();

        if (User.HasPermission(PermissionKeys.BookingsProgress) && !User.HasPermission(PermissionKeys.BookingsEdit))
        {
            var currentEmployeeId = User.GetEmployeeId();
            var booking = await _bookingManager.GetBookingDetailByIdAsync(id, ct);
            if (booking == null || !booking.AssignedEmployees.Any(e => e.EmployeeId == currentEmployeeId))
                return Forbid();
        }

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