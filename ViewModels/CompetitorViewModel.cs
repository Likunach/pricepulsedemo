using System.ComponentModel.DataAnnotations;
using PricePulse.Models;

namespace PricePulse.ViewModels
{
    public class CompetitorDiscoveryViewModel
    {
        [Required(ErrorMessage = "Domain is required")]
        [Display(Name = "Company Domain")]
        public string Domain { get; set; } = string.Empty;
        
        public List<CompetitorInfo> DiscoveredCompetitors { get; set; } = new();
        public List<CompetitorInfo> ConfirmedCompetitors { get; set; } = new();
        public List<ManualCompetitor> Competitors { get; set; } = new();
        public bool ShowManualEntry { get; set; } = false;
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }
    }

    public class CompetitorConfirmationViewModel
    {
        public string Domain { get; set; } = string.Empty;
        public List<CompetitorInfo> Competitors { get; set; } = new();
        public List<int> SelectedCompetitorIds { get; set; } = new();
    }

    public class ManualCompetitorEntryViewModel
    {
        public string Domain { get; set; } = string.Empty;
        public List<ManualCompetitor> Competitors { get; set; } = new();
    }

    public class ManualCompetitor
    {
        public int CompetitorId { get; set; }
        
        [Display(Name = "Competitor Name")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Website URL is required")]
        [Display(Name = "Website URL")]
        public string Website { get; set; } = string.Empty;
        
        [Display(Name = "Product Listing Page")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? ProductListingPage { get; set; }
        
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }

    public class CompetitorListViewModel
    {
        public string Domain { get; set; } = string.Empty;
        public List<CompetitorInfo> Competitors { get; set; } = new();
        public int TotalCompetitors { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class CompetitorAnalysisResultsViewModel
    {
        public string Domain { get; set; } = string.Empty;
        public List<CompetitorAnalysisResult> AnalysisResults { get; set; } = new();
        public int TotalCompetitors { get; set; }
        public int TotalProducts { get; set; }
        public int ProductsWithPrices { get; set; }
        public DateTime LastAnalyzed { get; set; }
    }

    public class CompetitorDetailsViewModel
    {
        public string CompetitorDomain { get; set; } = string.Empty;
        public CompetitorAnalysisResult AnalysisResult { get; set; } = new();
        public List<DiscoveredProduct> Products { get; set; } = new();
    }

    public class CompetitorProfileViewModel
    {
        public int CompetitorId { get; set; }
        public string CompetitorName { get; set; } = string.Empty;
        public string CompetitorWebsite { get; set; } = string.Empty;
        public string? CompetitorDescription { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime LastAnalyzed { get; set; }
        public int TotalProductsDiscovered { get; set; }
        public int ActiveProducts { get; set; }
        public List<CompetitorProductAnalysis> HistoricalProducts { get; set; } = new();
        public bool IsAnalyzing { get; set; } = false;
    }
}
