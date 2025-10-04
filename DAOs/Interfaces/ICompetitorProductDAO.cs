using PricePulse.Models;

namespace PricePulse.DAOs.Interfaces
{
    public interface ICompetitorProductDAO : IBaseDAO<CompetitorProduct>
    {
        // CompetitorProduct-specific operations
        Task<IEnumerable<CompetitorProduct>> GetByCompetitorIdAsync(int competitorId);
        Task<IEnumerable<CompetitorProduct>> GetByOwnProductIdAsync(int ownProductId);
        Task<CompetitorProduct?> GetCompetitorProductWithPricesAsync(int competitorProductId);
        Task<IEnumerable<CompetitorProduct>> GetProductsWithRecentPricesAsync(int days = 30);
        Task<bool> CompetitorProductExistsAsync(int competitorId, int ownProductId);
        Task<IEnumerable<CompetitorProduct>> GetByCompetitorAndProductAsync(int competitorId, int ownProductId);
    }
}
