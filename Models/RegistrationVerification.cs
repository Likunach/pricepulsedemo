using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PricePulse.Models
{
    public class RegistrationVerification
    {
        [Key]
        public int VerificationId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [StringLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string VerificationToken { get; set; } = string.Empty;

        [Required]
        public DateTime TokenExpiry { get; set; }

        // Alias for TokenExpiry to match DAO expectations
        public DateTime? ExpiryDate 
        { 
            get => TokenExpiry; 
            set => TokenExpiry = value ?? DateTime.MinValue; 
        }

        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string? VerificationStatus { get; set; }

        // Add properties expected by DAOs
        public string? VerificationCode 
        { 
            get => VerificationToken; 
            set => VerificationToken = value ?? string.Empty; 
        }

        public bool IsVerified 
        { 
            get => VerificationStatus == "Verified"; 
            set => VerificationStatus = value ? "Verified" : "Pending"; 
        }

        // Add CreatedDate property expected by DAOs
        public DateTime? CreatedDate { get; set; }

        // Add VerifiedDate property expected by DAOs
        public DateTime? VerifiedDate { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}