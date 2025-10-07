using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PricePulse.Models
{
    public class Session
    {
        [Key]
        [StringLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string SessionId { get; set; } = string.Empty;

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        public DateTime? LoginTimestamp { get; set; }

        public DateTime? ExpiryTimestamp { get; set; }

        // Alias for ExpiryTimestamp to match DAO expectations
        public DateTime? ExpiryDate 
        { 
            get => ExpiryTimestamp; 
            set => ExpiryTimestamp = value; 
        }

        [StringLength(45)]
        [Column(TypeName = "varchar(45)")]
        public string? IpAddress { get; set; }

        [StringLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string? DeviceInfo { get; set; }

        // Add properties expected by DAOs
        public string? SessionToken 
        { 
            get => SessionId; 
            set => SessionId = value ?? string.Empty; 
        }

        public DateTime? CreatedDate 
        { 
            get => LoginTimestamp; 
            set => LoginTimestamp = value; 
        }

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}