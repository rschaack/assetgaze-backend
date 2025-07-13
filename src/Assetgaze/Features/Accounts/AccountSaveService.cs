// In: src/Assetgaze/Features/Transactions/Services/TransactionSaveService.cs
using Assetgaze.Features.Accounts.DTOs;

namespace Assetgaze.Features.Accounts.Services;

public class AccountSaveService : IAccountSaveService
{
    private readonly IAccountRepository _AccountRepository;

    public AccountSaveService(IAccountRepository AccountRepository)
    {
        _AccountRepository = AccountRepository;
    }

    // The method signature doesn't need to change, but the mapping logic inside MUST be updated.
    public async Task<Account> SaveAccountAsync(CreateAccountRequest request, Guid loggedInUserId)
    {
        var newAccount = new Account
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
        };

        await _AccountRepository.AddAsync(newAccount);
        
        return newAccount;
    }
}