// In: tests/Assetgaze.Tests/FakeTransactionRepository.cs

using Assetgaze.Features.Transactions;

namespace Assetgaze.Tests.Features.Transactions;

/// <summary>
/// This is a "Fake" implementation of the repository for unit testing.
/// It uses an in-memory list to simulate database behavior.
/// </summary>
public class FakeTransactionRepository : ITransactionRepository
{
    // This public list allows our tests to inspect the "database" state after an action.
    public readonly List<Transaction> Transactions = new();

    public Task AddAsync(Transaction transaction)
    {
        // Simply add the transaction to our in-memory list.
        Transactions.Add(transaction);
        return Task.CompletedTask;
    }

    public Task<Transaction?> GetByIdAsync(Guid id)
    {
        // Use LINQ to find a transaction in our list.
        var transaction = Transactions.FirstOrDefault(t => t.Id == id);
        return Task.FromResult(transaction);
    }
}