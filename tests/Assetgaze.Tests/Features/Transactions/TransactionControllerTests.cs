// In: tests/Assetgaze.Tests/Features/Transactions/TransactionControllerTests.cs
using System.Net;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Assetgaze.Features.Transactions.DTOs;
using LinqToDB;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Assetgaze.Domain;
using Assetgaze.Features.Accounts;
using Assetgaze.Features.Brokers;
using Assetgaze.Features.Users;
using Microsoft.Extensions.Configuration;

namespace Assetgaze.Tests.Features.Transactions;



[TestFixture]
public class TransactionControllerTests
{
    private static AssetGazeApiFactory _factory = null!;
    private HttpClient _client = null!;
    private static Guid _seededBrokerId;
    private static Guid _seededAccountId;
    private static Guid _seededUserId;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new AssetGazeApiFactory();
        await _factory.InitializeContainerAsync();
        
        // Step 1: Run migrations directly, using the connection string from the now-running container.
        MigrationManager.ApplyMigrations(_factory.ConnectionString);
        
        // Step 2: Seed the database with prerequisite data.
        await using var db = new AppDataConnection(_factory.ConnectionString);
        
        var user = new User { Id = Guid.NewGuid(), Email = "test@user.com", PasswordHash = "some_hash" };
        await db.InsertAsync(user);
        _seededUserId = user.Id;

        var broker = new Broker { Id = Guid.NewGuid(), Name = "Test Broker" };
        await db.InsertAsync(broker);
        _seededBrokerId = broker.Id;

        var account = new Account { Id = Guid.NewGuid(), Name = "Test Account" };
        await db.InsertAsync(account);
        _seededAccountId = account.Id;

        // Step 3: Create the HttpClient, configuring it to use the test configuration.
        _client = _factory.WithWebHostBuilder(builder =>
        {
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
        }).CreateClient();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _factory.DisposeContainerAsync();
        _factory.Dispose();
        _client.Dispose();
    }

    private void AuthenticateClient(Guid userId)
    {
        var token = GenerateTestJwtToken(userId);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [Test]
    public async Task PostTransaction_WhenCalledWithValidData_ReturnsCreatedStatus()
    {
        // Arrange
        AuthenticateClient(_seededUserId);

        var newTransaction = new CreateTransactionRequest
        {
            TransactionType = TransactionType.Buy,
            BrokerId = _seededBrokerId,
            AccountId = _seededAccountId,
            TaxWrapper = TaxWrapper.ISA,
            ISIN = "US0378331005",
            TransactionDate = DateTime.UtcNow,
            Quantity = 10,
            NativePrice = 200.00m,
            LocalPrice = 200.00m,
            Consideration = 2000.00m
        };
        var content = new StringContent(JsonSerializer.Serialize(newTransaction), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/transactions", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }
    
    private static string GenerateTestJwtToken(Guid userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("ThisIsMySuperSecretKeyForAssetGazeWhichIsLongAndSecure");
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
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
}