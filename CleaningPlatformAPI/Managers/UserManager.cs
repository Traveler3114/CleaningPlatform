using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Dtos;

namespace CleaningPlatformAPI.Managers;

public class UserManager
{
    private readonly AppDbContext _db;

    public UserManager(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _db.Users
            .OrderBy(u => u.RoleName)
            .ThenBy(u => u.Surname)
            .ToListAsync();

        var rolePermissions = await _db.Roles
            .Include(r => r.Permissions)
            .ToDictionaryAsync(r => r.Name, r => r.Permissions.Select(p => p.PermissionKey).ToList());

        return users.Select(u => MapToDto(u, rolePermissions.GetValueOrDefault(u.RoleName, new List<string>()))).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return null;

        var permissions = await _db.Roles
            .Where(r => r.Name == user.RoleName)
            .SelectMany(r => r.Permissions)
            .Select(p => p.PermissionKey)
            .ToListAsync();

        return MapToDto(user, permissions);
    }

    public async Task<OperationResult<UserDto>> ToggleActiveAsync(int id, int requestingUserId)
    {
        if (id == requestingUserId)
            return OperationResult<UserDto>.Fail("You cannot deactivate your own account.");

        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return OperationResult<UserDto>.Fail("User not found.");

        user.IsActive = !user.IsActive;
        await _db.SaveChangesAsync();

        var permissions = await _db.Roles
            .Where(r => r.Name == user.RoleName)
            .SelectMany(r => r.Permissions)
            .Select(p => p.PermissionKey)
            .ToListAsync();

        return OperationResult<UserDto>.Ok(MapToDto(user, permissions));
    }

    private static UserDto MapToDto(Entities.User u, List<string> permissions) => new()
    {
        Id = u.Id,
        Name = u.Name,
        Surname = u.Surname,
        Username = u.Username,
        RoleName = u.RoleName,
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt,
        Permissions = permissions
    };
}
