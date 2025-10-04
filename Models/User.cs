using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PricePulse.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public DateTime RegistrationDate { get; set; }

        public DateTime? LastLoginDate { get; set; }

        [Required]
        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string AccountStatus { get; set; } = "Active";

        [StringLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string? AvatarPath { get; set; }

        // Navigation properties
        public virtual Profile? Profile { get; set; }
        public virtual Contact? Contact { get; set; }
        public virtual Authentication? Authentication { get; set; }
        public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
        public virtual ICollection<RegistrationVerification> RegistrationVerifications { get; set; } = new List<RegistrationVerification>();
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<OwnProduct> OwnProducts { get; set; } = new List<OwnProduct>();
        public virtual ICollection<CompanyProfile> CompanyProfiles { get; set; } = new List<CompanyProfile>();
    }
}