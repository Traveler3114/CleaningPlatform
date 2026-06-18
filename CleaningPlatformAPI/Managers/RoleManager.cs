using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using CleaningPlatformAPI;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;
using CleaningPlatformAPI.Mapping;
using CleaningPlatformAPI.Common;

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

    public async Task<RoleResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var role = await _db.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
        return role is null
            ? throw new AppException("ROLE_NOT_FOUND", $"Role #{id} was not found.", 404)
            : RoleMapper.ToResponse(role);
    }

    public List<AvailablePermissionResponse> GetAvailablePermissions()
    {
        return PermissionKeys.All.Select(key =>
        {
            var meta = PermissionKeys.Meta[key];
            return RoleMapper.ToPermissionResponse(key, meta);
        }).ToList();
    }

    public async Task<RoleResponse> CreateRoleAsync(CreateRoleRequest dto, CancellationToken ct = default)
    {
        var trimmedName = dto.Name.Trim();
        if (string.IsNullOrEmpty(trimmedName))
            throw new AppException("ROLE_NAME_REQUIRED", _localizer["msg_role_name_required"], 422);

        if (await _db.Roles.AnyAsync(r => r.Name == trimmedName, ct))
            throw new AppException("ROLE_NAME_EXISTS", "A role with this name already exists.", 409);

        var invalidKeys = dto.Permissions.Except(PermissionKeys.All).ToList();
        if (invalidKeys.Count > 0)
            throw new AppException("INVALID_PERMISSION_KEYS", $"Invalid permission keys: {string.Join(", ", invalidKeys)}", 422);

        var role = new Role
        {
            Name = trimmedName,
            IsProtected = false,
            CreatedAt = DateTime.UtcNow,
            Permissions = dto.Permissions.Distinct().Select(k => new RolePermission { PermissionKey = k }).ToList()
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync(ct);

        return RoleMapper.ToResponse(role);
    }

    public async Task<RoleResponse> UpdateRoleAsync(int id, UpdateRoleRequest dto, CancellationToken ct = default)
    {
        var role = await _db.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (role is null)
            throw new AppException("ROLE_NOT_FOUND", "Role not found.", 404);

        if (role.IsProtected)
            throw new AppException("ROLE_PROTECTED", "This role is protected and cannot be modified.", 400);

        var trimmedName = dto.Name.Trim();
        if (string.IsNullOrEmpty(trimmedName))
            throw new AppException("ROLE_NAME_REQUIRED", _localizer["msg_role_name_required"], 422);

        if (await _db.Roles.AnyAsync(r => r.Name == trimmedName && r.Id != id, ct))
            throw new AppException("ROLE_NAME_EXISTS", "A role with this name already exists.", 409);

        var invalidKeys = dto.Permissions.Except(PermissionKeys.All).ToList();
        if (invalidKeys.Count > 0)
            throw new AppException("INVALID_PERMISSION_KEYS", $"Invalid permission keys: {string.Join(", ", invalidKeys)}", 422);

        role.Name = trimmedName;
        _db.RolePermissions.RemoveRange(role.Permissions);
        role.Permissions = dto.Permissions.Distinct().Select(k => new RolePermission { PermissionKey = k, RoleId = role.Id }).ToList();

        await _db.SaveChangesAsync(ct);

        return RoleMapper.ToResponse(role);
    }

    public async Task DeleteRoleAsync(int id, CancellationToken ct = default)
    {
        var role = await _db.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (role is null)
            throw new AppException("ROLE_NOT_FOUND", "Role not found.", 404);

        if (role.IsProtected)
            throw new AppException("ROLE_PROTECTED", "This role is protected and cannot be deleted.", 400);

        var assignedCount = await _db.Employees.CountAsync(e => e.RoleId == id, ct);

        if (assignedCount > 0)
            throw new AppException("ROLE_HAS_ASSIGNED_USERS", $"{assignedCount} user(s) are assigned to this role. Reassign them before deleting.", 400);

        _db.RolePermissions.RemoveRange(role.Permissions);
        _db.Roles.Remove(role);
        await _db.SaveChangesAsync(ct);

        return;
    }

    public async Task<List<string>> GetRolePermissionsAsync(string roleName, CancellationToken ct = default)
    {
        var role = await _db.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Name == roleName, ct);

        return role?.Permissions.Select(p => p.PermissionKey).ToList() ?? [];
    }
}