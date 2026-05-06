namespace VechileCleaningAPI.Dtos;

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}
