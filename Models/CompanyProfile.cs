using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PricePulse.Models
{
    public class CompanyProfile
    {
        [Key]
        public int CompanyProfileId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? CompanyName { get; set; }

        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? CompanyProfileName { get; set; }

        [Column(TypeName = "text")]
        public string? Summary { get; set; }

        // Alias for Summary to match DAO expectations
        public string? CompanyDescription 
        { 
            get => Summary; 
            set => Summary = value; 
        }

        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? CompanyWebsite { get; set; }

        // Add Industry property expected by DAOs
        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? Industry { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Address? Address { get; set; }
        public virtual ICollection<Competitor> Competitors { get; set; } = new List<Competitor>();
        public virtual ICollection<OwnProduct> OwnProducts { get; set; } = new List<OwnProduct>();
    }
}