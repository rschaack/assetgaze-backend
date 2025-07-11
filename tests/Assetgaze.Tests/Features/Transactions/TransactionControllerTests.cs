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

namespace Assetgaze.Tests.Features.Transactions;



[TestFixture]
public class TransactionControllerTests
{
    private static AssetGazeApiFactory _factory = null!;
    private HttpClient _client = null!;
    private static Guid _seededBrokerId;
    private static Guid _seededAccountId;
    private static Guid _seededUserId; // Field to hold our valid user's ID

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new AssetGazeApiFactory();
        await _factory.InitializeContainerAsync();
        
        // The client must be created here to trigger the migrations in the factory
        _client = _factory.CreateClient();
        
        // Now that tables exist, we can connect and seed data
        await using var db = new AppDataConnection(_factory.ConnectionString);

        // --- THIS IS THE FIX: SEED A USER ---
        var user = new User { Id = Guid.NewGuid(), Email = "test@user.com", PasswordHash = "some_hash" };
        await db.InsertAsync(user);
        _seededUserId = user.Id; // Store the ID of the user we just created
        // ------------------------------------

        // Seed Broker
        var broker = new Broker { Id = Guid.NewGuid(), Name = "Test Broker" };
        await db.InsertAsync(broker);
        _seededBrokerId = broker.Id;

        // Seed Account
        var account = new Account { Id = Guid.NewGuid(), Name = "Test Account" };
        await db.InsertAsync(account);
        _seededAccountId = account.Id;
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
        // --- USE THE SEEDED USER ID FOR AUTHENTICATION ---
        // This ensures the token represents a user that actually exists in the test database.
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