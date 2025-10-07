namespace PricePulse.ViewModels
{
    public class CompetitorDiscoveryResultViewModel
    {
        public string WebsiteUrl { get; set; } = string.Empty;
        public string CompanyLocation { get; set; } = string.Empty;
        public List<DiscoveredCompetitor> DiscoveredCompetitors { get; set; } = new();
        public double DiscoveryTime { get; set; }
        public DateTime DiscoveredAt { get; set; }
        public int TotalCompetitors => DiscoveredCompetitors.Count;
        public int SelectedCompetitors => DiscoveredCompetitors.Count(c => c.IsSelected);
    }
}
