using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Dtos;
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

    public BookingDetailModel(BookingManager bookingManager, EmployeeManager employeeManager, ServiceCatalogManager serviceCatalogManager, InvoiceManager invoiceManager)
    {
        _bookingManager = bookingManager;
        _employeeManager = employeeManager;
        _serviceCatalogManager = serviceCatalogManager;
        _invoiceManager = invoiceManager;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public BookingDetailDto? Booking { get; set; }
    public List<EmployeeSimpleDto> ActiveEmployees { get; set; } = [];
    public List<ServiceCatalogDto> ServiceCatalog { get; set; } = [];

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var bookingTask = _bookingManager.GetBookingDetailByIdAsync(Id);
        var employeesTask = _employeeManager.GetActiveEmployeesAsync();
        var servicesTask = _serviceCatalogManager.GetAllAsync();

        await Task.WhenAll(bookingTask, employeesTask, servicesTask);

        Booking = bookingTask.Result;
        ActiveEmployees = employeesTask.Result;
        ServiceCatalog = servicesTask.Result;

        return Booking == null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status)
    {
        if (!User.HasPermission(PermissionKeys.ActionsBookingUpdateStatus))
            return Forbid();

        var result = await _bookingManager.UpdateStatusAsync(id, status);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAssignEmployeeAsync(int id, int? employeeId)
    {
        if (!User.HasPermission(PermissionKeys.ActionsBookingUpdateStatus))
            return Forbid();

        var result = await _bookingManager.AssignEmployeeAsync(id, employeeId);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAddServiceAsync(int id, int serviceCatalogId, decimal quantity, decimal? estimatedPrice, string? notes)
    {
        if (!User.HasPermission(PermissionKeys.ActionsBookingUpdateStatus))
            return Forbid();

        var result = await _bookingManager.AddServiceAsync(id, serviceCatalogId, estimatedPrice, quantity, null, notes);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRemoveServiceAsync(int id, int serviceId)
    {
        if (!User.HasPermission(PermissionKeys.ActionsBookingUpdateStatus))
            return Forbid();

        var result = await _bookingManager.RemoveServiceAsync(id, serviceId);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostUpdateServicePriceAsync(int id, int serviceId, decimal? finalPrice)
    {
        if (!User.HasPermission(PermissionKeys.ActionsBookingUpdateStatus))
            return Forbid();

        var result = await _bookingManager.UpdateServicePriceAsync(id, serviceId, finalPrice);
        if (!result.Success) ErrorMessage = result.Message;
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostGenerateInvoiceAsync(int id)
    {
        if (!User.HasPermission(PermissionKeys.ActionsBookingUpdateStatus))
            return Forbid();

        var result = await _invoiceManager.CreateFromBookingAsync(id, User.GetEmployeeId());
        if (!result.Success || result.Data == null)
        {
            ErrorMessage = result.Message;
            return RedirectToPage(new { id });
        }

        return RedirectToPage("/Admin/InvoiceDetail", new { id = result.Data.Id });
    }
}
