using IdentityService.Data;
using IdentityService.Models;
using IdentityService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly IdentityDbContext _context;

    public RoleRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByNameAsync(string roleName)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
    }

    public async Task<Role?> GetByIdAsync(int roleId)
    {
        return await _context.Roles.FindAsync(roleId);
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await _context.Roles.ToListAsync();
    }
}
