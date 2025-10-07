using PricePulse.Models;

namespace PricePulse.DAOs.Interfaces
{
    public interface IPriceDAO : IBaseDAO<Price>
    {
        // Price-specific operations
        Task<IEnumerable<Price>> GetByOwnProductIdAsync(int ownProductId);
        Task<IEnumerable<Price>> GetByCompetitorProductIdAsync(int competitorProductId);
        Task<Price?> GetLatestPriceForProductAsync(int ownProductId);
        Task<Price?> GetLatestPriceForCompetitorProductAsync(int competitorProductId);
        Task<IEnumerable<Price>> GetPriceHistoryAsync(int ownProductId, DateTime fromDate, DateTime toDate);
        Task<IEnumerable<Price>> GetCompetitorPriceHistoryAsync(int competitorProductId, DateTime fromDate, DateTime toDate);
        Task<IEnumerable<Price>> GetPricesInRangeAsync(decimal minPrice, decimal maxPrice);
        Task<decimal> GetAveragePriceForProductAsync(int ownProductId, int days = 30);
        Task<decimal> GetAveragePriceForCompetitorProductAsync(int competitorProductId, int days = 30);
        Task<IEnumerable<Price>> GetRecentPricesAsync(int days = 7);
    }
}
