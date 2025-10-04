using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PricePulse.Models
{
    public class CompetitorProduct
    {
        [Key]
        public int CompProductId { get; set; }

        // Alias for CompProductId to match DAO expectations
        public int CompetitorProductId 
        { 
            get => CompProductId; 
            set => CompProductId = value; 
        }

        [Required]
        [ForeignKey("OwnProduct")]
        public int OwnProductId { get; set; }

        [Required]
        [ForeignKey("Competitor")]
        public int CompetitorId { get; set; }

        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? CProductName { get; set; }

        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? CProductWebsiteUrl { get; set; }

        // Navigation properties
        public virtual OwnProduct OwnProduct { get; set; } = null!;
        public virtual Competitor Competitor { get; set; } = null!;
        public virtual ICollection<Price> Prices { get; set; } = new List<Price>();
    }
}