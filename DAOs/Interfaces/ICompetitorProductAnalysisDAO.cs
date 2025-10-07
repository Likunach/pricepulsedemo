using PricePulse.Models;

namespace PricePulse.DAOs.Interfaces
{
    public interface ICompetitorProductAnalysisDAO : IBaseDAO<CompetitorProductAnalysis>
    {
        Task<List<CompetitorProductAnalysis>> GetByCompetitorDomainAsync(string domain);
        Task<List<CompetitorProductAnalysis>> GetByProductNameAsync(string productName);
        Task<CompetitorProductAnalysis?> GetByCompetitorDomainAndProductNameAsync(string domain, string productName);
        Task<List<CompetitorProductAnalysis>> GetActiveProductsAsync();
        Task<List<CompetitorProductAnalysis>> GetByCategoryAsync(string category);
        Task UpdateLastUpdatedAsync(int productId);
        Task DeactivateProductAsync(int productId);
        Task ReactivateProductAsync(int productId);
    }
}
