using Microsoft.EntityFrameworkCore;
using PricePulse.Data;
using PricePulse.DAOs.Interfaces;
using PricePulse.Models;

namespace PricePulse.DAOs.Implementations
{
    public class SessionDAO : BaseDAO<Session>, ISessionDAO
    {
        public SessionDAO(PricePulseDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Session>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedDate)
                .Include(s => s.User)
                .ToListAsync();
        }

        public async Task<Session?> GetBySessionTokenAsync(string sessionToken)
        {
            return await _dbSet
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SessionToken == sessionToken);
        }

        public async Task<IEnumerable<Session>> GetActiveSessionsAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(s => s.ExpiryDate > now)
                .Include(s => s.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Session>> GetExpiredSessionsAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(s => s.ExpiryDate <= now)
                .Include(s => s.User)
                .ToListAsync();
        }

        public async Task<bool> SessionExistsAsync(string sessionToken)
        {
            return await _dbSet.AnyAsync(s => s.SessionToken == sessionToken);
        }

        public async Task<bool> ExpireSessionAsync(string sessionToken)
        {
            var session = await _dbSet.FirstOrDefaultAsync(s => s.SessionToken == sessionToken);
            if (session == null)
                return false;

            session.ExpiryDate = DateTime.UtcNow;
            await UpdateAsync(session);
            return true;
        }

        public async Task<int> ExpireAllUserSessionsAsync(int userId)
        {
            var sessions = await _dbSet
                .Where(s => s.UserId == userId && s.ExpiryDate > DateTime.UtcNow)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.ExpiryDate = DateTime.UtcNow;
            }

            if (sessions.Any())
            {
                await UpdateManyAsync(sessions);
            }

            return sessions.Count;
        }

        public async Task<int> CleanupExpiredSessionsAsync()
        {
            var expiredSessions = await GetExpiredSessionsAsync();
            var sessionsList = expiredSessions.ToList();
            
            if (sessionsList.Any())
            {
                _dbSet.RemoveRange(sessionsList);
                await _context.SaveChangesAsync();
            }

            return sessionsList.Count;
        }

        public async Task<IEnumerable<Session>> GetUserActiveSessionsAsync(int userId)
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(s => s.UserId == userId && s.ExpiryDate > now)
                .OrderByDescending(s => s.CreatedDate)
                .Include(s => s.User)
                .ToListAsync();
        }

        // Override GetAllAsync to include User navigation property
        public override async Task<IEnumerable<Session>> GetAllAsync()
        {
            return await _dbSet
                .Include(s => s.User)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
        }
    }
}
