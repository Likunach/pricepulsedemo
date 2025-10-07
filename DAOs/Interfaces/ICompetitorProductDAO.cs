using PricePulse.Models;

namespace PricePulse.DAOs.Interfaces
{
    public interface ICompetitorProductDAO : IBaseDAO<CompetitorProduct>
    {
        Task<List<CompetitorProduct>> GetByCompetitorIdAsync(int competitorId);
        Task<List<CompetitorProduct>> GetByOwnProductIdAsync(int ownProductId);
        Task<List<CompetitorProduct>> GetByProductNameAsync(string productName);
    }
}