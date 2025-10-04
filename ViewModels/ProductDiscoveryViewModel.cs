using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PricePulse.ViewModels
{
    public class ProductDiscoveryViewModel
    {
        [Required]
        [Url]
        [Display(Name = "Company Website URL")]
        public string WebsiteUrl { get; set; } = string.Empty;

        [Display(Name = "Company Location")]
        public string? CompanyLocation { get; set; }

        public List<DiscoveredProduct>? DiscoveredProducts { get; set; }
        public string? SearchPrompt { get; set; }
        public string? ModifiedPrompt { get; set; }
        public bool IsManualEntry { get; set; } = false;
    }

    public class DiscoveredProduct
    {
        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = string.Empty;
        
        [JsonPropertyName("ourPrice")]
        public decimal? OurPrice { get; set; }
        
        [JsonPropertyName("competitorPrices")]
        public List<CompetitorPrice> CompetitorPrices { get; set; } = new List<CompetitorPrice>();
    }

    public class CompetitorPrice
    {
        [JsonPropertyName("retailerName")]
        public string RetailerName { get; set; } = string.Empty;
        
        [JsonPropertyName("price")]
        public decimal? Price { get; set; }
        
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    public class ManualProductEntryViewModel
    {
        [Required]
        [Display(Name = "Your Product Name")]
        public string OwnProductName { get; set; } = string.Empty;

        [Required]
        [Url]
        [Display(Name = "Your Product URL")]
        public string OwnProductUrl { get; set; } = string.Empty;

        [Display(Name = "Current Price")]
        public decimal? CurrentPrice { get; set; }

        [Required]
        [Display(Name = "Currency")]
        public string Currency { get; set; } = "USD";

        [Display(Name = "Product SKU")]
        public string? ProductSku { get; set; }

        [Required]
        [Display(Name = "Competitor Product Name")]
        public string CompetitorProductName { get; set; } = string.Empty;

        [Required]
        [Url]
        [Display(Name = "Competitor Product URL")]
        public string CompetitorProductUrl { get; set; } = string.Empty;

        public string? CompetitorProductUrl2 { get; set; }
        public string? CompetitorProductUrl3 { get; set; }
        public string? CompetitorProductUrl4 { get; set; }
        public string? CompetitorProductUrl5 { get; set; }

        public int CompanyProfileId { get; set; }

        public List<string> AvailableCurrencies => new List<string>
        {
            "USD", "EUR", "GBP", "CAD", "AUD", "JPY", "CHF", "CNY", "INR", "BRL", "MXN", "SGD", "HKD", "NOK", "SEK", "DKK", "PLN", "CZK", "HUF", "RUB"
        };
    }
}
