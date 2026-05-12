using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Mapping;

public static class RoleMapper
{
    public static RoleResponse ToResponse(Role r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        IsProtected = r.IsProtected,
        Permissions = r.Permissions.Select(p => p.PermissionKey).ToList()
    };

    public static AvailablePermissionResponse ToPermissionResponse(string key, (string DisplayName, string Description, string Category) meta) => new()
    {
        Key = key,
        DisplayName = meta.DisplayName,
        Description = meta.Description,
        Category = meta.Category
    };
}
