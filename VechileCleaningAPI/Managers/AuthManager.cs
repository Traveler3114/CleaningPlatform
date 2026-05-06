using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VechileCleaningAPI.Common;
using VechileCleaningAPI.Data;
using VechileCleaningAPI.Dtos;
using VechileCleaningAPI.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VechileCleaningAPI.Managers;

public class AuthManager
{
    private readonly TokenManager _tokenManager;
    private readonly AppDbContext _db;

    public AuthManager(TokenManager tokenManager,AppDbContext db)
    {
        _tokenManager = tokenManager;
        _db = db;
    }



    public async Task<OperationResult<string>> RegisterAsync(CreateUserDto dto)
    {
        var existing = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (existing != null)
            return OperationResult<string>.Fail("Username already taken.");

        var user = new User
        {
            Username = dto.Username,
            Name = dto.Name,
            Surname = dto.Surname,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
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

        var token = _tokenManager.CreateToken(user);
        return OperationResult<string>.Ok(token);
    }
}
