using PricePulse.Models;

namespace PricePulse.DAOs.Interfaces
{
    public interface IProfileDAO : IBaseDAO<Profile>
    {
        // Profile-specific operations
        Task<Profile?> GetByUserIdAsync(int userId);
        Task<Profile?> GetProfileWithUserAsync(int profileId);
        Task<IEnumerable<Profile>> SearchByNameAsync(string searchTerm);
        Task<bool> ProfileExistsForUserAsync(int userId);
        Task<bool> UpdateProfileAsync(int userId, string firstName, string lastName, string? bio = null);
    }
}
