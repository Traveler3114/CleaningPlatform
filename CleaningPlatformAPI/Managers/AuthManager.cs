using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

        var usernameBase = (firstName[..1] + lastName).ToLowerInvariant();
        var username = usernameBase;
        var counter = 2;
        while (await _db.Employees.AnyAsync(u => u.Username == username))
        {
            username = usernameBase + counter;
            counter++;
        }

        var user = new Employee
        {
            Username = username,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = role.Id,
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
        var auth = await ValidateLoginAsync(dto);
        if (!auth.Success || auth.Data is null)
            return OperationResult<string>.Fail(auth.Message ?? "Invalid credentials.");

        var user = auth.Data.User;
        var permissions = auth.Data.Permissions;
        var token = _tokenManager.CreateToken(user, permissions);
        return OperationResult<string>.Ok(token);
    }

    public async Task<OperationResult<List<Claim>>> GetClaimsAsync(LoginDto dto)
    {
        var auth = await ValidateLoginAsync(dto);
        if (!auth.Success || auth.Data is null)
            return OperationResult<List<Claim>>.Fail(auth.Message ?? "Invalid credentials.");

        var user = auth.Data.User;
        var permissions = auth.Data.Permissions;
        var roleName = user.Role?.Name ?? string.Empty;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, roleName),
            new Claim("security_stamp", user.SecurityStamp)
        };

        if (roleName != "Owner")
        {
            foreach (var permission in permissions)
                claims.Add(new Claim("permission", permission));
        }

        return OperationResult<List<Claim>>.Ok(claims);
    }

    private async Task<OperationResult<LoginContext>> ValidateLoginAsync(LoginDto dto)
    {
        var username = dto.Username.Trim();
        var user = await _db.Employees
            .Include(e => e.Role)
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null || !user.IsActive)
            return OperationResult<LoginContext>.Fail("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return OperationResult<LoginContext>.Fail("Invalid credentials.");

        //  Use Role.Id to get permissions
        var permissions = await _db.RolePermissions
            .Where(rp => rp.RoleId == user.RoleId)
            .Select(rp => rp.PermissionKey)
            .ToListAsync();

        return OperationResult<LoginContext>.Ok(new LoginContext
        {
            User = user,
            Permissions = permissions
        });
    }

    public async Task<OperationResult<string>> ResetPasswordAsync(ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NewPassword))
            return OperationResult<string>.Fail("New password is required.");
        if (!IsValidPassword(dto.NewPassword))
            return OperationResult<string>.Fail("New password must be at least 8 characters and include uppercase, lowercase, and a number.");

        var user = await _db.Employees.FirstOrDefaultAsync(e => e.Id == dto.UserId);
        if (user == null)
            return OperationResult<string>.Fail("User not found.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.SecurityStamp = Guid.NewGuid().ToString();
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return OperationResult<string>.Ok("Password reset.");
    }

    public async Task<OperationResult<string>> ChangePasswordAsync(ChangePasswordDto dto, int requestingUserId)
    {
        if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
            return OperationResult<string>.Fail("Current password is required.");
        if (string.IsNullOrWhiteSpace(dto.NewPassword))
            return OperationResult<string>.Fail("New password is required.");
        if (!IsValidPassword(dto.NewPassword))
            return OperationResult<string>.Fail("New password must be at least 8 characters and include uppercase, lowercase, and a number.");

        var user = await _db.Employees.FirstOrDefaultAsync(e => e.Id == requestingUserId);
        if (user == null)
            return OperationResult<string>.Fail("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return OperationResult<string>.Fail("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.SecurityStamp = Guid.NewGuid().ToString();
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return OperationResult<string>.Ok("Password changed.");
    }

    private static bool IsValidPassword(string password)
    {
        if (password.Length < 8)
            return false;

        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);

        return hasUpper && hasLower && hasDigit;
    }

    private sealed class LoginContext
    {
        public required Employee User { get; init; }
        public required List<string> Permissions { get; init; }
    }
}
