using Microsoft.EntityFrameworkCore;
using PricePulse.Data;
using PricePulse.DAOs.Interfaces;
using PricePulse.Models;

namespace PricePulse.DAOs.Implementations
{
    public class AddressDAO : BaseDAO<Address>, IAddressDAO
    {
        public AddressDAO(PricePulseDbContext context) : base(context)
        {
        }

        public async Task<Address?> GetByCompanyProfileIdAsync(int companyProfileId)
        {
            return await _dbSet
                .Include(a => a.CompanyProfile)
                .FirstOrDefaultAsync(a => a.CompanyProfileId == companyProfileId);
        }

        public async Task<IEnumerable<Address>> GetByCountryAsync(string country)
        {
            return await _dbSet
                .Where(a => a.Country == country)
                .Include(a => a.CompanyProfile)
                .ToListAsync();
        }

        public async Task<IEnumerable<Address>> GetByStateProvinceAsync(string stateProvince)
        {
            return await _dbSet
                .Where(a => a.StateProvince == stateProvince)
                .Include(a => a.CompanyProfile)
                .ToListAsync();
        }

        public async Task<IEnumerable<Address>> GetByCityAsync(string city)
        {
            return await _dbSet
                .Where(a => a.City == city)
                .Include(a => a.CompanyProfile)
                .ToListAsync();
        }

        public async Task<IEnumerable<Address>> SearchByPostalCodeAsync(string postalCode)
        {
            return await _dbSet
                .Where(a => a.PostalCode == postalCode)
                .Include(a => a.CompanyProfile)
                .ToListAsync();
        }

        public async Task<bool> AddressExistsForCompanyAsync(int companyProfileId)
        {
            return await _dbSet.AnyAsync(a => a.CompanyProfileId == companyProfileId);
        }

        // Override GetAllAsync to include CompanyProfile navigation property
        public override async Task<IEnumerable<Address>> GetAllAsync()
        {
            return await _dbSet
                .Include(a => a.CompanyProfile)
                .ToListAsync();
        }
    }
}
