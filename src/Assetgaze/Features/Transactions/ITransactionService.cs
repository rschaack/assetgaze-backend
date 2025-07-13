using Assetgaze.Features.Transactions.DTOs;

namespace Assetgaze.Features.Transactions;

public interface ITransactionService
{
    Task<Transaction> SaveTransactionAsync(CreateTransactionRequest request, Guid loggedInUserId);
    Task<Transaction?> UpdateTransactionAsync(Guid transactionId, UpdateTransactionRequest request, Guid loggedInUserId);
    Task<bool> DeleteTransactionAsync(Guid transactionId, Guid loggedInUserId);
}