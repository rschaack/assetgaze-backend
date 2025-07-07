using Assetgaze.Features.Users;

namespace Assetgaze.Tests.Features.Users;

/// <summary>
/// This is a "Fake" implementation of the repository for unit testing the AuthService.
/// It uses an in-memory list to simulate database behavior.
/// </summary>
public class FakeUserRepository : IUserRepository
{
    // This public list allows our tests to inspect the "database" state after an action.
    public readonly List<User> Users = new();

    public Task<User?> GetByEmailAsync(string email)
    {
        var user = Users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task AddAsync(User user)
    {
        Users.Add(user);
        return Task.CompletedTask;
    }
}