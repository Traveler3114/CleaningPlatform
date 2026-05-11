namespace CleaningPlatformAPI.Dtos;

public class AssignedEmployeeDto
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int AssignmentId { get; set; }
}
