namespace CleaningPlatformAPI.Dtos;

public class UpdateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}
