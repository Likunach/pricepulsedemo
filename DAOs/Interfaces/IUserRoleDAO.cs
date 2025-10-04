using PricePulse.Models;

namespace PricePulse.DAOs.Interfaces
{
    public interface IUserRoleDAO : IBaseDAO<UserRole>
    {
        // UserRole-specific operations
        Task<IEnumerable<UserRole>> GetByUserIdAsync(int userId);
        Task<IEnumerable<UserRole>> GetByRoleNameAsync(string roleName);
        Task<bool> UserHasRoleAsync(int userId, string roleName);
        Task<bool> AssignRoleToUserAsync(int userId, string roleName);
        Task<bool> RemoveRoleFromUserAsync(int userId, string roleName);
        Task<IEnumerable<string>> GetUserRolesAsync(int userId);
        Task<IEnumerable<User>> GetUsersInRoleAsync(string roleName);
        Task<int> CountUsersInRoleAsync(string roleName);
    }
}
