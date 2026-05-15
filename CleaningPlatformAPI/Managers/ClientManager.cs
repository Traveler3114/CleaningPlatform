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

    public ClientManager(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ClientResponse>> GetAllAsync(string? search, string? type, CancellationToken ct = default)
    {
        var query = _db.Clients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c => c.ClientName.ToLower().Contains(term) || c.Contacts.Any(con => con.Phone.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(c => c.Type == type);

        return await query
            .OrderBy(c => c.ClientName)
            .Select(c => new ClientResponse
            {
                Id = c.Id,
                ClientName = c.ClientName,
                Type = c.Type,
                Oib = c.Oib,
                PaymentTerms = c.PaymentTerms,
                Notes = c.Notes,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                PrimaryContactName = c.Contacts.Where(con => con.IsPrimary && con.IsActive).Select(con => con.ContactName).FirstOrDefault(),
                PrimaryContactPhone = c.Contacts.Where(con => con.IsPrimary && con.IsActive).Select(con => con.Phone).FirstOrDefault(),
                PrimaryContactEmail = c.Contacts.Where(con => con.IsPrimary && con.IsActive).Select(con => con.Email).FirstOrDefault(),
                TotalBookings = c.Bookings.Count
            })
            .ToListAsync(ct);
    }

    public async Task<ClientResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var client = await _db.Clients
            .Include(c => c.Contacts)
            .Include(c => c.Sites)
            .Include(c => c.Bookings)
                .ThenInclude(b => b.BookingServices)
                    .ThenInclude(bs => bs.ServiceCatalog)
            .Include(c => c.Bookings)
                .ThenInclude(b => b.Assignments)
                    .ThenInclude(a => a.Employee)
                        .ThenInclude(e => e.Role)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        return client == null ? null : ClientMapper.ToProfileResponse(client);
    }

    public async Task<OperationResult<ClientResponse>> CreateAsync(CreateClientRequest dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.ClientName))
            return OperationResult<ClientResponse>.Fail("Client name is required.");

        if (dto.Type != "RepeatIndividual" && dto.Type != "RepeatBusiness")
            return OperationResult<ClientResponse>.Fail("Type must be RepeatIndividual or RepeatBusiness.");

        if (string.IsNullOrWhiteSpace(dto.PrimaryContactName) || string.IsNullOrWhiteSpace(dto.PrimaryContactPhone))
            return OperationResult<ClientResponse>.Fail("Primary contact name and phone are required.");

        var now = DateTime.UtcNow;
        var client = new Client
        {
            ClientName = dto.ClientName.Trim(),
            Type = dto.Type,
            Oib = dto.Oib?.Trim(),
            PaymentTerms = dto.PaymentTerms?.Trim(),
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            Contacts =
            [
                new Contact
                {
                    ContactName = dto.PrimaryContactName.Trim(),
                    Phone = dto.PrimaryContactPhone.Trim(),
                    Email = dto.PrimaryContactEmail?.Trim(),
                    IsPrimary = true,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            ]
        };

        _db.Clients.Add(client);
        await _db.SaveChangesAsync(ct);

        return OperationResult<ClientResponse>.Ok(new ClientResponse
        {
            Id = client.Id,
            ClientName = client.ClientName,
            Type = client.Type,
            Oib = client.Oib,
            PaymentTerms = client.PaymentTerms,
            Notes = client.Notes,
            IsActive = client.IsActive,
            CreatedAt = client.CreatedAt,
            PrimaryContactName = dto.PrimaryContactName,
            PrimaryContactPhone = dto.PrimaryContactPhone,
            PrimaryContactEmail = dto.PrimaryContactEmail,
            TotalBookings = 0
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

        if (rawContacts.Any(c => string.IsNullOrWhiteSpace(c.ContactName) || string.IsNullOrWhiteSpace(c.Phone)))
            return OperationResult<ClientResponse>.Fail("Each contact must have name and phone.");

        var client = await _db.Clients
            .Include(c => c.Contacts)
            .Include(c => c.Sites)
            .Include(c => c.Bookings)
                .ThenInclude(b => b.BookingServices)
                    .ThenInclude(bs => bs.ServiceCatalog)
            .Include(c => c.Bookings)
                .ThenInclude(b => b.Assignments)
                    .ThenInclude(a => a.Employee)
                        .ThenInclude(e => e.Role)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (client == null)
            return OperationResult<ClientResponse>.Fail($"Client #{id} was not found.");

        var now = DateTime.UtcNow;
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            client.ClientName = dto.ClientName.Trim();
            client.Oib = string.IsNullOrWhiteSpace(dto.Oib) ? null : dto.Oib.Trim();
            client.PaymentTerms = string.IsNullOrWhiteSpace(dto.PaymentTerms) ? null : dto.PaymentTerms.Trim();
            client.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
            client.UpdatedAt = now;

            var existingContactsById = client.Contacts.ToDictionary(c => c.Id, c => c);
            var touchedContactIds = new HashSet<int>();

            foreach (var contactRequest in rawContacts)
            {
                Contact contact;
                var contactId = contactRequest.Id.GetValueOrDefault();

                if (contactId > 0)
                {
                    if (!existingContactsById.TryGetValue(contactId, out contact!))
                        return OperationResult<ClientResponse>.Fail($"Contact #{contactId} was not found for this client.");

                    touchedContactIds.Add(contactId);
                }
                else
                {
                    contact = new Contact
                    {
                        ClientId = client.Id,
                        CreatedAt = now
                    };
                    _db.Contacts.Add(contact);
                    client.Contacts.Add(contact);
                }

                contact.ContactName = contactRequest.ContactName.Trim();
                contact.Role = string.IsNullOrWhiteSpace(contactRequest.Role) ? null : contactRequest.Role.Trim();
                contact.Phone = contactRequest.Phone.Trim();
                contact.Email = string.IsNullOrWhiteSpace(contactRequest.Email) ? null : contactRequest.Email.Trim();
                contact.Address = string.IsNullOrWhiteSpace(contactRequest.Address) ? null : contactRequest.Address.Trim();
                contact.IsPrimary = contactRequest.IsPrimary;
                contact.IsActive = contactRequest.IsActive;
                contact.UpdatedAt = now;
            }

            foreach (var existing in client.Contacts.Where(c => !touchedContactIds.Contains(c.Id)))
            {
                existing.IsActive = false;
                existing.IsPrimary = false;
                existing.UpdatedAt = now;
            }

            var activeContacts = client.Contacts.Where(c => c.IsActive).ToList();
            if (activeContacts.Count == 0)
                return OperationResult<ClientResponse>.Fail("At least one active contact with a name and phone number is required.");

            var primaryContact = activeContacts.FirstOrDefault(c => c.IsPrimary) ?? activeContacts.First();
            foreach (var activeContact in activeContacts)
            {
                activeContact.IsPrimary = activeContact.Id == primaryContact.Id;
                activeContact.UpdatedAt = now;
            }

            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return OperationResult<ClientResponse>.Ok(ClientMapper.ToProfileResponse(client));
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
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
        if (validationError != null)
            return OperationResult<SiteResponse>.Fail(validationError);

        var clientExists = await _db.Clients.AnyAsync(c => c.Id == clientId, ct);
        if (!clientExists)
            return OperationResult<SiteResponse>.Fail($"Client #{id} was not found.");

        var now = DateTime.UtcNow;
        var site = new Site
        {
            ClientId = clientId,
            SiteName = dto.SiteName.Trim(),
            Address = dto.Address.Trim(),
            City = string.IsNullOrWhiteSpace(dto.City) ? null : dto.City.Trim(),
            PostalCode = string.IsNullOrWhiteSpace(dto.PostalCode) ? null : dto.PostalCode.Trim(),
            SiteType = string.IsNullOrWhiteSpace(dto.SiteType) ? null : dto.SiteType.Trim(),
            FloorAreaM2 = dto.FloorAreaM2,
            AccessNotes = string.IsNullOrWhiteSpace(dto.AccessNotes) ? null : dto.AccessNotes.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Sites.Add(site);
        await _db.SaveChangesAsync(ct);

        return OperationResult<SiteResponse>.Ok(ClientMapper.ToSiteResponse(site));
    }

    public async Task<OperationResult<SiteResponse>> UpdateSiteAsync(int clientId, int siteId, UpsertSiteRequest dto, CancellationToken ct = default)
    {
        var validationError = ValidateSiteDto(dto);
        if (validationError != null)
            return OperationResult<SiteResponse>.Fail(validationError);

        var site = await _db.Sites.FirstOrDefaultAsync(s => s.Id == siteId && s.ClientId == clientId, ct);
        if (site == null)
            return OperationResult<SiteResponse>.Fail("Site not found.");

        site.SiteName = dto.SiteName.Trim();
        site.Address = dto.Address.Trim();
        site.City = string.IsNullOrWhiteSpace(dto.City) ? null : dto.City.Trim();
        site.PostalCode = string.IsNullOrWhiteSpace(dto.PostalCode) ? null : dto.PostalCode.Trim();
        site.SiteType = string.IsNullOrWhiteSpace(dto.SiteType) ? null : dto.SiteType.Trim();
        site.FloorAreaM2 = dto.FloorAreaM2;
        site.AccessNotes = string.IsNullOrWhiteSpace(dto.AccessNotes) ? null : dto.AccessNotes.Trim();
        site.IsActive = dto.IsActive;
        site.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return OperationResult<SiteResponse>.Ok(ClientMapper.ToSiteResponse(site));
    }

    public async Task<OperationResult<SiteResponse>> DeactivateSiteAsync(int clientId, int siteId, CancellationToken ct = default)
    {
        var site = await _db.Sites.FirstOrDefaultAsync(s => s.Id == siteId && s.ClientId == clientId, ct);
        if (site == null)
            return OperationResult<SiteResponse>.Fail("Site not found.");

        site.IsActive = false;
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
            return "Invalid site type.";

        if (dto.FloorAreaM2.HasValue && dto.FloorAreaM2.Value < 0)
            return "Floor area cannot be negative.";

        return null;
    }
}
