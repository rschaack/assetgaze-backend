using LinqToDB;

namespace Assetgaze.Features.Accounts;

public class Linq2DbAccountRepository : IAccountRepository
{
    private readonly string _connectionString;

    // We now inject IConfiguration to get the connection string
    public Linq2DbAccountRepository(IConfiguration configuration)
    {
        // We get the connection string once and store it
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
                            ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
    }

    public async Task AddAsync(Account Account)
    {
        // Create a new connection for this specific operation
        await using var db = new AppDataConnection(_connectionString);
        await db.InsertAsync(Account);
    }

    public async Task<Account?> GetByIdAsync(Guid id)
    {
        // Create a new connection for this specific operation
        await using var db = new AppDataConnection(_connectionString);
        return await db.Accounts
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync();
    }
    
    public async Task<List<Account?>> GetAllAsync()
    {
        // Create a new connection for this specific operation
        await using var db = new AppDataConnection(_connectionString);
        return await db.Accounts.ToListAsync();
    }
}