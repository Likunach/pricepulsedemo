using System.ComponentModel.DataAnnotations;

namespace PricePulse.Models
{
    public class CompetitorProduct
    {
        [Key]
        public int CompProductId { get; set; }
        
        public int? OwnProductId { get; set; }
        
        public int? CompetitorId { get; set; }
        
        public string? CProductName { get; set; }
        
        public string? CProductWebsiteUrl { get; set; }
        
        // Navigation properties
        public virtual OwnProduct? OwnProduct { get; set; }
        public virtual Competitor? Competitor { get; set; }
        public virtual ICollection<Price> Prices { get; set; } = new List<Price>();
    }
}