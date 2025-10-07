using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PricePulse.Models
{
    public class OwnProduct
    {
        [Key]
        public int OwnProductId { get; set; }

        [Required]
        [ForeignKey("CompanyProfile")]
        public int CompanyProfileId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? ProductName { get; set; }

        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? ProductWebsiteUrl { get; set; }

        // Navigation properties
        public virtual CompanyProfile CompanyProfile { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<CompetitorProduct> CompetitorProducts { get; set; } = new List<CompetitorProduct>();
        public virtual ICollection<Price> Prices { get; set; } = new List<Price>();
    }
}