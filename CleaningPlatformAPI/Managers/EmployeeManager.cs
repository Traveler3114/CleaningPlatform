using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using CleaningPlatformAPI;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Enums;
using CleaningPlatformAPI.Mapping;

namespace CleaningPlatformAPI.Managers;

public class EmployeeManager
{
    private readonly AppDbContext _db;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public EmployeeManager(AppDbContext db, IStringLocalizer<SharedResources> localizer) { _db = db; 
            _localizer = localizer;}

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
            return OperationResult<UserResponse>.Fail("USER_NOT_FOUND", $"User #{id} was not found.");

        var permissions = await _db.RolePermissions
            .Where(rp => rp.RoleId == user.RoleId)
            .Select(rp => rp.PermissionKey)
            .ToListAsync(ct);

        return OperationResult<UserResponse>.Ok(UserMapper.ToResponse(user, permissions));
    }

    public async Task<List<AvailableEmployeeResponse>> GetAvailableForBookingAsync(int bookingId, CancellationToken ct = default)
    {
        var booking = await _db.Bookings.FindAsync([bookingId], ct);
        if (booking is null) return [];

        var assignedIds = await _db.BookingAssignments
            .Where(a => a.BookingId == bookingId)
            .Select(a => a.EmployeeId)
            .ToListAsync(ct);

        var todayJobs = await _db.BookingAssignments
            .Where(a => a.Booking.ScheduledDate.Date == booking.ScheduledDate.Date
                     && a.Booking.Status != BookingStatus.Cancelled)
            .GroupBy(a => a.EmployeeId)
            .Select(g => new { EmployeeId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EmployeeId, x => x.Count, ct);

        var employees = await _db.Employees
            .Include(e => e.Role)
            .Where(e => e.IsActive && !assignedIds.Contains(e.Id))
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync(ct);

        return employees.Select(e =>
        {
            todayJobs.TryGetValue(e.Id, out var count);
            return new AvailableEmployeeResponse
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Role = e.Role?.Name ?? string.Empty,
                JobsToday = count,
                MaxJobsPerDay = e.MaxJobsPerDay,
                IsAvailable = !e.MaxJobsPerDay.HasValue || count < e.MaxJobsPerDay.Value
            };
        }).ToList();
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
            return OperationResult<UserResponse>.Fail("EMPLOYEE_NOT_FOUND", $"Employee #{id} was not found.");

        var roleName = request.Role?.Trim();
        if (string.IsNullOrWhiteSpace(roleName))
            return OperationResult<UserResponse>.Fail("ROLE_REQUIRED", "Role is required.");

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName, ct);
        if (role is null)
            return OperationResult<UserResponse>.Fail("ROLE_NOT_FOUND", $"Role '{roleName}' was not found.");

        if (id == requestingUserId && role.Id != user.RoleId)
            return OperationResult<UserResponse>.Fail("CANNOT_CHANGE_OWN_ROLE", "You cannot change your own role.");

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
            return OperationResult<UserResponse>.Fail("CANNOT_DEACTIVATE_SELF", "You cannot deactivate your own account.");

        var user = await _db.Employees
            .Include(e => e.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
            return OperationResult<UserResponse>.Fail("USER_NOT_FOUND", "User not found.");

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