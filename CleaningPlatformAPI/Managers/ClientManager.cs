// CleaningPlatformAPI/Managers/ClientManager.cs

using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Dtos;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Common;

namespace CleaningPlatformAPI.Managers;

public class ClientManager
{
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
            .Include(c => c.Bookings)
                .ThenInclude(b => b.BookingServices)
                    .ThenInclude(bs => bs.ServiceCatalog)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null) return null;

        var profile = new ClientProfileDto
        {
            Id = client.Id,
            ClientName = client.ClientName,
            Type = client.Type,
            Oib = client.Oib,
            PaymentTerms = client.PaymentTerms,
            IsActive = client.IsActive,
            CreatedAt = client.CreatedAt,
            PrimaryContactName = client.Contacts
                .FirstOrDefault(c => c.IsPrimary)?.ContactName,
            PrimaryContactPhone = client.Contacts
                .FirstOrDefault(c => c.IsPrimary)?.Phone,
            PrimaryContactEmail = client.Contacts
                .FirstOrDefault(c => c.IsPrimary)?.Email,
            TotalBookings = client.Bookings.Count,
            Contacts = client.Contacts.Select(c => new ContactDto
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
            }).ToList(),
            Bookings = client.Bookings
                .OrderByDescending(b => b.ScheduledDate)
                .Select(b => BookingManager.MapToDto(b))
                .ToList()
        };

        return profile;
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
            Notes = dto.Notes,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            Contacts = new List<Contact>
            {
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
            }
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
            IsActive = client.IsActive,
            CreatedAt = client.CreatedAt,
            PrimaryContactName = dto.PrimaryContactName,
            PrimaryContactPhone = dto.PrimaryContactPhone,
            PrimaryContactEmail = dto.PrimaryContactEmail,
            TotalBookings = 0
        };

        return OperationResult<ClientDto>.Ok(resultDto);
    }

    public async Task<OperationResult<ClientDto>> UpdateTypeAsync(int id, string newType)
    {
        var validTypes = new[] { "OneTime", "RepeatIndividual", "RepeatBusiness" };
        if (!validTypes.Contains(newType))
            return OperationResult<ClientDto>.Fail("Invalid client type.");

        var client = await _db.Clients.FindAsync(id);
        if (client == null)
            return OperationResult<ClientDto>.Fail("Client not found.");

        client.Type = newType;
        client.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var dto = new ClientDto
        {
            Id = client.Id,
            ClientName = client.ClientName,
            Type = client.Type,
            Oib = client.Oib,
            PaymentTerms = client.PaymentTerms,
            IsActive = client.IsActive,
            CreatedAt = client.CreatedAt,
            TotalBookings = await _db.Bookings.CountAsync(b => b.ClientId == client.Id)
        };

        return OperationResult<ClientDto>.Ok(dto);
    }
}