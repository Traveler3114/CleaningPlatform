using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Dtos;
using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Managers;

public class EmployeeManager
{
    private readonly AppDbContext _db;

    public EmployeeManager(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _db.Employees
            .Include(e => e.Role)   // ✅ Include Role
            .OrderBy(u => u.Role.Name)
            .ThenBy(u => u.LastName)
            .ToListAsync();

        var rolePermissions = await _db.Roles
            .Include(r => r.Permissions)
            .ToDictionaryAsync(r => r.Name, r => r.Permissions.Select(p => p.PermissionKey).ToList());

        return users.Select(u => MapToDto(u, rolePermissions.GetValueOrDefault(u.Role?.Name ?? string.Empty, new List<string>()))).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _db.Employees
            .Include(e => e.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return null;

        var permissions = await _db.RolePermissions
            .Where(rp => rp.RoleId == user.RoleId)
            .Select(rp => rp.PermissionKey)
            .ToListAsync();

        return MapToDto(user, permissions);
    }

    public async Task<OperationResult<UserDto>> ToggleActiveAsync(int id, int requestingUserId)
    {
        if (id == requestingUserId)
            return OperationResult<UserDto>.Fail("You cannot deactivate your own account.");

        var user = await _db.Employees
            .Include(e => e.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return OperationResult<UserDto>.Fail("User not found.");

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var permissions = await _db.RolePermissions
            .Where(rp => rp.RoleId == user.RoleId)
            .Select(rp => rp.PermissionKey)
            .ToListAsync();

        return OperationResult<UserDto>.Ok(MapToDto(user, permissions));
    }

    private static UserDto MapToDto(Employee u, List<string> permissions) => new()
    {
        Id = u.Id,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Email = u.Email,
        Role = u.Role?.Name ?? string.Empty,   // ✅ Extract role name from navigation property
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt,
        Permissions = permissions
    };
}