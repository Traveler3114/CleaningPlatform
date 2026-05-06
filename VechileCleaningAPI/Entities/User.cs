
namespace VechileCleaningAPI.Entities;

public enum UserRole { Owner, Dispatcher, Cleaner, Finance, Client }

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    

}
