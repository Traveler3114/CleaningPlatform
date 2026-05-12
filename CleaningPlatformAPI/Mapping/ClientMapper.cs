using System.Linq.Expressions;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Mapping;

public static class ClientMapper
{
    public static SiteResponse ToSiteResponse(Site site) => new()
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

    public static Expression<Func<Site, SiteResponse>> ToSiteResponseExpression() => site => new SiteResponse
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

    public static ClientResponse ToProfileResponse(Client client)
    {
        return new ClientResponse
        {
            Id = client.Id,
            ClientName = client.ClientName,
            Type = client.Type,
            Oib = client.Oib,
            PaymentTerms = client.PaymentTerms,
            Notes = client.Notes,
            IsActive = client.IsActive,
            CreatedAt = client.CreatedAt,
            PrimaryContactName = client.Contacts.FirstOrDefault(c => c.IsPrimary && c.IsActive)?.ContactName
                ?? client.Contacts.FirstOrDefault(c => c.IsActive)?.ContactName,
            PrimaryContactPhone = client.Contacts.FirstOrDefault(c => c.IsPrimary && c.IsActive)?.Phone
                ?? client.Contacts.FirstOrDefault(c => c.IsActive)?.Phone,
            PrimaryContactEmail = client.Contacts.FirstOrDefault(c => c.IsPrimary && c.IsActive)?.Email
                ?? client.Contacts.FirstOrDefault(c => c.IsActive)?.Email,
            TotalBookings = client.Bookings.Count,
            Contacts = client.Contacts
                .OrderByDescending(c => c.IsPrimary)
                .ThenByDescending(c => c.IsActive)
                .ThenBy(c => c.ContactName)
                .Select(c => new ContactResponse
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
                .Select(ToSiteResponse)
                .ToList(),
            Bookings = client.Bookings
                .OrderByDescending(b => b.ScheduledDate)
                .Select(BookingMapper.ToResponse)
                .ToList()
        };
    }
}
