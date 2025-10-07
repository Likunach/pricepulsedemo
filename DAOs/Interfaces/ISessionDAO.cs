using PricePulse.Models;

namespace PricePulse.DAOs.Interfaces
{
    public interface ISessionDAO : IBaseDAO<Session>
    {
        // Session-specific operations
        Task<IEnumerable<Session>> GetByUserIdAsync(int userId);
        Task<Session?> GetBySessionTokenAsync(string sessionToken);
        Task<IEnumerable<Session>> GetActiveSessionsAsync();
        Task<IEnumerable<Session>> GetExpiredSessionsAsync();
        Task<bool> SessionExistsAsync(string sessionToken);
        Task<bool> ExpireSessionAsync(string sessionToken);
        Task<int> ExpireAllUserSessionsAsync(int userId);
        Task<int> CleanupExpiredSessionsAsync();
        Task<IEnumerable<Session>> GetUserActiveSessionsAsync(int userId);
    }
}
