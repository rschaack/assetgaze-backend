// In: src/Assetgaze.Transactions.API/Abstractions/ITransactionRepository.cs
namespace Assetgaze.Transactions.API.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id);
    Task AddAsync(Transaction transaction);
}