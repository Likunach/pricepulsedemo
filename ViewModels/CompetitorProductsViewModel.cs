using PricePulse.Models;

namespace PricePulse.ViewModels
{
    public class CompetitorProductsViewModel
    {
        public string CompetitorDomain { get; set; } = string.Empty;
        public string CompetitorName { get; set; } = string.Empty;
        public List<CompetitorProductAnalysis> Products { get; set; } = new();
        public int TotalProducts { get; set; }
        public int ProductsWithRetailers { get; set; }
        public DateTime LastAnalyzed { get; set; }
        public bool IsAnalyzing { get; set; }
        public string? AnalysisStatus { get; set; }
        public string CompanyLocation { get; set; } = "United States";
    }

    public class CompetitorProductDetailsViewModel
    {
        public CompetitorProductAnalysis Product { get; set; } = new();
        public List<CompetitorProductRetailer> Retailers { get; set; } = new();
        public string CompetitorDomain { get; set; } = string.Empty;
        public string CompetitorName { get; set; } = string.Empty;
        public bool HasRetailers => Retailers.Any();
        public decimal? LowestPrice { get; set; }
        public decimal? HighestPrice { get; set; }
        public decimal? AveragePrice { get; set; }
        public int TotalRetailers { get; set; }
    }

    public class CompetitorAnalysisViewModel
    {
        public string CompetitorDomain { get; set; } = string.Empty;
        public string CompetitorName { get; set; } = string.Empty;
        public List<CompetitorProductAnalysis> Products { get; set; } = new();
        public int TotalProducts { get; set; }
        public int ProductsWithRetailers { get; set; }
        public DateTime LastAnalyzed { get; set; }
        public bool IsAnalyzing { get; set; }
        public string? AnalysisStatus { get; set; }
        public string CompanyLocation { get; set; } = "United States";
        public List<string> Categories { get; set; } = new();
        public Dictionary<string, int> CategoryCounts { get; set; } = new();
    }
}
