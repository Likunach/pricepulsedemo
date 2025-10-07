using Microsoft.EntityFrameworkCore;
using PricePulse.Data;
using PricePulse.DAOs.Interfaces;
using PricePulse.Models;

namespace PricePulse.DAOs.Implementations
{
    public class CompetitorProductAnalysisDAO : BaseDAO<CompetitorProductAnalysis>, ICompetitorProductAnalysisDAO
    {
        public CompetitorProductAnalysisDAO(PricePulseDbContext context) : base(context)
        {
        }

        public async Task<List<CompetitorProductAnalysis>> GetByCompetitorDomainAsync(string domain)
        {
            return await _context.CompetitorProductAnalyses
                .Where(cp => cp.CompetitorDomain == domain && cp.IsActive)
                .Include(cp => cp.Retailers.Where(r => r.IsActive))
                .OrderBy(cp => cp.ProductName)
                .ToListAsync();
        }

        public async Task<List<CompetitorProductAnalysis>> GetByProductNameAsync(string productName)
        {
            return await _context.CompetitorProductAnalyses
                .Where(cp => cp.ProductName.Contains(productName) && cp.IsActive)
                .Include(cp => cp.Retailers.Where(r => r.IsActive))
                .OrderBy(cp => cp.ProductName)
                .ToListAsync();
        }

        public async Task<CompetitorProductAnalysis?> GetByCompetitorDomainAndProductNameAsync(string domain, string productName)
        {
            return await _context.CompetitorProductAnalyses
                .Where(cp => cp.CompetitorDomain == domain && cp.ProductName == productName && cp.IsActive)
                .Include(cp => cp.Retailers.Where(r => r.IsActive))
                .FirstOrDefaultAsync();
        }

        public async Task<List<CompetitorProductAnalysis>> GetActiveProductsAsync()
        {
            return await _context.CompetitorProductAnalyses
                .Where(cp => cp.IsActive)
                .Include(cp => cp.Retailers.Where(r => r.IsActive))
                .OrderBy(cp => cp.CompetitorDomain)
                .ThenBy(cp => cp.ProductName)
                .ToListAsync();
        }

        public async Task<List<CompetitorProductAnalysis>> GetByCategoryAsync(string category)
        {
            return await _context.CompetitorProductAnalyses
                .Where(cp => cp.ProductCategory == category && cp.IsActive)
                .Include(cp => cp.Retailers.Where(r => r.IsActive))
                .OrderBy(cp => cp.ProductName)
                .ToListAsync();
        }

        public async Task UpdateLastUpdatedAsync(int productId)
        {
            var product = await _context.CompetitorProductAnalyses.FindAsync(productId);
            if (product != null)
            {
                product.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeactivateProductAsync(int productId)
        {
            var product = await _context.CompetitorProductAnalyses.FindAsync(productId);
            if (product != null)
            {
                product.IsActive = false;
                product.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ReactivateProductAsync(int productId)
        {
            var product = await _context.CompetitorProductAnalyses.FindAsync(productId);
            if (product != null)
            {
                product.IsActive = true;
                product.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
