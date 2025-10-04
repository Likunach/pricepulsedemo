using System.ComponentModel.DataAnnotations;

namespace PricePulse.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalProductsTracked { get; set; }
        public int ActiveAlerts { get; set; }
        public decimal AveragePriceChange { get; set; }
        public List<PriceChangeViewModel> RecentPriceChanges { get; set; } = new List<PriceChangeViewModel>();
        public List<PriceAlertViewModel> PriceAlerts { get; set; } = new List<PriceAlertViewModel>();
    }

    public class PriceChangeViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public decimal PreviousPrice { get; set; }
        public DateTime ChangeDate { get; set; }
        public decimal PriceDifference => CurrentPrice - PreviousPrice;
        public decimal PercentageChange => PreviousPrice != 0 ? (PriceDifference / PreviousPrice) * 100 : 0;
    }

    public class PriceAlertViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal TargetPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal PriceDifference => CurrentPrice - TargetPrice;
    }
}
