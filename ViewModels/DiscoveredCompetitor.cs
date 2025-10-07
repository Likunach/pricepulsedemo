using System.Text.Json.Serialization;

namespace PricePulse.ViewModels
{
    public class DiscoveredCompetitor
    {
        [JsonPropertyName("company_name")]
        public string CompanyName { get; set; } = string.Empty;
        
        [JsonPropertyName("website_url")]
        public string WebsiteUrl { get; set; } = string.Empty;
        
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        
        [JsonPropertyName("key_products_services")]
        public string KeyProductsServices { get; set; } = string.Empty;
        
        [JsonPropertyName("competition_reason")]
        public string CompetitionReason { get; set; } = string.Empty;
        
        [JsonPropertyName("company_type")]
        public string CompanyType { get; set; } = string.Empty;
        
        [JsonPropertyName("market_position")]
        public string MarketPosition { get; set; } = string.Empty;
        
        public bool IsSelected { get; set; } = false;
    }
}
