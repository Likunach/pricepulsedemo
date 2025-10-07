using PricePulse.Models;

namespace PricePulse.DAOs.Interfaces
{
    public interface IContactDAO : IBaseDAO<Contact>
    {
        // Contact-specific operations
        Task<Contact?> GetByUserIdAsync(int userId);
        Task<Contact?> GetContactWithUserAsync(int contactId);
        Task<IEnumerable<Contact>> SearchByPhoneNumberAsync(string phoneNumber);
        Task<bool> ContactExistsForUserAsync(int userId);
        Task<bool> UpdateContactInfoAsync(int userId, string? phoneNumber = null, string? alternateEmail = null);
    }
}
