using Microsoft.EntityFrameworkCore;
using PricePulse.Data;
using PricePulse.DAOs.Interfaces;
using PricePulse.Models;

namespace PricePulse.DAOs.Implementations
{
    public class UserRoleDAO : BaseDAO<UserRole>, IUserRoleDAO
    {
        public UserRoleDAO(PricePulseDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UserRole>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(ur => ur.UserId == userId)
                .Include(ur => ur.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserRole>> GetByRoleNameAsync(string roleName)
        {
            return await _dbSet
                .Where(ur => ur.RoleName == roleName)
                .Include(ur => ur.User)
                .ToListAsync();
        }

        public async Task<bool> UserHasRoleAsync(int userId, string roleName)
        {
            return await _dbSet.AnyAsync(ur => ur.UserId == userId && ur.RoleName == roleName);
        }

        public async Task<bool> AssignRoleToUserAsync(int userId, string roleName)
        {
            // Check if role already exists
            if (await UserHasRoleAsync(userId, roleName))
                return false;

            var userRole = new UserRole
            {
                UserId = userId,
                RoleName = roleName,
                AssignedDate = DateTime.UtcNow
            };

            await CreateAsync(userRole);
            return true;
        }

        public async Task<bool> RemoveRoleFromUserAsync(int userId, string roleName)
        {
            var userRole = await _dbSet
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleName == roleName);
            
            if (userRole == null)
                return false;

            await DeleteAsync(userRole);
            return true;
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
        {
            return await _dbSet
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleName)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersInRoleAsync(string roleName)
        {
            return await _dbSet
                .Where(ur => ur.RoleName == roleName)
                .Select(ur => ur.User)
                .ToListAsync();
        }

        public async Task<int> CountUsersInRoleAsync(string roleName)
        {
            return await _dbSet
                .Where(ur => ur.RoleName == roleName)
                .CountAsync();
        }

        // Override GetAllAsync to include User navigation property
        public override async Task<IEnumerable<UserRole>> GetAllAsync()
        {
            return await _dbSet
                .Include(ur => ur.User)
                .OrderBy(ur => ur.RoleName)
                .ThenBy(ur => ur.AssignedDate)
                .ToListAsync();
        }
    }
}
