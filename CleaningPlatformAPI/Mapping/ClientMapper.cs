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

    public static ClientResponse ToProfileResponse(Client client, List<Booking>? recentBookings = null)
    {
        var primary = client.Contacts.FirstOrDefault(c => c.IsPrimary && c.IsActive)
            ?? client.Contacts.FirstOrDefault(c => c.IsActive);

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
            PrimaryContactName = primary?.ContactName,
            PrimaryContactPhone = primary?.Phone,
            PrimaryContactEmail = primary?.Email,
            TotalBookings = client.Bookings?.Count ?? 0,
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
            Bookings = (recentBookings ?? client.Bookings?.ToList() ?? [])
                .OrderByDescending(b => b.ScheduledDate)
                .Select(BookingMapper.ToResponse)
                .ToList()
        };
    }
}
