using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PricePulse.Models
{
    public class Profile
    {
        [Key]
        public int ProfileId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? FirstName { get; set; }

        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? LastName { get; set; }

        [Column(TypeName = "text")]
        public string? Bio { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}