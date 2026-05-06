namespace VechileCleaningAPI.Entities;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsProtected { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<RolePermission> Permissions { get; set; } = new();
}
