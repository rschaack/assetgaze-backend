
using LinqToDB;

namespace Assetgaze.Features.Users;



public class Linq2DbUserRepository : IUserRepository
{
    private readonly string _connectionString;

    public Linq2DbUserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
                            ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        await using var db = new AppDataConnection(_connectionString);
        return await db.Users
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(User user)
    {
        await using var db = new AppDataConnection(_connectionString);
        await db.InsertAsync(user);
    }
    
    public async Task UpdateAsync(User user)
    {
        await using var db = new AppDataConnection(_connectionString);
        await db.UpdateAsync(user);
    }
    
    public async Task<List<Guid>> GetAccountIdsForUserAsync(Guid userId)
    {
        await using var db = new AppDataConnection(_connectionString);
        return await db.UserAccountPermissions // Use the new table
            .Where(p => p.UserId == userId)
            .Select(p => p.AccountId)
            .ToListAsync();
    }

    public async Task AddUserAccountPermissionAsync(Guid userId, Guid accountId) // New method implementation
    {
        await using var db = new AppDataConnection(_connectionString);
        var permission = new UserAccountPermission { UserId = userId, AccountId = accountId };
        await db.InsertAsync(permission);
    }
}