using Microsoft.EntityFrameworkCore;
using PricePulse.Data;
using PricePulse.DAOs.Interfaces;
using PricePulse.Models;

namespace PricePulse.DAOs.Implementations
{
    public class UserDAO : BaseDAO<User>, IUserDAO
    {
        public UserDAO(PricePulseDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserWithProfileAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User?> GetUserWithCompanyProfileAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.CompanyProfiles)
                    .ThenInclude(cp => cp.Address)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User?> GetUserWithFullDetailsAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.Profile)
                .Include(u => u.CompanyProfiles)
                    .ThenInclude(cp => cp.Address)
                .Include(u => u.Contact)
                .Include(u => u.Authentication)
                .Include(u => u.UserRoles)
                .Include(u => u.OwnProducts)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _dbSet
                .Where(u => u.AccountStatus == "Active")
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByStatusAsync(string status)
        {
            return await _dbSet
                .Where(u => u.AccountStatus == status)
                .ToListAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
                return false;

            user.LastLoginDate = DateTime.UtcNow;
            await UpdateAsync(user);
            return true;
        }

        public async Task<bool> UpdateAccountStatusAsync(int userId, string status)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
                return false;

            user.AccountStatus = status;
            await UpdateAsync(user);
            return true;
        }

        public async Task<IEnumerable<User>> GetUsersRegisteredAfterAsync(DateTime date)
        {
            return await _dbSet
                .Where(u => u.RegistrationDate >= date)
                .OrderByDescending(u => u.RegistrationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersWithRoleAsync(string roleName)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .Where(u => u.UserRoles.Any(ur => ur.RoleName == roleName))
                .ToListAsync();
        }

        // Override GetByIdAsync to include navigation properties by default
        public override async Task<User?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(u => u.Profile)
                .Include(u => u.Contact)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        // Override GetAllAsync to include basic navigation properties
        public override async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _dbSet
                .Include(u => u.Profile)
                .Include(u => u.Contact)
                .ToListAsync();
        }
    }
}
