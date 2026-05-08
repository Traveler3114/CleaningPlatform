using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Dtos;
using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Managers;

public class RoleManager
{
    private readonly AppDbContext _db;

    public RoleManager(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _db.Roles
            .Include(r => r.Permissions)
            .OrderBy(r => r.Name)
            .ToListAsync();

        return roles.Select(MapToDto).ToList();
    }

    public async Task<RoleDto?> GetByIdAsync(int id)
    {
        var role = await _db.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id);

        return role == null ? null : MapToDto(role);
    }

    public List<AvailablePermissionDto> GetAvailablePermissions()
    {
        return PermissionKeys.All.Select(key =>
        {
            var meta = PermissionKeys.Meta[key];
            return new AvailablePermissionDto
            {
                Key = key,
                DisplayName = meta.DisplayName,
                Description = meta.Description,
                Category = meta.Category
            };
        }).ToList();
    }

    public async Task<OperationResult<RoleDto>> CreateRoleAsync(CreateRoleDto dto)
    {
        var trimmedName = dto.Name.Trim();
        if (string.IsNullOrEmpty(trimmedName))
            return OperationResult<RoleDto>.Fail("Role name is required.");

        if (await _db.Roles.AnyAsync(r => r.Name == trimmedName))
            return OperationResult<RoleDto>.Fail("A role with this name already exists.");

        var invalidKeys = dto.Permissions.Except(PermissionKeys.All).ToList();
        if (invalidKeys.Count > 0)
            return OperationResult<RoleDto>.Fail($"Invalid permission keys: {string.Join(", ", invalidKeys)}");

        var role = new Role
        {
            Name = trimmedName,
            IsProtected = false,
            CreatedAt = DateTime.UtcNow,
            Permissions = dto.Permissions.Distinct().Select(k => new RolePermission { PermissionKey = k }).ToList()
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync();

        return OperationResult<RoleDto>.Ok(MapToDto(role));
    }

    public async Task<OperationResult<RoleDto>> UpdateRoleAsync(int id, UpdateRoleDto dto)
    {
        var role = await _db.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
            return OperationResult<RoleDto>.Fail("Role not found.");

        if (role.IsProtected)
            return OperationResult<RoleDto>.Fail("This role is protected and cannot be modified.");

        var trimmedName = dto.Name.Trim();
        if (string.IsNullOrEmpty(trimmedName))
            return OperationResult<RoleDto>.Fail("Role name is required.");

        if (await _db.Roles.AnyAsync(r => r.Name == trimmedName && r.Id != id))
            return OperationResult<RoleDto>.Fail("A role with this name already exists.");

        var invalidKeys = dto.Permissions.Except(PermissionKeys.All).ToList();
        if (invalidKeys.Count > 0)
            return OperationResult<RoleDto>.Fail($"Invalid permission keys: {string.Join(", ", invalidKeys)}");

        role.Name = trimmedName;
        _db.RolePermissions.RemoveRange(role.Permissions);
        role.Permissions = dto.Permissions.Distinct().Select(k => new RolePermission { PermissionKey = k, RoleId = role.Id }).ToList();

        await _db.SaveChangesAsync();

        return OperationResult<RoleDto>.Ok(MapToDto(role));
    }

    public async Task<OperationResult<string>> DeleteRoleAsync(int id)
    {
        var role = await _db.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
            return OperationResult<string>.Fail("Role not found.");

        if (role.IsProtected)
            return OperationResult<string>.Fail("This role is protected and cannot be deleted.");

        // ✅ Fixed: compare role.Name with employee's role name
        var assignedUsers = await _db.Employees
            .Include(e => e.Role)
            .Where(e => e.Role != null && e.Role.Name == role.Name)
            .ToListAsync();

        if (assignedUsers.Count > 0)
            return OperationResult<string>.Fail($"{assignedUsers.Count} user(s) are assigned to this role. Reassign them before deleting.");

        _db.RolePermissions.RemoveRange(role.Permissions);
        _db.Roles.Remove(role);
        await _db.SaveChangesAsync();

        return OperationResult<string>.Ok("Role deleted.");
    }

    public async Task<List<string>> GetRolePermissionsAsync(string roleName)
    {
        var role = await _db.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Name == roleName);

        return role?.Permissions.Select(p => p.PermissionKey).ToList() ?? new List<string>();
    }

    private static RoleDto MapToDto(Role r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        IsProtected = r.IsProtected,
        Permissions = r.Permissions.Select(p => p.PermissionKey).ToList()
    };
}