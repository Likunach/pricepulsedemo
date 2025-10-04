using PricePulse.Models;

namespace PricePulse.DAOs.Interfaces
{
    public interface ICompetitorDAO : IBaseDAO<Competitor>
    {
        // Competitor-specific operations
        Task<IEnumerable<Competitor>> GetByCompanyProfileIdAsync(int companyProfileId);
        Task<Competitor?> GetCompetitorWithProductsAsync(int competitorId);
        Task<IEnumerable<Competitor>> SearchByCompetitorNameAsync(string searchTerm);
        Task<IEnumerable<Competitor>> GetByWebsiteUrlAsync(string websiteUrl);
        Task<bool> CompetitorExistsForCompanyAsync(int companyProfileId, string competitorName);
        Task<IEnumerable<Competitor>> GetCompetitorsWithRecentDataAsync(int days = 30);
    }
}
