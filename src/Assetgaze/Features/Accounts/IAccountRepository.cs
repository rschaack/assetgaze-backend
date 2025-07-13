namespace Assetgaze.Features.Accounts;

public interface IAccountRepository
{
    Task<List<Account>> GetAllAsync(); // Corrected: Returns a list of non-nullable Accounts
    Task<Account?> GetByIdAsync(Guid id);
    Task AddAsync(Account Account); // Corrected: Returns Task, not Task<Account>
}