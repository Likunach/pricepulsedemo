using PricePulse.ViewModels;
using System.Linq;

namespace PricePulse.Models
{
    public class CompetitorAnalysisResult
    {
        public string CompetitorDomain { get; set; } = string.Empty;
        public CompetitorInfo? CompetitorInfo { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public List<DiscoveredProduct> Products { get; set; } = new();
        public long AnalysisTime { get; set; }
        public DateTime AnalyzedAt { get; set; }
        public int TotalProducts => Products.Count;
        public int ProductsWithPrices => Products.Count(p => p.OurPrice.HasValue);
        public decimal? AveragePrice => Products.Where(p => p.OurPrice.HasValue).Any() ? Products.Where(p => p.OurPrice.HasValue).Average(p => p.OurPrice!.Value) : null;
        public decimal? MinPrice => Products.Where(p => p.OurPrice.HasValue).Any() ? Products.Where(p => p.OurPrice.HasValue).Min(p => p.OurPrice!.Value) : null;
        public decimal? MaxPrice => Products.Where(p => p.OurPrice.HasValue).Any() ? Products.Where(p => p.OurPrice.HasValue).Max(p => p.OurPrice!.Value) : null;
    }
}
