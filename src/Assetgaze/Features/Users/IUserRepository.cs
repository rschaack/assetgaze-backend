// In: src/Assetgaze/Features/Users/IUserRepository.cs
namespace Assetgaze.Features.Users;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
}