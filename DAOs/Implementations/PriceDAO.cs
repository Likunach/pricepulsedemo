using Microsoft.EntityFrameworkCore;
using PricePulse.Data;
using PricePulse.DAOs.Interfaces;
using PricePulse.Models;

namespace PricePulse.DAOs.Implementations
{
    public class PriceDAO : BaseDAO<Price>, IPriceDAO
    {
        public PriceDAO(PricePulseDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Price>> GetByOwnProductIdAsync(int ownProductId)
        {
            return await _dbSet
                .Where(p => p.OwnProductId == ownProductId)
                .OrderByDescending(p => p.CreatedDate)
                .Include(p => p.OwnProduct)
                .ToListAsync();
        }

        public async Task<IEnumerable<Price>> GetByCompetitorProductIdAsync(int competitorProductId)
        {
            return await _dbSet
                .Where(p => p.CompetitorProductId == competitorProductId)
                .OrderByDescending(p => p.CreatedDate)
                .Include(p => p.CompetitorProduct)
                .ToListAsync();
        }

        public async Task<Price?> GetLatestPriceForProductAsync(int ownProductId)
        {
            return await _dbSet
                .Where(p => p.OwnProductId == ownProductId)
                .OrderByDescending(p => p.CreatedDate)
                .Include(p => p.OwnProduct)
                .FirstOrDefaultAsync();
        }

        public async Task<Price?> GetLatestPriceForCompetitorProductAsync(int competitorProductId)
        {
            return await _dbSet
                .Where(p => p.CompetitorProductId == competitorProductId)
                .OrderByDescending(p => p.CreatedDate)
                .Include(p => p.CompetitorProduct)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Price>> GetPriceHistoryAsync(int ownProductId, DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Where(p => p.OwnProductId == ownProductId && 
                           p.CreatedDate >= fromDate && 
                           p.CreatedDate <= toDate)
                .OrderBy(p => p.CreatedDate)
                .Include(p => p.OwnProduct)
                .ToListAsync();
        }

        public async Task<IEnumerable<Price>> GetCompetitorPriceHistoryAsync(int competitorProductId, DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Where(p => p.CompetitorProductId == competitorProductId && 
                           p.CreatedDate >= fromDate && 
                           p.CreatedDate <= toDate)
                .OrderBy(p => p.CreatedDate)
                .Include(p => p.CompetitorProduct)
                .ToListAsync();
        }

        public async Task<IEnumerable<Price>> GetPricesInRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _dbSet
                .Where(p => p.ProductPrice >= minPrice && p.ProductPrice <= maxPrice)
                .OrderByDescending(p => p.CreatedDate)
                .Include(p => p.OwnProduct)
                .Include(p => p.CompetitorProduct)
                .ToListAsync();
        }

        public async Task<decimal> GetAveragePriceForProductAsync(int ownProductId, int days = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            var prices = await _dbSet
                .Where(p => p.OwnProductId == ownProductId && p.CreatedDate >= cutoffDate)
                .Select(p => p.ProductPrice)
                .ToListAsync();

            return prices.Any() ? (decimal)prices.Average()! : 0;
        }

        public async Task<decimal> GetAveragePriceForCompetitorProductAsync(int competitorProductId, int days = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            var prices = await _dbSet
                .Where(p => p.CompetitorProductId == competitorProductId && p.CreatedDate >= cutoffDate)
                .Select(p => p.ProductPrice)
                .ToListAsync();

            return prices.Any() ? (decimal)prices.Average()! : 0;
        }

        public async Task<IEnumerable<Price>> GetRecentPricesAsync(int days = 7)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            return await _dbSet
                .Where(p => p.CreatedDate >= cutoffDate)
                .OrderByDescending(p => p.CreatedDate)
                .Include(p => p.OwnProduct)
                .Include(p => p.CompetitorProduct)
                .ToListAsync();
        }

        // Override GetAllAsync to include navigation properties
        public override async Task<IEnumerable<Price>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.OwnProduct)
                .Include(p => p.CompetitorProduct)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }
    }
}
