using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PricePulse.Models
{
    public class Price
    {
        [Key]
        public int PriceId { get; set; }

        [ForeignKey("OwnProduct")]
        public int? OwnProductId { get; set; }

        [ForeignKey("CompetitorProduct")]
        public int? CompProductId { get; set; }

        // Alias for CompProductId to match DAO expectations
        public int? CompetitorProductId 
        { 
            get => CompProductId; 
            set => CompProductId = value; 
        }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PriceValue { get; set; }

        // Alias for PriceValue to match DAO expectations
        public decimal? ProductPrice 
        { 
            get => PriceValue; 
            set => PriceValue = value; 
        }

        [StringLength(3)]
        [Column(TypeName = "varchar(3)")]
        public string? Currency { get; set; } = "USD";

        [Column(TypeName = "date")]
        public DateTime? PriceDate { get; set; }

        // Add CreatedDate property expected by DAOs
        public DateTime? CreatedDate 
        { 
            get => PriceDate; 
            set => PriceDate = value; 
        }

        // Navigation properties
        public virtual OwnProduct? OwnProduct { get; set; }
        public virtual CompetitorProduct? CompetitorProduct { get; set; }
    }
}