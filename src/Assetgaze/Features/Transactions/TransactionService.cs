
using Assetgaze.Features.Transactions.DTOs;

namespace Assetgaze.Features.Transactions;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;

    public TransactionService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<Transaction> SaveTransactionAsync(CreateTransactionRequest request, Guid loggedInUserId, List<Guid> authorizedAccountIds)
    {
        if (!authorizedAccountIds.Contains(request.AccountId))
            throw new UnauthorizedAccessException("Cannot create transaction for an account not authorized for the user.");
        
        
        var newTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            TransactionType = request.TransactionType.ToString(), 
            BrokerDealReference = request.BrokerDealReference,
            BrokerId = request.BrokerId,
            AccountId = request.AccountId,
            TaxWrapper = request.TaxWrapper.ToString(),
            ISIN = request.ISIN,
            TransactionDate = request.TransactionDate,
            Quantity = request.Quantity,
            NativePrice = request.NativePrice,
            LocalPrice = request.LocalPrice,
            Consideration = request.Consideration,
            BrokerCharge = request.BrokerCharge,
            StampDuty = request.StampDuty,
            FxCharge = request.FxCharge,
            AccruedInterest = request.AccruedInterest
        };
        await _transactionRepository.AddAsync(newTransaction);
        return newTransaction;
    }

    public async Task<Transaction?> UpdateTransactionAsync(Guid transactionId, UpdateTransactionRequest request, Guid loggedInUserId, List<Guid> authorizedAccountIds) 
    {
        var existingTransaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (existingTransaction == null)
        {
            return null; // Transaction not found
        }

        // Authorization: Check if the transaction's current AccountId is authorized for this user
        if (!authorizedAccountIds.Contains(existingTransaction.AccountId)) 
        { 
            throw new UnauthorizedAccessException("User is not authorized to update this transaction."); 
        } 

        // If the AccountId is being changed, ensure the new AccountId is also authorized
        if (existingTransaction.AccountId != request.AccountId && !authorizedAccountIds.Contains(request.AccountId))
        {
            throw new UnauthorizedAccessException("Cannot move transaction to an account not authorized for the user.");
        }

        // Apply all updates from the new request DTO
        existingTransaction.TransactionType = request.TransactionType.ToString();
        existingTransaction.BrokerDealReference = request.BrokerDealReference;
        existingTransaction.BrokerId = request.BrokerId;
        existingTransaction.AccountId = request.AccountId; 
        existingTransaction.TaxWrapper = request.TaxWrapper.ToString();
        existingTransaction.ISIN = request.ISIN;
        existingTransaction.TransactionDate = request.TransactionDate;
        existingTransaction.Quantity = request.Quantity;
        existingTransaction.NativePrice = request.NativePrice;
        existingTransaction.LocalPrice = request.LocalPrice;
        existingTransaction.Consideration = request.Consideration;
        existingTransaction.BrokerCharge = request.BrokerCharge;
        existingTransaction.StampDuty = request.StampDuty;
        existingTransaction.FxCharge = request.FxCharge;
        existingTransaction.AccruedInterest = request.AccruedInterest;

        await _transactionRepository.UpdateAsync(existingTransaction);
        
        return existingTransaction;
    }

    public async Task<bool> DeleteTransactionAsync(Guid transactionId, Guid loggedInUserId, List<Guid> authorizedAccountIds) 
    {
        var transactionToDelete = await _transactionRepository.GetByIdAsync(transactionId);
        if (transactionToDelete == null)
        {
            return false; // Transaction not found
        }

        // Authorization: Check if the transaction's AccountId is authorized for this user
        if (!authorizedAccountIds.Contains(transactionToDelete.AccountId)) 
        { 
            throw new UnauthorizedAccessException("User is not authorized to update this transaction."); // Changed to throw
        } 

        return await _transactionRepository.DeleteAsync(transactionId);
    }
}