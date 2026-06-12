using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using CleaningPlatformAPI;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Mapping;

namespace CleaningPlatformAPI.Managers;

public class RoleManager
{
    private readonly AppDbContext _db;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public RoleManager(AppDbContext db, IStringLocalizer<SharedResources> localizer) { _db = db; 
            _localizer = localizer;}

    public async Task<List<RoleResponse>> GetAllRolesAsync(CancellationToken ct = default)
    {
        var roles = await _db.Roles
            .Include(r => r.Permissions)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);

        return roles.Select(RoleMapper.ToResponse).ToList();
    }

    public async Task<OperationResult<RoleResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var role = await _db.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
        return role is null
            ? OperationResult<RoleResponse>.Fail("ROLE_NOT_FOUND", $"Role #{id} was not found.")
            : OperationResult<RoleResponse>.Ok(RoleMapper.ToResponse(role));
    }

    public List<AvailablePermissionResponse> GetAvailablePermissions()
    {
        return PermissionKeys.All.Select(key =>
        {
            var meta = PermissionKeys.Meta[key];
            return RoleMapper.ToPermissionResponse(key, meta);
        }).ToList();
    }

    public async Task<OperationResult<RoleResponse>> CreateRoleAsync(CreateRoleRequest dto, CancellationToken ct = default)
    {
        var trimmedName = dto.Name.Trim();
        if (string.IsNullOrEmpty(trimmedName))
            return OperationResult<RoleResponse>.Fail("ROLE_NAME_REQUIRED", _localizer["msg_role_name_required"]);

        if (await _db.Roles.AnyAsync(r => r.Name == trimmedName, ct))
            return OperationResult<RoleResponse>.Fail("ROLE_NAME_EXISTS", "A role with this name already exists.");

        var invalidKeys = dto.Permissions.Except(PermissionKeys.All).ToList();
        if (invalidKeys.Count > 0)
            return OperationResult<RoleResponse>.Fail("INVALID_PERMISSION_KEYS", $"Invalid permission keys: {string.Join(", ", invalidKeys)}");

        var role = new Role
        {
            Name = trimmedName,
            IsProtected = false,
            CreatedAt = DateTime.UtcNow,
            Permissions = dto.Permissions.Distinct().Select(k => new RolePermission { PermissionKey = k }).ToList()
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync(ct);

        return OperationResult<RoleResponse>.Ok(RoleMapper.ToResponse(role));
    }

    public async Task<OperationResult<RoleResponse>> UpdateRoleAsync(int id, UpdateRoleRequest dto, CancellationToken ct = default)
    {
        var role = await _db.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (role is null)
            return OperationResult<RoleResponse>.Fail("ROLE_NOT_FOUND", "Role not found.");

        if (role.IsProtected)
            return OperationResult<RoleResponse>.Fail("ROLE_PROTECTED", "This role is protected and cannot be modified.");

        var trimmedName = dto.Name.Trim();
        if (string.IsNullOrEmpty(trimmedName))
            return OperationResult<RoleResponse>.Fail("ROLE_NAME_REQUIRED", _localizer["msg_role_name_required"]);

        if (await _db.Roles.AnyAsync(r => r.Name == trimmedName && r.Id != id, ct))
            return OperationResult<RoleResponse>.Fail("ROLE_NAME_EXISTS", "A role with this name already exists.");

        var invalidKeys = dto.Permissions.Except(PermissionKeys.All).ToList();
        if (invalidKeys.Count > 0)
            return OperationResult<RoleResponse>.Fail("INVALID_PERMISSION_KEYS", $"Invalid permission keys: {string.Join(", ", invalidKeys)}");

        role.Name = trimmedName;
        _db.RolePermissions.RemoveRange(role.Permissions);
        role.Permissions = dto.Permissions.Distinct().Select(k => new RolePermission { PermissionKey = k, RoleId = role.Id }).ToList();

        await _db.SaveChangesAsync(ct);

        return OperationResult<RoleResponse>.Ok(RoleMapper.ToResponse(role));
    }

    public async Task<OperationResult<string>> DeleteRoleAsync(int id, CancellationToken ct = default)
    {
        var role = await _db.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (role is null)
            return OperationResult<string>.Fail("ROLE_NOT_FOUND", "Role not found.");

        if (role.IsProtected)
            return OperationResult<string>.Fail("ROLE_PROTECTED", "This role is protected and cannot be deleted.");

        var assignedCount = await _db.Employees.CountAsync(e => e.RoleId == id, ct);

        if (assignedCount > 0)
            return OperationResult<string>.Fail("ROLE_HAS_ASSIGNED_USERS", $"{assignedCount} user(s) are assigned to this role. Reassign them before deleting.");

        _db.RolePermissions.RemoveRange(role.Permissions);
        _db.Roles.Remove(role);
        await _db.SaveChangesAsync(ct);

        return OperationResult<string>.Ok("Role deleted.");
    }

    public async Task<List<string>> GetRolePermissionsAsync(string roleName, CancellationToken ct = default)
    {
        var role = await _db.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Name == roleName, ct);

        return role?.Permissions.Select(p => p.PermissionKey).ToList() ?? [];
    }
}