namespace VechileCleaningAPI.Dtos;

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsProtected { get; set; }
    public List<string> Permissions { get; set; } = new();
}
