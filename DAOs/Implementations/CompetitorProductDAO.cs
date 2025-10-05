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

        public async Task<List<CompetitorProduct>> GetByCompetitorIdAsync(int competitorId)
        {
            return await _context.CompetitorProducts
                .Where(cp => cp.CompetitorId == competitorId)
                .Include(cp => cp.OwnProduct)
                .Include(cp => cp.Competitor)
                .ToListAsync();
        }

        public async Task<List<CompetitorProduct>> GetByOwnProductIdAsync(int ownProductId)
        {
            return await _context.CompetitorProducts
                .Where(cp => cp.OwnProductId == ownProductId)
                .Include(cp => cp.OwnProduct)
                .Include(cp => cp.Competitor)
                .ToListAsync();
        }

        public async Task<List<CompetitorProduct>> GetByProductNameAsync(string productName)
        {
            return await _context.CompetitorProducts
                .Where(cp => cp.CProductName != null && cp.CProductName.Contains(productName))
                .Include(cp => cp.OwnProduct)
                .Include(cp => cp.Competitor)
                .ToListAsync();
        }
    }
}