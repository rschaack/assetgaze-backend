// In: src/Assetgaze/Abstractions/ITransactionRepository.cs
namespace Assetgaze.Features.Transactions;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id);
    Task AddAsync(Transaction transaction);
    Task UpdateAsync(Transaction transaction);
    Task<bool> DeleteAsync(Guid id);
}