using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PricePulse.Models
{
    public class UserRole
    {
        [Key]
        public int UserRoleId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string RoleName { get; set; } = string.Empty;

        public DateTime? AssignedDate { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}