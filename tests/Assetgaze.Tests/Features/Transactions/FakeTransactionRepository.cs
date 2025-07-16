

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
        var existing = Transactions.FirstOrDefault(t => t.Id == transaction.Id);
        if (existing != null)
        {
            // Update properties of the existing fake transaction
            existing.TransactionType = transaction.TransactionType;
            existing.BrokerDealReference = transaction.BrokerDealReference;
            existing.BrokerId = transaction.BrokerId;
            existing.AccountId = transaction.AccountId;
            existing.TaxWrapper = transaction.TaxWrapper;
            existing.ISIN = transaction.ISIN;
            existing.TransactionDate = transaction.TransactionDate;
            existing.Quantity = transaction.Quantity;
            existing.NativePrice = transaction.NativePrice;
            existing.LocalPrice = transaction.LocalPrice;
            existing.Consideration = transaction.Consideration;
            existing.BrokerCharge = transaction.BrokerCharge;
            existing.StampDuty = transaction.StampDuty;
            existing.FxCharge = transaction.FxCharge;
            existing.AccruedInterest = transaction.AccruedInterest;
        }
        return Task.CompletedTask;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var removedCount = Transactions.RemoveAll(t => t.Id == id);
        return Task.FromResult(removedCount > 0);
    }
}

