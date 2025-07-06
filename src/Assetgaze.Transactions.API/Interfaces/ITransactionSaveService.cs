namespace Assetgaze.Transactions.API.Interfaces;

public interface ITransactionSaveService
{
    Task<Transaction> SaveTransactionAsync(CreateTransactionRequest request);
}