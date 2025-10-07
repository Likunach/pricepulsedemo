using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PricePulse.Models
{
    public class Authentication
    {
        [Key]
        public int AuthId { get; set; }

        // Alias for AuthId to match DAO expectations
        public int AuthenticationId 
        { 
            get => AuthId; 
            set => AuthId = value; 
        }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        public bool TwoFactorEnabled { get; set; } = false;

        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? RecoveryEmail { get; set; }

        [Column(TypeName = "json")]
        public string? SecurityQuestions { get; set; }

        // Add individual security question properties expected by DAOs
        public string? SecurityQuestion 
        { 
            get => SecurityQuestions; 
            set => SecurityQuestions = value; 
        }

        // Add security answer property expected by DAOs
        [StringLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string? SecurityAnswer { get; set; }

        public DateTime? LastPasswordChange { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}