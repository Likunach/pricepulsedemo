using System.ComponentModel.DataAnnotations;

namespace PricePulse.Models
{
    public class CompetitorProductAnalysis
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string CompetitorDomain { get; set; } = string.Empty;
        
        [Required]
        public string ProductName { get; set; } = string.Empty;
        
        public string? ProductDescription { get; set; }
        
        public string? CompetitorPrice { get; set; }
        
        public string? CompetitorCurrency { get; set; }
        
        public string? ProductCategory { get; set; }
        
        public string? ProductImageUrl { get; set; }
        
        public string? CompetitorProductUrl { get; set; }
        
        public DateTime DiscoveredAt { get; set; }
        
        public DateTime LastUpdated { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<CompetitorProductRetailer> Retailers { get; set; } = new List<CompetitorProductRetailer>();
    }
    
    public class CompetitorProductRetailer
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int CompetitorProductAnalysisId { get; set; }
        
        [Required]
        public string RetailerName { get; set; } = string.Empty;
        
        public string? RetailerUrl { get; set; }
        
        public string? ProductUrl { get; set; }
        
        public string? Price { get; set; }
        
        public string? Currency { get; set; }
        
        public string? Availability { get; set; }
        
        public string? ShippingInfo { get; set; }
        
        public string? Rating { get; set; }
        
        public string? ReviewsCount { get; set; }
        
        public DateTime LastUpdated { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation property
        public virtual CompetitorProductAnalysis CompetitorProductAnalysis { get; set; } = null!;
    }
}
