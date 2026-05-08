using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Dtos;
using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Managers;

public class AuthManager
{
    private readonly TokenManager _tokenManager;
    private readonly AppDbContext _db;

    public AuthManager(TokenManager tokenManager, AppDbContext db)
    {
        _tokenManager = tokenManager;
        _db = db;
    }

    public async Task<OperationResult<string>> RegisterAsync(CreateUserDto dto)
    {
        var email = dto.Email.Trim();
        if (string.IsNullOrWhiteSpace(email))
            return OperationResult<string>.Fail("Email is required.");

        var existing = await _db.Employees.FirstOrDefaultAsync(u => u.Email == email);
        if (existing != null)
            return OperationResult<string>.Fail("Email already taken.");

        var roleName = dto.Role.Trim();
        if (string.IsNullOrWhiteSpace(roleName))
            return OperationResult<string>.Fail("Role is required.");

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role == null)
            return OperationResult<string>.Fail("Invalid role name.");

        var firstName = dto.FirstName.Trim();
        var lastName = dto.LastName.Trim();
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            return OperationResult<string>.Fail("First and last name are required.");

        var user = new Employee
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = role.Id,        // Use RoleId, not a string
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Employees.Add(user);
        await _db.SaveChangesAsync();
        return OperationResult<string>.Ok("User created.");
    }

    public async Task<OperationResult<string>> LoginAsync(LoginDto dto)
    {
        var email = dto.Email.Trim();
        var user = await _db.Employees
            .Include(e => e.Role)       // Include the navigation property
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !user.IsActive)
            return OperationResult<string>.Fail("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return OperationResult<string>.Fail("Invalid credentials.");

        //  Use Role.Id to get permissions
        var permissions = await _db.RolePermissions
            .Where(rp => rp.RoleId == user.RoleId)
            .Select(rp => rp.PermissionKey)
            .ToListAsync();

        var token = _tokenManager.CreateToken(user, permissions);
        return OperationResult<string>.Ok(token);
    }
}