using PricePulse.Models;

namespace PricePulse.DAOs.Interfaces
{
    public interface IAddressDAO : IBaseDAO<Address>
    {
        // Address-specific operations
        Task<Address?> GetByCompanyProfileIdAsync(int companyProfileId);
        Task<IEnumerable<Address>> GetByCountryAsync(string country);
        Task<IEnumerable<Address>> GetByStateProvinceAsync(string stateProvince);
        Task<IEnumerable<Address>> GetByCityAsync(string city);
        Task<IEnumerable<Address>> SearchByPostalCodeAsync(string postalCode);
        Task<bool> AddressExistsForCompanyAsync(int companyProfileId);
    }
}
