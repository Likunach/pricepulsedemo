using System.ComponentModel.DataAnnotations;
using PricePulse.ViewModels;

namespace PricePulse.Models
{
    public class CompetitorInfo
    {
        public int Id { get; set; }
        
        [Required]
        public string Domain { get; set; } = string.Empty;
        
        public int CommonKeywords { get; set; }
        public int OrganicKeywords { get; set; }
        public int OrganicTraffic { get; set; }
        public decimal OrganicCost { get; set; }
        public int AdwordsKeywords { get; set; }
        public int AdwordsTraffic { get; set; }
        public decimal AdwordsCost { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime DiscoveredAt { get; set; }
        
        // Additional details
        public int Traffic { get; set; }
        public int Keywords { get; set; }
        public int Backlinks { get; set; }
        
        // Manual entry fields
        public string? ProductListingPage { get; set; }
        public string? Notes { get; set; }
        
        // Company relationship
        public int? CompanyProfileId { get; set; }
        
        // Product analysis results
        public List<DiscoveredProduct>? DiscoveredProducts { get; set; }
        public DateTime? LastProductAnalysis { get; set; }
        public bool HasProductAnalysis { get; set; }
    }
}
