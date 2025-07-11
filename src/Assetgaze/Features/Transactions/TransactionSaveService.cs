// In: src/Assetgaze/Features/Transactions/Services/TransactionSaveService.cs
using Assetgaze.Features.Transactions.DTOs;

namespace Assetgaze.Features.Transactions.Services;

public class TransactionSaveService : ITransactionSaveService
{
    private readonly ITransactionRepository _transactionRepository;

    public TransactionSaveService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    // The method signature doesn't need to change, but the mapping logic inside MUST be updated.
    public async Task<Transaction> SaveTransactionAsync(CreateTransactionRequest request, Guid loggedInUserId)
    {
        var newTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            // OwnerId = loggedInUserId, // We will add these back when auth is fully wired up
            // EnteredById = loggedInUserId,

            // --- Map all the new fields from the DTO to the entity ---
            TransactionType = request.TransactionType,
            BrokerDealReference = request.BrokerDealReference,
            BrokerId = request.BrokerId,
            AccountId = request.AccountId,
            TaxWrapper = request.TaxWrapper,
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
}