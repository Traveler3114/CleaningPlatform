using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Mapping;

public static class UserMapper
{
    public static UserResponse ToResponse(Employee u, List<string> permissions) => new()
    {
        Id = u.Id,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Username = u.Username,
        Role = u.Role?.Name ?? string.Empty,
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt,
        Permissions = permissions
    };

    public static EmployeeSimpleResponse ToSimpleResponse(Employee e) => new()
    {
        Id = e.Id,
        FirstName = e.FirstName,
        LastName = e.LastName,
        Role = e.Role?.Name ?? string.Empty
    };
}
