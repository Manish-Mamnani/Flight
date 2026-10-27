using IdentityService.Models;

namespace IdentityService.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string roleName);
    Task<Role?> GetByIdAsync(int roleId);
    Task<IEnumerable<Role>> GetAllAsync();
}
