using Microsoft.EntityFrameworkCore;
using VechileCleaningAPI.Common;
using VechileCleaningAPI.Data;
using VechileCleaningAPI.Dtos;

namespace VechileCleaningAPI.Managers;

public class UserManager
{
    private readonly AppDbContext _db;

    public UserManager(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _db.Users
            .OrderBy(u => u.Role)
            .ThenBy(u => u.Surname)
            .ToListAsync();

        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        return user == null ? null : MapToDto(user);
    }

    public async Task<OperationResult<UserDto>> ToggleActiveAsync(int id, int requestingUserId)
    {
        if (id == requestingUserId)
            return OperationResult<UserDto>.Fail("You cannot deactivate your own account.");

        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return OperationResult<UserDto>.Fail("User not found.");

        user.IsActive = !user.IsActive;
        await _db.SaveChangesAsync();

        return OperationResult<UserDto>.Ok(MapToDto(user));
    }

    private static UserDto MapToDto(Entities.User u) => new()
    {
        Id = u.Id,
        Name = u.Name,
        Surname = u.Surname,
        Username = u.Username,
        Role = u.Role.ToString(),
        IsActive = u.IsActive,
        CreatedAt = u.CreatedAt
    };
}
