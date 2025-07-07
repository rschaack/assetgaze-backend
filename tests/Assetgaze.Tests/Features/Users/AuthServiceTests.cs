// In: tests/Assetgaze.Tests/Features/Users/AuthServiceTests.cs
using Assetgaze.Features.Users;
using Assetgaze.Features.Users.DTOs;
using NUnit.Framework;

namespace Assetgaze.Tests.Features.Users;

[TestFixture]
public class AuthServiceTests
{
    private FakeUserRepository _fakeUserRepo = null!;
    private IAuthService _authService = null!;

    [SetUp]
    public void SetUp()
    {
        // Before each test, create a fresh fake repository and service instance
        _fakeUserRepo = new FakeUserRepository();
        _authService = new AuthService(_fakeUserRepo);
    }

    [Test]
    public async Task RegisterAsync_WithNewEmail_ShouldAddUserAndReturnTrue()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.That(result, Is.True); // Registration should be successful
        Assert.That(_fakeUserRepo.Users.Count, Is.EqualTo(1)); // The user should be "saved" to our list

        var savedUser = _fakeUserRepo.Users.First();
        Assert.That(savedUser.Email, Is.EqualTo(request.Email));

        // Verify that the password was hashed and is not stored in plain text
        Assert.That(savedUser.PasswordHash, Is.Not.EqualTo(request.Password)); 
        Assert.That(BCrypt.Net.BCrypt.Verify(request.Password, savedUser.PasswordHash), Is.True);
    }

    [Test]
    public async Task RegisterAsync_WithExistingEmail_ShouldNotAddUserAndReturnFalse()
    {
        // Arrange
        var existingEmail = "existing@example.com";

        // 1. Seed the fake repository with a pre-existing user
        _fakeUserRepo.Users.Add(new User { Id = Guid.NewGuid(), Email = existingEmail, PasswordHash = "somehash" });

        var request = new RegisterRequest
        {
            Email = existingEmail, // Attempt to register with the same email
            Password = "NewPassword123!"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.That(result, Is.False); // Registration should fail
        Assert.That(_fakeUserRepo.Users.Count, Is.EqualTo(1)); // No new user should have been added
    }
}