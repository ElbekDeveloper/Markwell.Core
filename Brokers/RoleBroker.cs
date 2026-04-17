using Markwell.Core.Data;
using Markwell.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Markwell.Core.Brokers
{
    public class RoleBroker
    {
        private readonly ApplicationDbContext _context;

        public RoleBroker(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name == roleName);
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(string roleId)
        {
            return await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == roleId);
        }
    }
}
