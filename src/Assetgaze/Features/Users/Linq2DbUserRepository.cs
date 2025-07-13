
using LinqToDB;

namespace Assetgaze.Features.Users;

public class Linq2DbUserRepository : IUserRepository
{
    private readonly string _connectionString;

    // We now inject IConfiguration to get the connection string
    public Linq2DbUserRepository(IConfiguration configuration)
    {
        // We get the connection string once and store it
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
                            ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        // Create a new connection for this specific operation
        await using var db = new AppDataConnection(_connectionString);
        return await db.Users
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(User user)
    {
        // Create a new connection for this specific operation
        await using var db = new AppDataConnection(_connectionString);
        await db.InsertAsync(user);
    }
    
    public async Task UpdateAsync(User user)
    {
        await using var db = new AppDataConnection(_connectionString);
        await db.UpdateAsync(user);
    }
}