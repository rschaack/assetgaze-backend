using Assetgaze.Features.Transactions.DTOs;

namespace Assetgaze.Features.Transactions;

public class TransactionSaveService : ITransactionSaveService
{
    private readonly ITransactionRepository _transactionRepository;

    public TransactionSaveService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<Transaction> SaveTransactionAsync(CreateTransactionRequest request)
    {
        // 1. Create the domain entity from the request
        var newTransaction = new Transaction
        {
            Id = Guid.NewGuid(), // The service is responsible for creating the new ID
            Ticker = request.Ticker,
            Quantity = request.Quantity,
            Price = request.Price,
            TransactionType = request.TransactionType,
            TransactionDate = request.TransactionDate,
            Currency = request.Currency
        };

        // 2. Delegate the persistence to the repository
        await _transactionRepository.AddAsync(newTransaction);

        // 3. Return the newly created entity
        return newTransaction;
    }
}