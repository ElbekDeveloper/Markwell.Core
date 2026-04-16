using Markwell.Core.Data;
using Markwell.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Markwell.Core.Brokers
{
    public class UserBroker
    {
        private readonly ApplicationDbContext _context;

        public UserBroker(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task UpdateUserAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<(List<User>, int totalCount)> GetAllUsersAsync(int pageNumber, int pageSize)
        {
            var query = _context.Users
                .AsNoTracking()
                .OrderBy(u => u.CreatedAt);

            int totalCount = await query.CountAsync();

            var users = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }

        public async Task<(List<User>, int totalCount)> SearchUsersByEmailAsync(
            string searchTerm, int pageNumber, int pageSize)
        {
            var query = _context.Users
                .Where(u => u.Email!.Contains(searchTerm) || u.FullName.Contains(searchTerm))
                .AsNoTracking()
                .OrderBy(u => u.CreatedAt);

            int totalCount = await query.CountAsync();

            var users = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }
    }
}
