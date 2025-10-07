using Microsoft.EntityFrameworkCore;
using PricePulse.Data;
using PricePulse.DAOs.Interfaces;
using PricePulse.Models;

namespace PricePulse.DAOs.Implementations
{
    public class OwnProductDAO : BaseDAO<OwnProduct>, IOwnProductDAO
    {
        public OwnProductDAO(PricePulseDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<OwnProduct>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(op => op.UserId == userId)
                .Include(op => op.CompanyProfile)
                .ToListAsync();
        }

        public async Task<IEnumerable<OwnProduct>> GetByCompanyProfileIdAsync(int companyProfileId)
        {
            return await _dbSet
                .Where(op => op.CompanyProfileId == companyProfileId)
                .Include(op => op.User)
                .ToListAsync();
        }

        public async Task<OwnProduct?> GetProductWithDetailsAsync(int productId)
        {
            return await _dbSet
                .Include(op => op.User)
                .Include(op => op.CompanyProfile)
                    .ThenInclude(cp => cp.Address)
                .Include(op => op.CompetitorProducts)
                    .ThenInclude(cp => cp.Competitor)
                .Include(op => op.Prices.OrderByDescending(p => p.CreatedDate))
                .FirstOrDefaultAsync(op => op.OwnProductId == productId);
        }

        public async Task<OwnProduct?> GetProductWithPricesAsync(int productId)
        {
            return await _dbSet
                .Include(op => op.Prices.OrderByDescending(p => p.CreatedDate))
                .FirstOrDefaultAsync(op => op.OwnProductId == productId);
        }

        public async Task<OwnProduct?> GetProductWithCompetitorsAsync(int productId)
        {
            return await _dbSet
                .Include(op => op.CompetitorProducts)
                    .ThenInclude(cp => cp.Competitor)
                .FirstOrDefaultAsync(op => op.OwnProductId == productId);
        }

        public async Task<IEnumerable<OwnProduct>> SearchByProductNameAsync(string searchTerm)
        {
            return await _dbSet
                .Where(op => EF.Functions.ILike(op.ProductName!, $"%{searchTerm}%"))
                .Include(op => op.CompanyProfile)
                .ToListAsync();
        }

        public async Task<bool> ProductExistsForUserAsync(int userId, string productName)
        {
            return await _dbSet.AnyAsync(op => op.UserId == userId && op.ProductName == productName);
        }

        public async Task<IEnumerable<OwnProduct>> GetProductsWithRecentPricesAsync(int days = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            return await _dbSet
                .Include(op => op.Prices.Where(p => p.CreatedDate >= cutoffDate))
                .Include(op => op.CompanyProfile)
                .Where(op => op.Prices.Any(p => p.CreatedDate >= cutoffDate))
                .ToListAsync();
        }

        // Override GetByIdAsync to include basic navigation properties
        public override async Task<OwnProduct?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(op => op.CompanyProfile)
                .Include(op => op.User)
                .FirstOrDefaultAsync(op => op.OwnProductId == id);
        }

        // Override GetAllAsync to include basic navigation properties
        public override async Task<IEnumerable<OwnProduct>> GetAllAsync()
        {
            return await _dbSet
                .Include(op => op.CompanyProfile)
                .Include(op => op.User)
                .ToListAsync();
        }
    }
}
