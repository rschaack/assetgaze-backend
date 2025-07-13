using Assetgaze.Features.Accounts.DTOs;

namespace Assetgaze.Features.Accounts;

public interface IAccountSaveService
{
    Task<Account> SaveAccountAsync(CreateAccountRequest request, Guid loggedInUserId);
}