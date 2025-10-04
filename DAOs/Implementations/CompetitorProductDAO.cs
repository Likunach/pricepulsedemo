using Microsoft.EntityFrameworkCore;
using PricePulse.Data;
using PricePulse.DAOs.Interfaces;
using PricePulse.Models;

namespace PricePulse.DAOs.Implementations
{
    public class CompetitorProductDAO : BaseDAO<CompetitorProduct>, ICompetitorProductDAO
    {
        public CompetitorProductDAO(PricePulseDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CompetitorProduct>> GetByCompetitorIdAsync(int competitorId)
        {
            return await _dbSet
                .Where(cp => cp.CompetitorId == competitorId)
                .Include(cp => cp.Competitor)
                .Include(cp => cp.OwnProduct)
                .ToListAsync();
        }

        public async Task<IEnumerable<CompetitorProduct>> GetByOwnProductIdAsync(int ownProductId)
        {
            return await _dbSet
                .Where(cp => cp.OwnProductId == ownProductId)
                .Include(cp => cp.Competitor)
                .Include(cp => cp.OwnProduct)
                .ToListAsync();
        }

        public async Task<CompetitorProduct?> GetCompetitorProductWithPricesAsync(int competitorProductId)
        {
            return await _dbSet
                .Include(cp => cp.Competitor)
                .Include(cp => cp.OwnProduct)
                .Include(cp => cp.Prices.OrderByDescending(p => p.CreatedDate))
                .FirstOrDefaultAsync(cp => cp.CompetitorProductId == competitorProductId);
        }

        public async Task<IEnumerable<CompetitorProduct>> GetProductsWithRecentPricesAsync(int days = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            return await _dbSet
                .Include(cp => cp.Competitor)
                .Include(cp => cp.OwnProduct)
                .Include(cp => cp.Prices.Where(p => p.CreatedDate >= cutoffDate))
                .Where(cp => cp.Prices.Any(p => p.CreatedDate >= cutoffDate))
                .ToListAsync();
        }

        public async Task<bool> CompetitorProductExistsAsync(int competitorId, int ownProductId)
        {
            return await _dbSet.AnyAsync(cp => cp.CompetitorId == competitorId && cp.OwnProductId == ownProductId);
        }

        public async Task<IEnumerable<CompetitorProduct>> GetByCompetitorAndProductAsync(int competitorId, int ownProductId)
        {
            return await _dbSet
                .Where(cp => cp.CompetitorId == competitorId && cp.OwnProductId == ownProductId)
                .Include(cp => cp.Competitor)
                .Include(cp => cp.OwnProduct)
                .ToListAsync();
        }

        // Override GetAllAsync to include navigation properties
        public override async Task<IEnumerable<CompetitorProduct>> GetAllAsync()
        {
            return await _dbSet
                .Include(cp => cp.Competitor)
                .Include(cp => cp.OwnProduct)
                .ToListAsync();
        }
    }
}
