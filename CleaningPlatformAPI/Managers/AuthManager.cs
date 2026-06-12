using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using CleaningPlatformAPI;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Managers;

public class AuthManager
{
    private const int MaxUsernameGenerationAttempts = 20;

    private readonly TokenManager _tokenManager;
    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthManager(TokenManager tokenManager, AppDbContext db, IConfiguration config, IStringLocalizer<SharedResources> localizer) { _tokenManager = tokenManager; _db = db; _config = config; 
            _localizer = localizer;}

    public async Task<OperationResult<string>> RegisterAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var roleName = request.Role.Trim();
        if (string.IsNullOrWhiteSpace(roleName))
            return OperationResult<string>.Fail("ROLE_REQUIRED", _localizer["err_role_required"]);

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName, ct);
        if (role is null)
            return OperationResult<string>.Fail("INVALID_ROLE_NAME", "Invalid role name.");

        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            return OperationResult<string>.Fail("NAME_REQUIRED", "First and last name are required.");

        if (!IsValidPassword(request.Password))
            return OperationResult<string>.Fail("PASSWORD_COMPLEXITY", "Password must be at least 8 characters and include at least one uppercase letter, one lowercase letter, and one digit.");

        var usernameBase = (firstName[..1] + lastName).ToLowerInvariant();
        var counter = 1;

        while (counter <= MaxUsernameGenerationAttempts)
        {
            var username = counter == 1 ? usernameBase : usernameBase + counter;
            var now = DateTime.UtcNow;
            var user = new Employee
            {
                Username = username,
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, GetBcryptWorkFactor()),
                RoleId = role.Id,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            _db.Employees.Add(user);
            try
            {
                await _db.SaveChangesAsync(ct);
                return OperationResult<string>.Ok("User created.");
            }
            catch (DbUpdateException ex) when (SqlHelper.IsUniqueConstraintViolation(ex))
            {
                _db.Entry(user).State = EntityState.Detached;
                counter++;
            }
        }

        return OperationResult<string>.Fail("USERNAME_GENERATION_FAILED", "Could not generate a unique username.");
    }

    public async Task<OperationResult<string>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var auth = await ValidateLoginAsync(request, ct);
        if (!auth.Success || auth.Data is null)
            return OperationResult<string>.Fail("INVALID_CREDENTIALS", auth.Message ?? "Invalid credentials.");

        var token = _tokenManager.CreateAdminToken(auth.Data.User, auth.Data.Permissions);
        return OperationResult<string>.Ok(token);
    }

    public async Task<OperationResult<List<Claim>>> GetClaimsAsync(LoginRequest request, CancellationToken ct = default)
    {
        var auth = await ValidateLoginAsync(request, ct);
        if (!auth.Success || auth.Data is null)
            return OperationResult<List<Claim>>.Fail("INVALID_CREDENTIALS", auth.Message ?? "Invalid credentials.");

        var user = auth.Data.User;
        var permissions = auth.Data.Permissions;
        var roleName = user.Role?.Name ?? string.Empty;

        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, roleName),
            new Claim("security_stamp", user.SecurityStamp)
        ];

        if (roleName != RoleNames.Owner)
        {
            foreach (var permission in permissions)
                claims.Add(new Claim("permission", permission));
        }

        return OperationResult<List<Claim>>.Ok(claims);
    }

    private async Task<OperationResult<LoginContext>> ValidateLoginAsync(LoginRequest request, CancellationToken ct)
    {
        var username = request.Username.Trim();
        var user = await _db.Employees
            .Include(e => e.Role)
            .FirstOrDefaultAsync(u => u.Username == username, ct);

        if (user is null || !user.IsActive)
            return OperationResult<LoginContext>.Fail("INVALID_CREDENTIALS", _localizer["err_invalid_credentials"]);

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return OperationResult<LoginContext>.Fail("INVALID_CREDENTIALS", _localizer["err_invalid_credentials"]);

        var permissions = await _db.RolePermissions
            .Where(rp => rp.RoleId == user.RoleId)
            .Select(rp => rp.PermissionKey)
            .ToListAsync(ct);

        return OperationResult<LoginContext>.Ok(new LoginContext(user, permissions));
    }

    public async Task<bool> ValidateSecurityStampAsync(int userId, string stamp, CancellationToken ct = default)
    {
        var user = await _db.Employees.FindAsync([userId], ct);
        return user is not null && user.SecurityStamp == stamp;
    }

    public async Task<OperationResult<string>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return OperationResult<string>.Fail("NEW_PASSWORD_REQUIRED", "New password is required.");
        if (!IsValidPassword(request.NewPassword))
            return OperationResult<string>.Fail("PASSWORD_COMPLEXITY", "New password must be at least 8 characters and include at least one uppercase letter, one lowercase letter, and one digit.");

        var user = await _db.Employees.FirstOrDefaultAsync(e => e.Id == request.UserId, ct);
        if (user is null)
            return OperationResult<string>.Fail("USER_NOT_FOUND", "User not found.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, GetBcryptWorkFactor());
        user.SecurityStamp = Guid.NewGuid().ToString();
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return OperationResult<string>.Ok("Password reset.");
    }

    public async Task<OperationResult<string>> ChangePasswordAsync(ChangePasswordRequest request, int requestingUserId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            return OperationResult<string>.Fail("CURRENT_PASSWORD_REQUIRED", "Current password is required.");
        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return OperationResult<string>.Fail("NEW_PASSWORD_REQUIRED", "New password is required.");
        if (!IsValidPassword(request.NewPassword))
            return OperationResult<string>.Fail("PASSWORD_COMPLEXITY", "New password must be at least 8 characters and include at least one uppercase letter, one lowercase letter, and one digit.");

        var user = await _db.Employees.FirstOrDefaultAsync(e => e.Id == requestingUserId, ct);
        if (user is null)
            return OperationResult<string>.Fail("USER_NOT_FOUND", "User not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return OperationResult<string>.Fail("CURRENT_PASSWORD_INCORRECT", "Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, GetBcryptWorkFactor());
        user.SecurityStamp = Guid.NewGuid().ToString();
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return OperationResult<string>.Ok("Password changed.");
    }

    // Fallback of 12 intentionally matches the default in appsettings.json
    private int GetBcryptWorkFactor()
    {
        var configured = _config.GetValue<int?>("Security:BcryptWorkFactor");
        return configured is > 3 and <= 31 ? configured.Value : 12;
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

    private record LoginContext(Employee User, List<string> Permissions);
}
