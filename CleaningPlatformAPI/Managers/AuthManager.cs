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
        var existing = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (existing != null)
            return OperationResult<string>.Fail("Username already taken.");

        var roleExists = await _db.Roles.AnyAsync(r => r.Name == dto.RoleName);
        if (!roleExists)
            return OperationResult<string>.Fail("Invalid role name.");

        var user = new Employee
        {
            Username = dto.Username,
            Name = dto.Name,
            Surname = dto.Surname,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleName = dto.RoleName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return OperationResult<string>.Ok("User created.");
    }

    public async Task<OperationResult<string>> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user == null || !user.IsActive)
            return OperationResult<string>.Fail("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return OperationResult<string>.Fail("Invalid credentials.");

        var permissions = await _db.Roles
            .Where(r => r.Name == user.RoleName)
            .SelectMany(r => r.Permissions)
            .Select(p => p.PermissionKey)
            .ToListAsync();

        var token = _tokenManager.CreateToken(user, permissions);
        return OperationResult<string>.Ok(token);
    }
}
