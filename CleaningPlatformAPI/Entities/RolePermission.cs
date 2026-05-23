using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities
{
    [Table("RolePermissions")]
    public class RolePermission
    {
        [Key]
        public int Id { get; set; }

        public int RoleId { get; set; }

        [Required, MaxLength(100)]
        public string PermissionKey { get; set; } = string.Empty;

        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; } = null!;
    }
}
