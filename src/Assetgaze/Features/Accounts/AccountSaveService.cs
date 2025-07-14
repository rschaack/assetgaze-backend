// In: src/Assetgaze/Features/Transactions/Services/TransactionSaveService.cs
using Assetgaze.Features.Accounts.DTOs;
using Assetgaze.Features.Users;

namespace Assetgaze.Features.Accounts.Services;

public class AccountSaveService : IAccountSaveService
{
    private readonly IAccountRepository _AccountRepository;
    private readonly IUserRepository _userRepository;

    public AccountSaveService(IAccountRepository AccountRepository, IUserRepository userRepository)
    {
        _AccountRepository = AccountRepository;
        _userRepository = userRepository;
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
        await _userRepository.AddUserAccountPermissionAsync(loggedInUserId, newAccount.Id);

        
        return newAccount;
    }
}