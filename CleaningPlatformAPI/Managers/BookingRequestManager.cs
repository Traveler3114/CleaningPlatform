using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Enums;
using CleaningPlatformAPI.Mapping;
using CleaningPlatformAPI.Services;

namespace CleaningPlatformAPI.Managers;

public class BookingRequestManager
{
    private readonly AppDbContext _db;
    private readonly EmailService _email;
    private readonly TokenManager _tokenManager;
    private readonly IConfiguration _config;

    public BookingRequestManager(AppDbContext db, EmailService email, TokenManager tokenManager, IConfiguration config)
    {
        _db = db;
        _email = email;
        _tokenManager = tokenManager;
        _config = config;
    }

    public async Task<PagedResult<BookingRequestResponse>> GetAllAsync(
        PaginationParams pagination,
        string? statusFilter = null,
        CancellationToken ct = default)
    {
        var query = _db.BookingRequests
            .Include(r => r.RequestServices).ThenInclude(s => s.ServiceCatalog)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(statusFilter))
            query = query.Where(r => r.Status == statusFilter);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip(pagination.Skip)
            .Take(pagination.Take)
            .ToListAsync(ct);

        var mapped = items.Select(BookingRequestMapper.ToResponse).ToList();
        return PagedResult<BookingRequestResponse>.From(mapped, totalCount, pagination.Page, pagination.PageSize);
    }

    public async Task<OperationResult<BookingRequestResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var request = await _db.BookingRequests
            .Include(r => r.RequestServices).ThenInclude(s => s.ServiceCatalog)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        return request is null
            ? OperationResult<BookingRequestResponse>.Fail($"Booking request #{id} was not found.")
            : OperationResult<BookingRequestResponse>.Ok(BookingRequestMapper.ToResponse(request));
    }

    public async Task<OperationResult<BookingRequestResponse>> CreateAsync(CreateBookingRequestRequest dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.ContactName))
            return OperationResult<BookingRequestResponse>.Fail("Contact name is required.");
        if (string.IsNullOrWhiteSpace(dto.Phone))
            return OperationResult<BookingRequestResponse>.Fail("Phone number is required.");
        if (string.IsNullOrWhiteSpace(dto.Email))
            return OperationResult<BookingRequestResponse>.Fail("Email is required.");
        if (dto.ServiceCatalogIds is null || dto.ServiceCatalogIds.Count == 0)
            return OperationResult<BookingRequestResponse>.Fail("At least one service must be selected.");

        var validServiceIds = await _db.ServiceCatalog
            .Where(s => dto.ServiceCatalogIds.Contains(s.Id) && s.IsActive)
            .Select(s => s.Id)
            .ToListAsync(ct);

        var invalidIds = dto.ServiceCatalogIds.Except(validServiceIds).ToList();
        if (invalidIds.Count > 0)
            return OperationResult<BookingRequestResponse>.Fail($"Invalid or inactive services: {string.Join(", ", invalidIds)}");

        var now = DateTime.UtcNow;

        var request = new BookingRequest
        {
            ContactName = dto.ContactName.Trim(),
            Phone = dto.Phone.Trim(),
            Email = dto.Email.Trim(),
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
            Status = "New",
            CreatedAt = now,
            UpdatedAt = now,
            RequestServices = validServiceIds.Select(id => new BookingRequestService
            {
                ServiceCatalogId = id
            }).ToList()
        };

        _db.BookingRequests.Add(request);
        await _db.SaveChangesAsync(ct);

        await _email.SendAsync(
            request.Email,
            "We received your service request",
            $"Dear {request.ContactName},\n\nThank you for submitting your service request. We have received it and will be in touch shortly to discuss the details and provide a quote.\n\nBest regards,\nCleanPro Team"
        );

        return OperationResult<BookingRequestResponse>.Ok(BookingRequestMapper.ToResponse(request));
    }

    public async Task<OperationResult<BookingRequestResponse>> UpdateAsync(int id, UpdateBookingRequestRequest dto, CancellationToken ct = default)
    {
        var request = await _db.BookingRequests
            .Include(r => r.RequestServices)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (request is null)
            return OperationResult<BookingRequestResponse>.Fail($"Booking request #{id} was not found.");

        if (request.Status is "Cancelled" or "Converted")
            return OperationResult<BookingRequestResponse>.Fail($"Cannot edit a request with status '{request.Status}'.");

        if (dto.ServiceCatalogIds is not null && dto.ServiceCatalogIds.Count > 0)
        {
            var validServiceIds = await _db.ServiceCatalog
                .Where(s => dto.ServiceCatalogIds.Contains(s.Id) && s.IsActive)
                .Select(s => s.Id)
                .ToListAsync(ct);

            var invalidIds = dto.ServiceCatalogIds.Except(validServiceIds).ToList();
            if (invalidIds.Count > 0)
                return OperationResult<BookingRequestResponse>.Fail($"Invalid or inactive services: {string.Join(", ", invalidIds)}");

            _db.BookingRequestServices.RemoveRange(request.RequestServices);

            request.RequestServices = validServiceIds.Select(id => new BookingRequestService
            {
                BookingRequestId = request.Id,
                ServiceCatalogId = id
            }).ToList();
        }

        request.EstimatedPrice = dto.EstimatedPrice;
        request.AdminNotes = string.IsNullOrWhiteSpace(dto.AdminNotes) ? null : dto.AdminNotes.Trim();
        request.UpdatedAt = DateTime.UtcNow;

        if (request.Status == "New")
            request.Status = "AdminReviewed";

        await _db.SaveChangesAsync(ct);

        // Reload with services for response
        var updated = await _db.BookingRequests
            .Include(r => r.RequestServices).ThenInclude(s => s.ServiceCatalog)
            .FirstAsync(r => r.Id == id, ct);

        return OperationResult<BookingRequestResponse>.Ok(BookingRequestMapper.ToResponse(updated));
    }

    public async Task<OperationResult<BookingRequestResponse>> SendToCustomerAsync(int id, CancellationToken ct = default)
    {
        var request = await _db.BookingRequests
            .Include(r => r.RequestServices).ThenInclude(s => s.ServiceCatalog)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (request is null)
            return OperationResult<BookingRequestResponse>.Fail($"Booking request #{id} was not found.");

        if (request.Status is not ("New" or "AdminReviewed"))
            return OperationResult<BookingRequestResponse>.Fail($"Cannot send to customer. Current status is '{request.Status}'. Expected 'New' or 'AdminReviewed'.");

        var token = _tokenManager.CreateBookingRequestToken(request.Id, request.Email);

        var baseUrl = _config.GetValue<string>("AppBaseUrl") ?? "https://localhost:5001";
        var confirmUrl = $"{baseUrl}/public/confirm-request.html?token={token}";

        var servicesList = string.Join("\n", request.RequestServices
            .Select(s => $"  - {s.ServiceCatalog?.Name ?? "Unknown service"}"));

        request.Status = "SentToCustomer";
        request.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        await _email.SendAsync(
            request.Email,
            "Please confirm your service request",
            $"Dear {request.ContactName},\n\nPlease review your service request details and confirm by clicking the link below:\n\n{confirmUrl}\n\nRequest details:\n  Contact: {request.ContactName}\n  Phone: {request.Phone}\n  Email: {request.Email}\n  Estimated price: {request.EstimatedPrice?.ToString("C") ?? "TBD"}\n  Notes: {request.Notes ?? "None"}\n\nServices:\n{servicesList}\n\nIf you have any questions, please contact us.\n\nBest regards,\nCleanPro Team"
        );

        Console.WriteLine("");
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("  CUSTOMER CONFIRMATION LINK (dev mode):");
        Console.WriteLine($"  {confirmUrl}");
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("");

        return OperationResult<BookingRequestResponse>.Ok(BookingRequestMapper.ToResponse(request));
    }

    public async Task<OperationResult<CustomerPreviewResponse>> CustomerPreviewAsync(string token, CancellationToken ct = default)
    {
        var principal = _tokenManager.ValidateBookingRequestToken(token);
        if (principal is null)
            return OperationResult<CustomerPreviewResponse>.Fail("Invalid or expired token.");

        var requestIdClaim = principal.FindFirst("booking_request_id")?.Value;
        if (requestIdClaim is null || !int.TryParse(requestIdClaim, out var requestId))
            return OperationResult<CustomerPreviewResponse>.Fail("Invalid token payload.");

        var request = await _db.BookingRequests
            .Include(r => r.RequestServices).ThenInclude(s => s.ServiceCatalog)
            .FirstOrDefaultAsync(r => r.Id == requestId, ct);

        if (request is null)
            return OperationResult<CustomerPreviewResponse>.Fail("Request was not found.");

        if (request.Status is not ("SentToCustomer" or "CustomerConfirmed" or "Cancelled"))
            return OperationResult<CustomerPreviewResponse>.Fail("This request is not available for action.");

        return OperationResult<CustomerPreviewResponse>.Ok(BookingRequestMapper.ToPreviewResponse(request));
    }

    public async Task<OperationResult<string>> CustomerConfirmAsync(string token, CancellationToken ct = default)
    {
        var principal = _tokenManager.ValidateBookingRequestToken(token);
        if (principal is null)
            return OperationResult<string>.Fail("Invalid or expired token.");

        var requestIdClaim = principal.FindFirst("booking_request_id")?.Value;
        if (requestIdClaim is null || !int.TryParse(requestIdClaim, out var requestId))
            return OperationResult<string>.Fail("Invalid token payload.");

        var request = await _db.BookingRequests
            .Include(r => r.RequestServices).ThenInclude(s => s.ServiceCatalog)
            .FirstOrDefaultAsync(r => r.Id == requestId, ct);

        if (request is null)
            return OperationResult<string>.Fail("Request was not found.");

        if (request.Status is "Cancelled" or "Converted" or "CustomerConfirmed")
            return OperationResult<string>.Fail($"Request already has status '{request.Status}'.");

        if (request.Status != "SentToCustomer")
            return OperationResult<string>.Fail($"Cannot confirm a request with status '{request.Status}'. Expected 'SentToCustomer'.");

        request.Status = "CustomerConfirmed";
        request.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return OperationResult<string>.Ok("Your request has been confirmed. We will be in touch to schedule the service.");
    }

    public async Task<OperationResult<string>> CustomerCancelAsync(string token, CancellationToken ct = default)
    {
        var principal = _tokenManager.ValidateBookingRequestToken(token);
        if (principal is null)
            return OperationResult<string>.Fail("Invalid or expired token.");

        var requestIdClaim = principal.FindFirst("booking_request_id")?.Value;
        if (requestIdClaim is null || !int.TryParse(requestIdClaim, out var requestId))
            return OperationResult<string>.Fail("Invalid token payload.");

        var request = await _db.BookingRequests.FirstOrDefaultAsync(r => r.Id == requestId, ct);

        if (request is null)
            return OperationResult<string>.Fail("Request was not found.");

        if (request.Status is "Cancelled" or "Converted")
            return OperationResult<string>.Fail($"Request already has status '{request.Status}'.");

        request.Status = "Cancelled";
        request.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return OperationResult<string>.Ok("Your request has been cancelled.");
    }

    public async Task<OperationResult<BookingResponse>> AdminConfirmAsync(int id, CancellationToken ct = default)
    {
        var request = await _db.BookingRequests
            .Include(r => r.RequestServices).ThenInclude(s => s.ServiceCatalog)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (request is null)
            return OperationResult<BookingResponse>.Fail($"Booking request #{id} was not found.");

        if (request.Status != "CustomerConfirmed")
            return OperationResult<BookingResponse>.Fail($"Cannot convert a request with status '{request.Status}'. Expected 'CustomerConfirmed'.");

        if (request.RequestServices is null || request.RequestServices.Count == 0)
            return OperationResult<BookingResponse>.Fail("Cannot convert a request with no services.");

        var now = DateTime.UtcNow;

        var client = new Client
        {
            ClientName = request.ContactName.Trim(),
            Type = "Person",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            Contacts =
            [
                new Contact
                {
                    ContactName = request.ContactName.Trim(),
                    Phone = request.Phone.Trim(),
                    Email = request.Email.Trim(),
                    IsPrimary = true,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            ]
        };

        _db.Clients.Add(client);
        await _db.SaveChangesAsync(ct);

        var serviceType = BookingServiceType.SiteBased;
        var firstCatalog = await _db.ServiceCatalog
            .FirstOrDefaultAsync(s => s.Id == request.RequestServices.First().ServiceCatalogId, ct);
        if (firstCatalog is not null && Enum.TryParse<BookingServiceType>(firstCatalog.ServiceType, true, out var parsed))
            serviceType = parsed;

        var booking = new Booking
        {
            ClientId = client.Id,
            ServiceType = serviceType,
            ScheduledDate = now.Date,
            Status = BookingStatus.Pending,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var rs in request.RequestServices)
        {
            booking.BookingServices.Add(new BookingService
            {
                ServiceCatalogId = rs.ServiceCatalogId,
                EstimatedPrice = rs.ServiceCatalog?.PriceAvg,
                Quantity = 1
            });
        }

        _db.Bookings.Add(booking);
        request.Status = "Converted";
        request.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);

        var createdBooking = await _db.Bookings
            .Include(b => b.Client).ThenInclude(c => c.Contacts)
            .Include(b => b.BookingServices).ThenInclude(bs => bs.ServiceCatalog)
            .FirstAsync(b => b.Id == booking.Id, ct);

        return OperationResult<BookingResponse>.Ok(BookingMapper.ToDetailResponse(createdBooking));
    }
}
