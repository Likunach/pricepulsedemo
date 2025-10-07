using Microsoft.EntityFrameworkCore;
using PricePulse.Data;
using PricePulse.DAOs.Interfaces;
using PricePulse.Models;

namespace PricePulse.DAOs.Implementations
{
    public class CompanyProfileDAO : BaseDAO<CompanyProfile>, ICompanyProfileDAO
    {
        public CompanyProfileDAO(PricePulseDbContext context) : base(context)
        {
        }

        public async Task<CompanyProfile?> GetByUserIdAsync(int userId)
        {
            return await _dbSet.FirstOrDefaultAsync(cp => cp.UserId == userId);
        }

        public async Task<CompanyProfile?> GetCompanyWithAddressAsync(int companyId)
        {
            return await _dbSet
                .Include(cp => cp.Address)
                .FirstOrDefaultAsync(cp => cp.CompanyProfileId == companyId);
        }

        public async Task<CompanyProfile?> GetCompanyWithFullDetailsAsync(int companyId)
        {
            return await _dbSet
                .Include(cp => cp.User)
                .Include(cp => cp.Address)
                .Include(cp => cp.Competitors)
                .Include(cp => cp.OwnProducts)
                .FirstOrDefaultAsync(cp => cp.CompanyProfileId == companyId);
        }

        public async Task<IEnumerable<CompanyProfile>> SearchByCompanyNameAsync(string searchTerm)
        {
            return await _dbSet
                .Where(cp => EF.Functions.ILike(cp.CompanyName!, $"%{searchTerm}%"))
                .Include(cp => cp.Address)
                .ToListAsync();
        }

        public async Task<IEnumerable<CompanyProfile>> GetCompaniesByIndustryAsync(string industry)
        {
            return await _dbSet
                .Where(cp => cp.Industry == industry)
                .Include(cp => cp.Address)
                .ToListAsync();
        }

        public async Task<bool> CompanyExistsForUserAsync(int userId)
        {
            return await _dbSet.AnyAsync(cp => cp.UserId == userId);
        }

        public async Task<bool> UpdateCompanyDetailsAsync(int companyId, string companyName, string? description = null, string? industry = null)
        {
            var company = await GetByIdAsync(companyId);
            if (company == null)
                return false;

            company.CompanyName = companyName;
            if (description != null)
                company.CompanyDescription = description;
            if (industry != null)
                company.Industry = industry;

            await UpdateAsync(company);
            return true;
        }

        public async Task<IEnumerable<CompanyProfile>> ListCompaniesForUserAsync(int userId)
        {
            return await _dbSet
                .Where(cp => cp.UserId == userId)
                .Include(cp => cp.Address)
                .OrderBy(cp => cp.CompanyName)
                .ToListAsync();
        }

        // Override GetByIdAsync to include Address navigation property
        public override async Task<CompanyProfile?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(cp => cp.Address)
                .FirstOrDefaultAsync(cp => cp.CompanyProfileId == id);
        }

        // Override GetAllAsync to include Address navigation property
        public override async Task<IEnumerable<CompanyProfile>> GetAllAsync()
        {
            return await _dbSet
                .Include(cp => cp.Address)
                .ToListAsync();
        }
    }
}
