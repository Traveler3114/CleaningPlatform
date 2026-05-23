using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Mapping;

namespace CleaningPlatformAPI.Managers;

public class ClientManager
{
    private static readonly string[] AllowedSiteTypes = ["Office", "Stairwell", "Garage", "Facility", "Boat", "Vehicle", "Other"];

    private readonly AppDbContext _db;

    public ClientManager(AppDbContext db) { _db = db; }

    public async Task<PagedResult<ClientResponse>> GetAllAsync(
        PaginationParams pagination,
        string? type = null,
        CancellationToken ct = default)
    {
        var query = _db.Clients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(pagination.Search))
        {
            var term = pagination.Search.Trim().ToLower();
            query = query.Where(c =>
                c.ClientName.ToLower().Contains(term) ||
                c.Contacts.Any(con => con.Phone.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(c => c.Type == type);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(c => c.ClientName)
            .Skip(pagination.Skip)
            .Take(pagination.Take)
            .Select(c => new ClientResponse
            {
                Id                   = c.Id,
                ClientName           = c.ClientName,
                Type                 = c.Type,
                Oib                  = c.Oib,
                PaymentTerms         = c.PaymentTerms,
                Notes                = c.Notes,
                IsActive             = c.IsActive,
                CreatedAt            = c.CreatedAt,
                PrimaryContactName   = c.Contacts.Where(con => con.IsPrimary && con.IsActive).Select(con => con.ContactName).FirstOrDefault(),
                PrimaryContactPhone  = c.Contacts.Where(con => con.IsPrimary && con.IsActive).Select(con => con.Phone).FirstOrDefault(),
                PrimaryContactEmail  = c.Contacts.Where(con => con.IsPrimary && con.IsActive).Select(con => con.Email).FirstOrDefault(),
                TotalBookings        = c.Bookings.Count
            })
            .ToListAsync(ct);

        return PagedResult<ClientResponse>.From(items, totalCount, pagination.Page, pagination.PageSize);
    }

    public async Task<OperationResult<ClientResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var client = await _db.Clients
            .Include(c => c.Contacts)
            .Include(c => c.Sites)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (client is null)
            return OperationResult<ClientResponse>.Fail($"Client #{id} was not found.");

        var recentBookings = await _db.Bookings
            .Where(b => b.ClientId == id)
            .OrderByDescending(b => b.ScheduledDate)
            .Take(5)
            .ToListAsync(ct);

        return OperationResult<ClientResponse>.Ok(ClientMapper.ToProfileResponse(client, recentBookings));
    }

    public async Task<PagedResult<BookingResponse>> GetClientBookingsAsync(int clientId, PaginationParams pagination, CancellationToken ct = default)
    {
        var query = _db.Bookings
            .Include(b => b.Client).ThenInclude(c => c.Contacts)
            .Include(b => b.BookingServices)
            .Include(b => b.Assignments).ThenInclude(a => a.Employee).ThenInclude(e => e.Role)
            .Where(b => b.ClientId == clientId);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(b => b.ScheduledDate)
            .ThenByDescending(b => b.Id)
            .Skip(pagination.Skip)
            .Take(pagination.Take)
            .ToListAsync(ct);

        var mapped = items.Select(BookingMapper.ToResponse).ToList();
        return PagedResult<BookingResponse>.From(mapped, totalCount, pagination.Page, pagination.PageSize);
    }

    public async Task<OperationResult<ClientResponse>> CreateAsync(CreateClientRequest dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.ClientName))
            return OperationResult<ClientResponse>.Fail("Client name is required.");

        if (dto.Type != "Person" && dto.Type != "Business")
            return OperationResult<ClientResponse>.Fail("Client type must be Person or Business.");

        if (string.IsNullOrWhiteSpace(dto.PrimaryContactName))
            return OperationResult<ClientResponse>.Fail("Primary contact name is required.");

        if (string.IsNullOrWhiteSpace(dto.PrimaryContactPhone))
            return OperationResult<ClientResponse>.Fail("Primary contact phone number is required.");

        var now = DateTime.UtcNow;
        var client = new Client
        {
            ClientName   = dto.ClientName.Trim(),
            Type         = dto.Type,
            Oib          = dto.Oib?.Trim(),
            PaymentTerms = dto.PaymentTerms?.Trim(),
            Notes        = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
            IsActive     = true,
            CreatedAt    = now,
            UpdatedAt    = now,
            Contacts     =
            [
                new Contact
                {
                    ContactName = dto.PrimaryContactName.Trim(),
                    Phone       = dto.PrimaryContactPhone.Trim(),
                    Email       = dto.PrimaryContactEmail?.Trim(),
                    IsPrimary   = true,
                    IsActive    = true,
                    CreatedAt   = now,
                    UpdatedAt   = now
                }
            ]
        };

        _db.Clients.Add(client);
        await _db.SaveChangesAsync(ct);

        return OperationResult<ClientResponse>.Ok(new ClientResponse
        {
            Id                  = client.Id,
            ClientName          = client.ClientName,
            Type                = client.Type,
            Oib                 = client.Oib,
            PaymentTerms        = client.PaymentTerms,
            Notes               = client.Notes,
            IsActive            = client.IsActive,
            CreatedAt           = client.CreatedAt,
            PrimaryContactName  = dto.PrimaryContactName,
            PrimaryContactPhone = dto.PrimaryContactPhone,
            PrimaryContactEmail = dto.PrimaryContactEmail,
            TotalBookings       = 0
        });
    }

    public async Task<OperationResult<ClientResponse>> UpdateProfileAsync(int id, UpdateClientProfileRequest dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.ClientName))
            return OperationResult<ClientResponse>.Fail("Client name is required.");

        var rawContacts = (dto.Contacts ?? [])
            .Where(c => !string.IsNullOrWhiteSpace(c.ContactName) || !string.IsNullOrWhiteSpace(c.Phone))
            .ToList();

        if (rawContacts.Count == 0)
            return OperationResult<ClientResponse>.Fail("At least one active contact with a name and phone number is required.");

        var invalidContacts = rawContacts.Where(c => string.IsNullOrWhiteSpace(c.ContactName) || string.IsNullOrWhiteSpace(c.Phone)).ToList();
        if (invalidContacts.Count > 0)
            return OperationResult<ClientResponse>.Fail("Every contact must have both a name and a phone number. Please check your contact entries.");

        var client = await _db.Clients
            .Include(c => c.Contacts)
            .Include(c => c.Sites)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (client is null)
            return OperationResult<ClientResponse>.Fail($"Client #{id} was not found.");

        var now = DateTime.UtcNow;
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            client.ClientName   = dto.ClientName.Trim();
            client.Oib          = string.IsNullOrWhiteSpace(dto.Oib)          ? null : dto.Oib.Trim();
            client.PaymentTerms = string.IsNullOrWhiteSpace(dto.PaymentTerms) ? null : dto.PaymentTerms.Trim();
            client.Notes        = string.IsNullOrWhiteSpace(dto.Notes)        ? null : dto.Notes.Trim();
            client.UpdatedAt    = now;

            SyncContacts(client, rawContacts, now);

            var activeContacts = client.Contacts.Where(c => c.IsActive).ToList();
            if (activeContacts.Count == 0)
                return OperationResult<ClientResponse>.Fail("At least one active contact is required. You cannot deactivate all contacts.");

            EnforceSinglePrimary(activeContacts, now);

            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            var recentBookings = await _db.Bookings
                .Where(b => b.ClientId == id)
                .OrderByDescending(b => b.ScheduledDate)
                .Take(5)
                .ToListAsync(ct);

            return OperationResult<ClientResponse>.Ok(ClientMapper.ToProfileResponse(client, recentBookings));
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    private void SyncContacts(Client client, List<UpsertContactRequest> incoming, DateTime now)
    {
        var existingById     = client.Contacts.ToDictionary(c => c.Id, c => c);
        HashSet<int> touchedContactIds = [];

        foreach (var req in incoming)
        {
            var contactId = req.Id.GetValueOrDefault();
            Contact contact;

            if (contactId > 0 && existingById.TryGetValue(contactId, out var existing))
            {
                contact = existing;
                touchedContactIds.Add(contactId);
            }
            else
            {
                contact = new Contact { ClientId = client.Id, CreatedAt = now };
                _db.Contacts.Add(contact);
                client.Contacts.Add(contact);
            }

            contact.ContactName = req.ContactName.Trim();
            contact.Role        = string.IsNullOrWhiteSpace(req.Role)    ? null : req.Role.Trim();
            contact.Phone       = req.Phone.Trim();
            contact.Email       = string.IsNullOrWhiteSpace(req.Email)   ? null : req.Email.Trim();
            contact.Address     = string.IsNullOrWhiteSpace(req.Address) ? null : req.Address.Trim();
            contact.IsPrimary   = req.IsPrimary;
            contact.IsActive    = req.IsActive;
            contact.UpdatedAt   = now;
        }

        foreach (var c in client.Contacts.Where(c => c.Id > 0 && !touchedContactIds.Contains(c.Id)))
        {
            c.IsActive  = false;
            c.IsPrimary = false;
            c.UpdatedAt = now;
        }
    }

    private static void EnforceSinglePrimary(List<Contact> activeContacts, DateTime now)
    {
        var primary = activeContacts.FirstOrDefault(c => c.IsPrimary) ?? activeContacts[0];
        foreach (var c in activeContacts)
        {
            c.IsPrimary = c.Id == primary.Id;
            c.UpdatedAt = now;
        }
    }

    public async Task<OperationResult<List<SiteResponse>>> GetSitesAsync(int clientId, CancellationToken ct = default)
    {
        var exists = await _db.Clients.AnyAsync(c => c.Id == clientId, ct);
        if (!exists)
            return OperationResult<List<SiteResponse>>.Fail($"Client #{clientId} was not found.");

        var sites = await _db.Sites
            .Where(s => s.ClientId == clientId)
            .OrderByDescending(s => s.IsActive)
            .ThenBy(s => s.SiteName)
            .Select(ClientMapper.ToSiteResponseExpression())
            .ToListAsync(ct);

        return OperationResult<List<SiteResponse>>.Ok(sites);
    }

    public async Task<OperationResult<SiteResponse>> CreateSiteAsync(int clientId, UpsertSiteRequest dto, CancellationToken ct = default)
    {
        var validationError = ValidateSiteDto(dto);
        if (validationError is not null)
            return OperationResult<SiteResponse>.Fail(validationError);

        var clientExists = await _db.Clients.AnyAsync(c => c.Id == clientId, ct);
        if (!clientExists)
            return OperationResult<SiteResponse>.Fail($"Client #{clientId} was not found.");

        var now  = DateTime.UtcNow;
        var site = new Site
        {
            ClientId     = clientId,
            SiteName     = dto.SiteName.Trim(),
            Address      = dto.Address.Trim(),
            City         = string.IsNullOrWhiteSpace(dto.City)         ? null : dto.City.Trim(),
            PostalCode   = string.IsNullOrWhiteSpace(dto.PostalCode)   ? null : dto.PostalCode.Trim(),
            SiteType     = string.IsNullOrWhiteSpace(dto.SiteType)     ? null : dto.SiteType.Trim(),
            FloorAreaM2  = dto.FloorAreaM2,
            AccessNotes  = string.IsNullOrWhiteSpace(dto.AccessNotes)  ? null : dto.AccessNotes.Trim(),
            IsActive     = dto.IsActive,
            CreatedAt    = now,
            UpdatedAt    = now
        };

        _db.Sites.Add(site);
        await _db.SaveChangesAsync(ct);
        return OperationResult<SiteResponse>.Ok(ClientMapper.ToSiteResponse(site));
    }

    public async Task<OperationResult<SiteResponse>> UpdateSiteAsync(int clientId, int siteId, UpsertSiteRequest dto, CancellationToken ct = default)
    {
        var validationError = ValidateSiteDto(dto);
        if (validationError is not null)
            return OperationResult<SiteResponse>.Fail(validationError);

        var site = await _db.Sites.FirstOrDefaultAsync(s => s.Id == siteId && s.ClientId == clientId, ct);
        if (site is null)
            return OperationResult<SiteResponse>.Fail($"Site #{siteId} was not found for client #{clientId}.");

        site.SiteName    = dto.SiteName.Trim();
        site.Address     = dto.Address.Trim();
        site.City        = string.IsNullOrWhiteSpace(dto.City)        ? null : dto.City.Trim();
        site.PostalCode  = string.IsNullOrWhiteSpace(dto.PostalCode)  ? null : dto.PostalCode.Trim();
        site.SiteType    = string.IsNullOrWhiteSpace(dto.SiteType)    ? null : dto.SiteType.Trim();
        site.FloorAreaM2 = dto.FloorAreaM2;
        site.AccessNotes = string.IsNullOrWhiteSpace(dto.AccessNotes) ? null : dto.AccessNotes.Trim();
        site.IsActive    = dto.IsActive;
        site.UpdatedAt   = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return OperationResult<SiteResponse>.Ok(ClientMapper.ToSiteResponse(site));
    }

    public async Task<OperationResult<SiteResponse>> DeactivateSiteAsync(int clientId, int siteId, CancellationToken ct = default)
    {
        var site = await _db.Sites.FirstOrDefaultAsync(s => s.Id == siteId && s.ClientId == clientId, ct);
        if (site is null)
            return OperationResult<SiteResponse>.Fail($"Site #{siteId} was not found for client #{clientId}.");

        site.IsActive  = false;
        site.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return OperationResult<SiteResponse>.Ok(ClientMapper.ToSiteResponse(site));
    }

    private static string? ValidateSiteDto(UpsertSiteRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.SiteName))
            return "Site name is required.";

        if (string.IsNullOrWhiteSpace(dto.Address))
            return "Address is required.";

        if (!string.IsNullOrWhiteSpace(dto.SiteType) && !AllowedSiteTypes.Contains(dto.SiteType.Trim()))
            return $"'{dto.SiteType}' is not a valid site type. Accepted values: {string.Join(", ", AllowedSiteTypes)}.";

        if (dto.FloorAreaM2.HasValue && dto.FloorAreaM2.Value < 0)
            return "Floor area cannot be negative.";

        return null;
    }
}