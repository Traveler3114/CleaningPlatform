using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities
{
    [Table("Roles")]
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsProtected { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
    }
}
