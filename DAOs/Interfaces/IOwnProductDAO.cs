using PricePulse.Models;

namespace PricePulse.DAOs.Interfaces
{
    public interface IOwnProductDAO : IBaseDAO<OwnProduct>
    {
        // OwnProduct-specific operations
        Task<IEnumerable<OwnProduct>> GetByUserIdAsync(int userId);
        Task<IEnumerable<OwnProduct>> GetByCompanyProfileIdAsync(int companyProfileId);
        Task<OwnProduct?> GetProductWithDetailsAsync(int productId);
        Task<OwnProduct?> GetProductWithPricesAsync(int productId);
        Task<OwnProduct?> GetProductWithCompetitorsAsync(int productId);
        Task<IEnumerable<OwnProduct>> SearchByProductNameAsync(string searchTerm);
        Task<bool> ProductExistsForUserAsync(int userId, string productName);
        Task<IEnumerable<OwnProduct>> GetProductsWithRecentPricesAsync(int days = 30);
    }
}
