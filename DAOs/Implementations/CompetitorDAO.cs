using Microsoft.EntityFrameworkCore;
using PricePulse.Data;
using PricePulse.DAOs.Interfaces;
using PricePulse.Models;

namespace PricePulse.DAOs.Implementations
{
    public class CompetitorDAO : BaseDAO<Competitor>, ICompetitorDAO
    {
        public CompetitorDAO(PricePulseDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Competitor>> GetByCompanyProfileIdAsync(int companyProfileId)
        {
            return await _dbSet
                .Where(c => c.CompanyProfileId == companyProfileId)
                .Include(c => c.CompanyProfile)
                .ToListAsync();
        }

        public async Task<Competitor?> GetCompetitorWithProductsAsync(int competitorId)
        {
            return await _dbSet
                .Include(c => c.CompanyProfile)
                .Include(c => c.CompetitorProducts)
                    .ThenInclude(cp => cp.OwnProduct)
                .FirstOrDefaultAsync(c => c.CompetitorId == competitorId);
        }

        public async Task<IEnumerable<Competitor>> SearchByCompetitorNameAsync(string searchTerm)
        {
            return await _dbSet
                .Where(c => EF.Functions.ILike(c.CompetitorName!, $"%{searchTerm}%"))
                .Include(c => c.CompanyProfile)
                .ToListAsync();
        }

        public async Task<IEnumerable<Competitor>> GetByWebsiteUrlAsync(string websiteUrl)
        {
            return await _dbSet
                .Where(c => c.CompetitorWebsiteUrl == websiteUrl)
                .Include(c => c.CompanyProfile)
                .ToListAsync();
        }

        public async Task<bool> CompetitorExistsForCompanyAsync(int companyProfileId, string competitorName)
        {
            return await _dbSet.AnyAsync(c => c.CompanyProfileId == companyProfileId && c.CompetitorName == competitorName);
        }

        public async Task<IEnumerable<Competitor>> GetCompetitorsWithRecentDataAsync(int days = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);

            return await _dbSet
                .Include(c => c.CompetitorProducts)
                    .ThenInclude(cp => cp.Prices)
                .Where(c => c.CompetitorProducts.Any(cp => cp.Prices.Any(p => p.CreatedDate >= cutoffDate)))
                .ToListAsync();
        }

        // Override GetAllAsync to include CompanyProfile navigation property
        public override async Task<IEnumerable<Competitor>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.CompanyProfile)
                .ToListAsync();
        }
    }
}
