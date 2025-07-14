

using Assetgaze.Features.Transactions;

namespace Assetgaze.Tests.Features.Transactions;

// This Fake Repository is for your service unit tests.
public class FakeTransactionRepository : ITransactionRepository
{
    public readonly List<Transaction> Transactions = new();
    public Task AddAsync(Transaction transaction)
    {
        Transactions.Add(transaction);
        return Task.CompletedTask;
    }
    public Task<Transaction?> GetByIdAsync(Guid id)
    {
        var transaction = Transactions.FirstOrDefault(t => t.Id == id);
        return Task.FromResult(transaction);
    }
    
    public async Task UpdateAsync(Transaction transaction)
    {
        throw new System.NotImplementedException();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
       throw new System.NotImplementedException();
    }
}

