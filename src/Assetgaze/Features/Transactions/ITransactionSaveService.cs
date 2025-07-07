using Assetgaze.Features.Transactions.DTOs;

namespace Assetgaze.Features.Transactions;

public interface ITransactionSaveService
{
    Task<Transaction> SaveTransactionAsync(CreateTransactionRequest request);
}