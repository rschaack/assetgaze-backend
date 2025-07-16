using System.Net;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Assetgaze.Features.Accounts.DTOs;
using LinqToDB;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Assetgaze.Features.Accounts;
using Assetgaze.Features.Users;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection; // For AddControllers().AddJsonOptions
using Microsoft.AspNetCore.Hosting; // For UseEnvironment

namespace Assetgaze.Tests.Features.Accounts;

[TestFixture]
public class AccountsControllerTests
{
    private static AssetGazeApiFactory _factory = null!;
    private HttpClient _client = null!;
    private static Guid _seededUserId;
    private static Guid _seededAccountId; // For a test-specific account
    private static IUserRepository _userRepository = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new AssetGazeApiFactory();
        await _factory.InitializeContainerAsync();

        MigrationManager.ApplyMigrations(_factory.ConnectionString);

        await using var db = new AppDataConnection(_factory.ConnectionString);

        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = _factory.ConnectionString
        });
        var userRepoConfig = configBuilder.Build();
        _userRepository = new Linq2DbUserRepository(userRepoConfig);

        var user = new User { Id = Guid.NewGuid(), Email = "test@user.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("testpassword") };
        await db.InsertAsync(user);
        _seededUserId = user.Id;

        // Seed an account for testing retrieval and authorization
        var account = new Account { Id = Guid.NewGuid(), Name = "Seeded Test Account" };
        await db.InsertAsync(account);
        _seededAccountId = account.Id;
        await _userRepository.AddUserAccountPermissionAsync(_seededUserId, _seededAccountId);


        _client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.Sources.Clear();
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = _factory.ConnectionString,
                    ["Jwt:Key"] = "ThisIsMySuperSecretKeyForAssetGazeWhichIsLongAndSecure",
                    ["Jwt:Issuer"] = "https://assetgaze.com",
                    ["Jwt:Audience"] = "https://assetgaze.com"
                });
            });
            builder.ConfigureServices(services =>
            {
                services.AddControllers().AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
            });

        }).CreateClient();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _factory.DisposeContainerAsync();
        _factory.Dispose();
        _client.Dispose();
    }

    private void AuthenticateClient(Guid userId, List<Guid> authorizedAccountIds)
    {
        var token = GenerateTestJwtToken(userId, authorizedAccountIds);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    // This helper is duplicated from TransactionControllerTests. Consider putting in a common place for reuse.
    private static string GenerateTestJwtToken(Guid userId, List<Guid> accountIds)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("ThisIsMySuperSecretKeyForAssetGazeWhichIsLongAndSecure");
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };

        foreach (var accountId in accountIds)
        {
            claims.Add(new Claim("account_permission", accountId.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = "https://assetgaze.com",
            Audience = "https://assetgaze.com"
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [Test]
    public async Task PostAccount_WhenCalledWithValidData_CreatesAccountAndPermission()
    {
        // Arrange
        // Authenticate with a fresh user ID to ensure only this account is associated
        var testUserId = Guid.NewGuid();
        // Create a user in the DB for this test to ensure _userRepository can link it
        await using var db = new AppDataConnection(_factory.ConnectionString);
        await db.InsertAsync(new User { Id = testUserId, Email = "newaccount@test.com", PasswordHash = "somehash" });

        // Authenticate this test user without any accounts initially
        AuthenticateClient(testUserId, new List<Guid>()); 

        var createAccountRequest = new CreateAccountRequest { Name = "My New Test Account" };
        var content = new StringContent(JsonSerializer.Serialize(createAccountRequest), Encoding.UTF8, "application/json");

        // Act
        var postResponse = await _client.PostAsync("/api/accounts", content);

        // Assert
        Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var createdAccount = JsonSerializer.Deserialize<Account>(await postResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.That(createdAccount, Is.Not.Null);
        Assert.That(createdAccount.Name, Is.EqualTo(createAccountRequest.Name));

        // Verify that the permission was also created in the database
        var userPermissions = await _userRepository.GetAccountIdsForUserAsync(testUserId);
        Assert.That(userPermissions, Contains.Item(createdAccount.Id));
    }

    [Test]
    public async Task GetAccountById_WhenAuthorized_ReturnsAccount()
    {
        // Arrange
        // _seededUserId owns _seededAccountId from OneTimeSetUp
        AuthenticateClient(_seededUserId, new List<Guid> { _seededAccountId });

        // Act
        var getResponse = await _client.GetAsync($"/api/accounts/{_seededAccountId}");

        // Assert
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var retrievedAccount = JsonSerializer.Deserialize<Account>(await getResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.That(retrievedAccount, Is.Not.Null);
        Assert.That(retrievedAccount.Id, Is.EqualTo(_seededAccountId));
    }

    [Test]
    public async Task GetAccountById_WhenUnauthorized_ReturnsForbidden()
    {
        // Arrange
        // Create an account not owned by the test user
        var anotherUserId = Guid.NewGuid();
        await using var db = new AppDataConnection(_factory.ConnectionString);
        await db.InsertAsync(new User { Id = anotherUserId, Email = "another@user.com", PasswordHash = "somehash" });
        var anotherAccountId = Guid.NewGuid();
        await db.InsertAsync(new Account { Id = anotherAccountId, Name = "Unauthorized Account" });
        await _userRepository.AddUserAccountPermissionAsync(anotherUserId, anotherAccountId); // Link to another user

        // Authenticate with _seededUserId who does NOT have permission to anotherAccountId
        AuthenticateClient(_seededUserId, new List<Guid> { _seededAccountId }); // _seededUserId only has _seededAccountId

        // Act
        var getResponse = await _client.GetAsync($"/api/accounts/{anotherAccountId}");

        // Assert
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task GetAllAccounts_ReturnsOnlyAuthorizedAccounts()
    {
        // Arrange
        // Create a second user and account that the _seededUserId does NOT own
        var secondUserId = Guid.NewGuid();
        await using var db = new AppDataConnection(_factory.ConnectionString);
        await db.InsertAsync(new User { Id = secondUserId, Email = "second@user.com", PasswordHash = "hash" });
        var secondAccountId = Guid.NewGuid();
        await db.InsertAsync(new Account { Id = secondAccountId, Name = "Second User Account" });
        await _userRepository.AddUserAccountPermissionAsync(secondUserId, secondAccountId);

        // Authenticate with _seededUserId (who only has _seededAccountId)
        AuthenticateClient(_seededUserId, new List<Guid> { _seededAccountId });

        // Act
        var getResponse = await _client.GetAsync("/api/accounts");

        // Assert
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var retrievedAccounts = JsonSerializer.Deserialize<List<Account>>(await getResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.That(retrievedAccounts, Is.Not.Null);
        Assert.That(retrievedAccounts.Count, Is.EqualTo(1)); // Should only see _seededAccountId
        Assert.That(retrievedAccounts.First().Id, Is.EqualTo(_seededAccountId));
        Assert.That(retrievedAccounts.First().Name, Is.EqualTo("Seeded Test Account"));
    }
}