// In: tests/Assetgaze.Tests/Features/Users/AuthServiceTests.cs
using Assetgaze.Features.Users;
using Assetgaze.Features.Users.DTOs;
using Microsoft.Extensions.Configuration; // <-- Add this using statement
using NUnit.Framework;

namespace Assetgaze.Tests.Features.Users;

[TestFixture]
public class AuthServiceTests
{
    private FakeUserRepository _fakeUserRepo = null!;
    private IConfiguration _fakeConfiguration = null!; // Add a field for the fake config
    private IAuthService _authService = null!;

    [SetUp]
    public void SetUp()
    {
        // 1. Create the Fake Repository
        _fakeUserRepo = new FakeUserRepository();

        // 2. Create an in-memory configuration for the test
        var inMemorySettings = new Dictionary<string, string?>
        {
            // Provide dummy values for the settings the AuthService will read
            {"Jwt:Key", "ThisIsMySuperSecretTestKeyThatIsVerySecure"},
            {"Jwt:Issuer", "https://test-issuer.com"},
            {"Jwt:Audience", "https://test-audience.com"}
        };

        _fakeConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // 3. Create the service with BOTH dependencies
        _authService = new AuthService(_fakeUserRepo, _fakeConfiguration);
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
        Assert.That(result, Is.True);
        Assert.That(_fakeUserRepo.Users.Count, Is.EqualTo(1));
        var savedUser = _fakeUserRepo.Users.First();
        Assert.That(savedUser.Email, Is.EqualTo(request.Email));
        Assert.That(BCrypt.Net.BCrypt.Verify(request.Password, savedUser.PasswordHash), Is.True);
    }

    [Test]
    public async Task RegisterAsync_WithExistingEmail_ShouldNotAddUserAndReturnFalse()
    {
        // Arrange
        var existingEmail = "existing@example.com";
        _fakeUserRepo.Users.Add(new User { Id = Guid.NewGuid(), Email = existingEmail, PasswordHash = "somehash" });
        
        var request = new RegisterRequest
        {
            Email = existingEmail,
            Password = "NewPassword123!"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(_fakeUserRepo.Users.Count, Is.EqualTo(1));
    }
}