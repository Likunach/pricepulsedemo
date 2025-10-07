using PricePulse.Models;

namespace PricePulse.DAOs.Interfaces
{
    public interface IUserDAO : IBaseDAO<User>
    {
        // User-specific operations
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetUserWithProfileAsync(int userId);
        Task<User?> GetUserWithCompanyProfileAsync(int userId);
        Task<User?> GetUserWithFullDetailsAsync(int userId);
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<IEnumerable<User>> GetUsersByStatusAsync(string status);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UpdateLastLoginAsync(int userId);
        Task<bool> UpdateAccountStatusAsync(int userId, string status);
        Task<IEnumerable<User>> GetUsersRegisteredAfterAsync(DateTime date);
        Task<IEnumerable<User>> GetUsersWithRoleAsync(string roleName);
    }
}
