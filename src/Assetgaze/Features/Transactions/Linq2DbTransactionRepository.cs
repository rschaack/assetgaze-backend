using LinqToDB;

namespace Assetgaze.Features.Transactions;

public class Linq2DbTransactionRepository : ITransactionRepository
{
    private readonly string _connectionString;

    // We now inject IConfiguration to get the connection string
    public Linq2DbTransactionRepository(IConfiguration configuration)
    {
        // We get the connection string once and store it
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
                            ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
    }

    public async Task AddAsync(Transaction transaction)
    {
        // Create a new connection for this specific operation
        await using var db = new AppDataConnection(_connectionString);
        
        await db.InsertAsync(transaction);
    }

    public async Task<Transaction?> GetByIdAsync(Guid id)
    {
        // Create a new connection for this specific operation
        await using var db = new AppDataConnection(_connectionString);
        return await db.Transactions
            .Where(t => t.Id == id)
            .SingleOrDefaultAsync();
    }
    public async Task UpdateAsync(Transaction transaction)
    {
        await using var db = new AppDataConnection(_connectionString);
        
        await db.UpdateAsync(transaction);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await using var db = new AppDataConnection(_connectionString);
        // The DeleteAsync method returns the number of rows affected.
        // We return true if one row was deleted.
        return await db.Transactions.Where(t => t.Id == id).DeleteAsync() > 0;
    }
}