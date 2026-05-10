// CleaningPlatformAPI/Managers/ClientManager.cs

using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Dtos;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Common;

namespace CleaningPlatformAPI.Managers;

public class ClientManager
{
    private static readonly string[] AllowedSiteTypes = ["Office", "Stairwell", "Garage", "Facility", "Boat", "Vehicle", "Other"];

    private readonly AppDbContext _db;

    public ClientManager(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ClientDto>> GetAllAsync(string? search, string? type)
    {
        var query = _db.Clients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c =>
                c.ClientName.ToLower().Contains(term) ||
                c.Contacts.Any(con => con.Phone.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(c => c.Type == type);
        }

        var clients = await query
            .OrderBy(c => c.ClientName)
            .Select(c => new ClientDto
            {
                Id = c.Id,
                ClientName = c.ClientName,
                Type = c.Type,
                Oib = c.Oib,
                PaymentTerms = c.PaymentTerms,
                Notes = c.Notes,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                PrimaryContactName = c.Contacts
                    .Where(con => con.IsPrimary)
                    .Select(con => con.ContactName)
                    .FirstOrDefault(),
                PrimaryContactPhone = c.Contacts
                    .Where(con => con.IsPrimary)
                    .Select(con => con.Phone)
                    .FirstOrDefault(),
                PrimaryContactEmail = c.Contacts
                    .Where(con => con.IsPrimary)
                    .Select(con => con.Email)
                    .FirstOrDefault(),
                TotalBookings = c.Bookings.Count
            })
            .ToListAsync();

        return clients;
    }

    public async Task<ClientProfileDto?> GetByIdAsync(int id)
    {
        var client = await _db.Clients
            .Include(c => c.Contacts)
            .Include(c => c.Sites)
            .Include(c => c.Bookings)
                .ThenInclude(b => b.BookingServices)
                    .ThenInclude(bs => bs.ServiceCatalog)
            .FirstOrDefaultAsync(c => c.Id == id);

        return client == null ? null : MapToProfileDto(client);
    }

    public async Task<OperationResult<ClientDto>> CreateAsync(CreateClientDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ClientName))
            return OperationResult<ClientDto>.Fail("Client name is required.");

        if (dto.Type != "RepeatIndividual" && dto.Type != "RepeatBusiness")
            return OperationResult<ClientDto>.Fail("Type must be RepeatIndividual or RepeatBusiness.");

        if (string.IsNullOrWhiteSpace(dto.PrimaryContactName) ||
            string.IsNullOrWhiteSpace(dto.PrimaryContactPhone))
            return OperationResult<ClientDto>.Fail("Primary contact name and phone are required.");

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
        await _db.SaveChangesAsync();

        var resultDto = new ClientDto
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
        };

        return OperationResult<ClientDto>.Ok(resultDto);
    }

    public async Task<OperationResult<ClientProfileDto>> UpdateProfileAsync(int id, UpdateClientProfileDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ClientName))
            return OperationResult<ClientProfileDto>.Fail("Client name is required.");

        var rawContacts = (dto.Contacts ?? [])
            .Where(c => !string.IsNullOrWhiteSpace(c.ContactName) || !string.IsNullOrWhiteSpace(c.Phone))
            .ToList();

        if (rawContacts.Count == 0)
            return OperationResult<ClientProfileDto>.Fail("At least one contact is required.");

        if (rawContacts.Any(c => string.IsNullOrWhiteSpace(c.ContactName) || string.IsNullOrWhiteSpace(c.Phone)))
            return OperationResult<ClientProfileDto>.Fail("Each contact must have name and phone.");

        var client = await _db.Clients
            .Include(c => c.Contacts)
            .Include(c => c.Sites)
            .Include(c => c.Bookings)
                .ThenInclude(b => b.BookingServices)
                    .ThenInclude(bs => bs.ServiceCatalog)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
            return OperationResult<ClientProfileDto>.Fail("Client not found.");

        var now = DateTime.UtcNow;

        client.ClientName = dto.ClientName.Trim();
        client.Oib = string.IsNullOrWhiteSpace(dto.Oib) ? null : dto.Oib.Trim();
        client.PaymentTerms = string.IsNullOrWhiteSpace(dto.PaymentTerms) ? null : dto.PaymentTerms.Trim();
        client.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
        client.UpdatedAt = now;

        var existingContactsById = client.Contacts.ToDictionary(c => c.Id, c => c);
        var touchedContactIds = new HashSet<int>();

        foreach (var contactDto in rawContacts)
        {
            Contact contact;
            var contactId = contactDto.Id.GetValueOrDefault();

            if (contactId > 0)
            {
                if (!existingContactsById.TryGetValue(contactId, out contact!))
                    return OperationResult<ClientProfileDto>.Fail($"Contact #{contactId} was not found for this client.");

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

            contact.ContactName = contactDto.ContactName.Trim();
            contact.Role = string.IsNullOrWhiteSpace(contactDto.Role) ? null : contactDto.Role.Trim();
            contact.Phone = contactDto.Phone.Trim();
            contact.Email = string.IsNullOrWhiteSpace(contactDto.Email) ? null : contactDto.Email.Trim();
            contact.Address = string.IsNullOrWhiteSpace(contactDto.Address) ? null : contactDto.Address.Trim();
            contact.IsPrimary = contactDto.IsPrimary;
            contact.IsActive = contactDto.IsActive;
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
            return OperationResult<ClientProfileDto>.Fail("At least one active contact is required.");

        var primaryContact = activeContacts.FirstOrDefault(c => c.IsPrimary) ?? activeContacts.First();
        foreach (var activeContact in activeContacts)
        {
            activeContact.IsPrimary = activeContact.Id == primaryContact.Id;
            activeContact.UpdatedAt = now;
        }

        await _db.SaveChangesAsync();
        return OperationResult<ClientProfileDto>.Ok(MapToProfileDto(client));
    }

    public async Task<OperationResult<List<SiteDto>>> GetSitesAsync(int clientId)
    {
        var exists = await _db.Clients.AnyAsync(c => c.Id == clientId);
        if (!exists)
            return OperationResult<List<SiteDto>>.Fail("Client not found.");

        var sites = await _db.Sites
            .Where(s => s.ClientId == clientId)
            .OrderByDescending(s => s.IsActive)
            .ThenBy(s => s.SiteName)
            .Select(MapToSiteDtoExpression())
            .ToListAsync();

        return OperationResult<List<SiteDto>>.Ok(sites);
    }

    public async Task<OperationResult<SiteDto>> CreateSiteAsync(int clientId, UpsertSiteDto dto)
    {
        var validationError = ValidateSiteDto(dto);
        if (validationError != null)
            return OperationResult<SiteDto>.Fail(validationError);

        var clientExists = await _db.Clients.AnyAsync(c => c.Id == clientId);
        if (!clientExists)
            return OperationResult<SiteDto>.Fail("Client not found.");

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
        await _db.SaveChangesAsync();

        return OperationResult<SiteDto>.Ok(MapToSiteDto(site));
    }

    public async Task<OperationResult<SiteDto>> UpdateSiteAsync(int clientId, int siteId, UpsertSiteDto dto)
    {
        var validationError = ValidateSiteDto(dto);
        if (validationError != null)
            return OperationResult<SiteDto>.Fail(validationError);

        var site = await _db.Sites.FirstOrDefaultAsync(s => s.Id == siteId && s.ClientId == clientId);
        if (site == null)
            return OperationResult<SiteDto>.Fail("Site not found.");

        site.SiteName = dto.SiteName.Trim();
        site.Address = dto.Address.Trim();
        site.City = string.IsNullOrWhiteSpace(dto.City) ? null : dto.City.Trim();
        site.PostalCode = string.IsNullOrWhiteSpace(dto.PostalCode) ? null : dto.PostalCode.Trim();
        site.SiteType = string.IsNullOrWhiteSpace(dto.SiteType) ? null : dto.SiteType.Trim();
        site.FloorAreaM2 = dto.FloorAreaM2;
        site.AccessNotes = string.IsNullOrWhiteSpace(dto.AccessNotes) ? null : dto.AccessNotes.Trim();
        site.IsActive = dto.IsActive;
        site.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return OperationResult<SiteDto>.Ok(MapToSiteDto(site));
    }

    public async Task<OperationResult<SiteDto>> DeactivateSiteAsync(int clientId, int siteId)
    {
        var site = await _db.Sites.FirstOrDefaultAsync(s => s.Id == siteId && s.ClientId == clientId);
        if (site == null)
            return OperationResult<SiteDto>.Fail("Site not found.");

        site.IsActive = false;
        site.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return OperationResult<SiteDto>.Ok(MapToSiteDto(site));
    }

    private static string? ValidateSiteDto(UpsertSiteDto dto)
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

    private static SiteDto MapToSiteDto(Site site) => new()
    {
        Id = site.Id,
        ClientId = site.ClientId,
        SiteName = site.SiteName,
        Address = site.Address,
        City = site.City,
        PostalCode = site.PostalCode,
        SiteType = site.SiteType,
        FloorAreaM2 = site.FloorAreaM2,
        AccessNotes = site.AccessNotes,
        IsActive = site.IsActive
    };

    private static System.Linq.Expressions.Expression<Func<Site, SiteDto>> MapToSiteDtoExpression() => site => new SiteDto
    {
        Id = site.Id,
        ClientId = site.ClientId,
        SiteName = site.SiteName,
        Address = site.Address,
        City = site.City,
        PostalCode = site.PostalCode,
        SiteType = site.SiteType,
        FloorAreaM2 = site.FloorAreaM2,
        AccessNotes = site.AccessNotes,
        IsActive = site.IsActive
    };

    private static ClientProfileDto MapToProfileDto(Client client)
    {
        return new ClientProfileDto
        {
            Id = client.Id,
            ClientName = client.ClientName,
            Type = client.Type,
            Oib = client.Oib,
            PaymentTerms = client.PaymentTerms,
            Notes = client.Notes,
            IsActive = client.IsActive,
            CreatedAt = client.CreatedAt,
            PrimaryContactName = client.Contacts
                .FirstOrDefault(c => c.IsPrimary)?.ContactName,
            PrimaryContactPhone = client.Contacts
                .FirstOrDefault(c => c.IsPrimary)?.Phone,
            PrimaryContactEmail = client.Contacts
                .FirstOrDefault(c => c.IsPrimary)?.Email,
            TotalBookings = client.Bookings.Count,
            Contacts = client.Contacts
                .OrderByDescending(c => c.IsPrimary)
                .ThenByDescending(c => c.IsActive)
                .ThenBy(c => c.ContactName)
                .Select(c => new ContactDto
                {
                    Id = c.Id,
                    ClientId = c.ClientId,
                    ContactName = c.ContactName,
                    Role = c.Role,
                    Phone = c.Phone,
                    Email = c.Email,
                    Address = c.Address,
                    IsPrimary = c.IsPrimary,
                    IsActive = c.IsActive
                })
                .ToList(),
            Sites = client.Sites
                .OrderByDescending(s => s.IsActive)
                .ThenBy(s => s.SiteName)
                .Select(MapToSiteDto)
                .ToList(),
            Bookings = client.Bookings
                .OrderByDescending(b => b.ScheduledDate)
                .Select(BookingManager.MapToDto)
                .ToList()
        };
    }
}
