using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PricePulse.Models
{
    public class Contact
    {
        [Key]
        public int ContactId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? SecondaryEmail { get; set; }

        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string? PreferredContactMethod { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}