using Microsoft.EntityFrameworkCore;
using PricePulse.Data;
using PricePulse.DAOs.Interfaces;
using PricePulse.Models;

namespace PricePulse.DAOs.Implementations
{
    public class AuthenticationDAO : BaseDAO<Authentication>, IAuthenticationDAO
    {
        public AuthenticationDAO(PricePulseDbContext context) : base(context)
        {
        }

        public async Task<Authentication?> GetByUserIdAsync(int userId)
        {
            return await _dbSet.FirstOrDefaultAsync(a => a.UserId == userId);
        }

        public async Task<Authentication?> GetAuthenticationWithUserAsync(int authId)
        {
            return await _dbSet
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AuthenticationId == authId);
        }

        public async Task<bool> AuthenticationExistsForUserAsync(int userId)
        {
            return await _dbSet.AnyAsync(a => a.UserId == userId);
        }

        public async Task<bool> UpdateTwoFactorStatusAsync(int userId, bool enabled)
        {
            var auth = await GetByUserIdAsync(userId);
            if (auth == null)
                return false;

            auth.TwoFactorEnabled = enabled;
            await UpdateAsync(auth);
            return true;
        }

        public async Task<bool> UpdateSecurityQuestionAsync(int userId, string? question = null, string? answer = null)
        {
            var auth = await GetByUserIdAsync(userId);
            if (auth == null)
                return false;

            if (question != null)
                auth.SecurityQuestion = question;
            if (answer != null)
                auth.SecurityAnswer = answer;

            await UpdateAsync(auth);
            return true;
        }

        public async Task<IEnumerable<Authentication>> GetUsersWithTwoFactorEnabledAsync()
        {
            return await _dbSet
                .Where(a => a.TwoFactorEnabled == true)
                .Include(a => a.User)
                .ToListAsync();
        }

        // Override GetAllAsync to include User navigation property
        public override async Task<IEnumerable<Authentication>> GetAllAsync()
        {
            return await _dbSet
                .Include(a => a.User)
                .ToListAsync();
        }
    }
}
