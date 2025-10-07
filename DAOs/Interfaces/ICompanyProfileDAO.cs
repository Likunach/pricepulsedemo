using PricePulse.Models;

namespace PricePulse.DAOs.Interfaces
{
    public interface ICompanyProfileDAO : IBaseDAO<CompanyProfile>
    {
        // CompanyProfile-specific operations
        Task<CompanyProfile?> GetByUserIdAsync(int userId);
        Task<CompanyProfile?> GetCompanyWithAddressAsync(int companyId);
        Task<CompanyProfile?> GetCompanyWithFullDetailsAsync(int companyId);
        Task<IEnumerable<CompanyProfile>> SearchByCompanyNameAsync(string searchTerm);
        Task<IEnumerable<CompanyProfile>> GetCompaniesByIndustryAsync(string industry);
        Task<bool> CompanyExistsForUserAsync(int userId);
        Task<bool> UpdateCompanyDetailsAsync(int companyId, string companyName, string? description = null, string? industry = null);
        Task<IEnumerable<CompanyProfile>> ListCompaniesForUserAsync(int userId);
    }
}
