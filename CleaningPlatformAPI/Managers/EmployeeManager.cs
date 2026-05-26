using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Mapping;

namespace CleaningPlatformAPI.Managers;

public class EmployeeManager
{
    private readonly AppDbContext _db;

    public EmployeeManager(AppDbContext db) { _db = db; }

    public async Task<List<UserResponse>> GetAllUsersAsync(CancellationToken ct = default)
    {
        var users = await _db.Employees
            .Include(e => e.Role)
            .OrderBy(u => u.Role!.Name)
            .ThenBy(u => u.LastName)
            .ToListAsync(ct);

        var rolePermissions = await _db.Roles
            .Include(r => r.Permissions)
            .ToDictionaryAsync(r => r.Name, r => r.Permissions.Select(p => p.PermissionKey).ToList(), ct);

        return users.Select(u => UserMapper.ToResponse(u, rolePermissions.GetValueOrDefault(u.Role?.Name ?? string.Empty, []))).ToList();
    }

    public async Task<OperationResult<UserResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await _db.Employees
            .Include(e => e.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
            return OperationResult<UserResponse>.Fail($"User #{id} was not found.");

        var permissions = await _db.RolePermissions
            .Where(rp => rp.RoleId == user.RoleId)
            .Select(rp => rp.PermissionKey)
            .ToListAsync(ct);

        return OperationResult<UserResponse>.Ok(UserMapper.ToResponse(user, permissions));
    }

    public async Task<List<EmployeeSimpleResponse>> GetActiveEmployeesAsync(CancellationToken ct = default)
    {
        var employees = await _db.Employees
            .Include(e => e.Role)
            .Where(e => e.IsActive)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync(ct);

        return employees.Select(UserMapper.ToSimpleResponse).ToList();
    }

    public async Task<OperationResult<UserResponse>> UpdateEmployeeAsync(int id, UpdateEmployeeRequest request, int requestingUserId, CancellationToken ct = default)
    {
        var user = await _db.Employees
            .Include(e => e.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
            return OperationResult<UserResponse>.Fail($"Employee #{id} was not found.");

        var roleName = request.Role?.Trim();
        if (string.IsNullOrWhiteSpace(roleName))
            return OperationResult<UserResponse>.Fail("Role is required.");

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName, ct);
        if (role is null)
            return OperationResult<UserResponse>.Fail($"Role '{roleName}' was not found.");

        if (id == requestingUserId && role.Id != user.RoleId)
            return OperationResult<UserResponse>.Fail("You cannot change your own role.");

        user.RoleId = role.Id;
        user.HourlyRate = request.HourlyRate;
        user.MaxJobsPerDay = request.MaxJobsPerDay;
        user.EmployeeCode = request.EmployeeCode?.Trim();
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var permissions = await _db.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .Select(rp => rp.PermissionKey)
            .ToListAsync(ct);

        return OperationResult<UserResponse>.Ok(UserMapper.ToResponse(user, permissions));
    }

    public async Task<OperationResult<UserResponse>> ToggleActiveAsync(int id, int requestingUserId, CancellationToken ct = default)
    {
        if (id == requestingUserId)
            return OperationResult<UserResponse>.Fail("You cannot deactivate your own account.");

        var user = await _db.Employees
            .Include(e => e.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
            return OperationResult<UserResponse>.Fail("User not found.");

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var permissions = await _db.RolePermissions
            .Where(rp => rp.RoleId == user.RoleId)
            .Select(rp => rp.PermissionKey)
            .ToListAsync(ct);

        return OperationResult<UserResponse>.Ok(UserMapper.ToResponse(user, permissions));
    }
}