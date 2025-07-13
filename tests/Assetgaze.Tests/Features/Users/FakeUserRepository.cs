using Assetgaze.Features.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assetgaze.Tests.Features.Users;

public class FakeUserRepository : IUserRepository
{
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

    public Task UpdateAsync(User user)
    {
        // For the fake, we just assume the update works.
        // The list holds the reference, so changes to the user object are automatically "persisted".
        return Task.CompletedTask;
    }
}