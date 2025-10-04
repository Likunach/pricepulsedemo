using PricePulse.Models;

namespace PricePulse.DAOs.Interfaces
{
    public interface IAuthenticationDAO : IBaseDAO<Authentication>
    {
        // Authentication-specific operations
        Task<Authentication?> GetByUserIdAsync(int userId);
        Task<Authentication?> GetAuthenticationWithUserAsync(int authId);
        Task<bool> AuthenticationExistsForUserAsync(int userId);
        Task<bool> UpdateTwoFactorStatusAsync(int userId, bool enabled);
        Task<bool> UpdateSecurityQuestionAsync(int userId, string? question = null, string? answer = null);
        Task<IEnumerable<Authentication>> GetUsersWithTwoFactorEnabledAsync();
    }
}
