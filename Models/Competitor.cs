using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PricePulse.Models
{
    public class Competitor
    {
        [Key]
        public int CompetitorId { get; set; }

        [Required]
        [ForeignKey("CompanyProfile")]
        public int CompanyProfileId { get; set; }

        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? CompetitorName { get; set; }

        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? CompetitorCompanyProfile { get; set; }

        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? CompetitorWebsite { get; set; }

        // Alias for CompetitorWebsite to match DAO expectations
        public string? CompetitorWebsiteUrl 
        { 
            get => CompetitorWebsite; 
            set => CompetitorWebsite = value; 
        }

        // Navigation properties
        public virtual CompanyProfile CompanyProfile { get; set; } = null!;
        public virtual ICollection<CompetitorProduct> CompetitorProducts { get; set; } = new List<CompetitorProduct>();
    }
}