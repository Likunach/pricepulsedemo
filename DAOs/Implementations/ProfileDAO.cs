using Microsoft.EntityFrameworkCore;
using PricePulse.Data;
using PricePulse.DAOs.Interfaces;
using PricePulse.Models;

namespace PricePulse.DAOs.Implementations
{
    public class ProfileDAO : BaseDAO<Profile>, IProfileDAO
    {
        public ProfileDAO(PricePulseDbContext context) : base(context)
        {
        }

        public async Task<Profile?> GetByUserIdAsync(int userId)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<Profile?> GetProfileWithUserAsync(int profileId)
        {
            return await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.ProfileId == profileId);
        }

        public async Task<IEnumerable<Profile>> SearchByNameAsync(string searchTerm)
        {
            return await _dbSet
                .Where(p => 
                    EF.Functions.ILike(p.FirstName!, $"%{searchTerm}%") ||
                    EF.Functions.ILike(p.LastName!, $"%{searchTerm}%") ||
                    EF.Functions.ILike($"{p.FirstName} {p.LastName}", $"%{searchTerm}%"))
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task<bool> ProfileExistsForUserAsync(int userId)
        {
            return await _dbSet.AnyAsync(p => p.UserId == userId);
        }

        public async Task<bool> UpdateProfileAsync(int userId, string firstName, string lastName, string? bio = null)
        {
            var profile = await GetByUserIdAsync(userId);
            if (profile == null)
                return false;

            profile.FirstName = firstName;
            profile.LastName = lastName;
            if (bio != null)
                profile.Bio = bio;

            await UpdateAsync(profile);
            return true;
        }

        // Override GetAllAsync to include User navigation property
        public override async Task<IEnumerable<Profile>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.User)
                .ToListAsync();
        }
    }
}
