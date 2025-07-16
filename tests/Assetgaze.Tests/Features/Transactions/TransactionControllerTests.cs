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
// Removed: using System.Transactions; // This was ambiguous and not needed
using Assetgaze.Domain;
using Assetgaze.Features.Accounts;
using Assetgaze.Features.Brokers;
using Assetgaze.Features.Users;
using Assetgaze.Features.Transactions; 
using Microsoft.Extensions.Configuration;
using System.Collections.Generic; // Added for List<Guid>
using System.Linq;
using System.Text.Json.Serialization; // Added for Select
using Microsoft.Extensions.DependencyInjection; // Added for AddControllers().AddJsonOptions

namespace Assetgaze.Tests.Features.Transactions;


[TestFixture]
public class TransactionControllerTests
{
    private static AssetGazeApiFactory _factory = null!;
    private HttpClient _client = null!;
    private static Guid _seededBrokerId;
    private static Guid _seededAccountId;
    private static Guid _seededUserId;
    private static IUserRepository _userRepository = null!; // New: to interact with real DB for permissions

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new AssetGazeApiFactory();
        await _factory.InitializeContainerAsync();
        
        // Step 1: Run migrations directly, using the connection string from the now-running container.
        MigrationManager.ApplyMigrations(_factory.ConnectionString);
        
        // Step 2: Seed the database with prerequisite data.
        await using var db = new AppDataConnection(_factory.ConnectionString);
        
        // Initialize a real Linq2DbUserRepository for seeding permissions
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = _factory.ConnectionString
        });
        var config = configBuilder.Build();
        _userRepository = new Linq2DbUserRepository(config); // Initialize the real repo

        var user = new User { Id = Guid.NewGuid(), Email = "test@user.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!") }; // Hash password for AuthService logic
        await db.InsertAsync(user);
        _seededUserId = user.Id;

        var broker = new Broker { Id = Guid.NewGuid(), Name = "Test Broker" };
        await db.InsertAsync(broker);
        _seededBrokerId = broker.Id;

        var account = new Account { Id = Guid.NewGuid(), Name = "Test Account" };
        await db.InsertAsync(account);
        _seededAccountId = account.Id;

        // NEW: Seed UserAccountPermission
        await _userRepository.AddUserAccountPermissionAsync(_seededUserId, _seededAccountId);


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
            // Ensure JSON options are configured for enums in tests as well
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

    // Modified AuthenticateClient to pass account IDs
    private void AuthenticateClient(Guid userId, List<Guid> authorizedAccountIds)
    {
        var token = GenerateTestJwtToken(userId, authorizedAccountIds); // Pass account IDs
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [Test]
    public async Task PostTransaction_WhenCalledWithValidData_CreatesAndRetrievesTransaction()
    {
        // Arrange
        // Authenticate the client with the seeded user and their associated account
        AuthenticateClient(_seededUserId, new List<Guid> { _seededAccountId }); // Pass seeded account ID

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
        // Ensure JsonSerializerOptions include JsonStringEnumConverter when serializing
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        
        var content = new StringContent(JsonSerializer.Serialize(newTransaction, jsonSerializerOptions), System.Text.Encoding.UTF8, "application/json");

        // --- DEBUGGING OUTPUT ---
        Console.WriteLine($"DEBUG TEST: Seeded Account ID for request: {newTransaction.AccountId}");
        var generatedToken = GenerateTestJwtToken(_seededUserId, new List<Guid> { _seededAccountId });
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = tokenHandler.ReadJwtToken(generatedToken);
        var claimsInToken = jwtSecurityToken.Claims.Where(c => c.Type == "account_permission").Select(c => c.Value).ToList();
        Console.WriteLine($"DEBUG TEST: Account IDs in generated token: {string.Join(", ", claimsInToken)}");
        Console.WriteLine($"DEBUG TEST: Request Account ID: {newTransaction.AccountId}");
        // ------------------------

        // Act
        var postResponse = await _client.PostAsync("/api/transactions", content);

        // Assert
        Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        
        // When deserializing the response into the Transaction entity (which now has string properties for enums),
        // we don't need JsonStringEnumConverter because it's already a string.
        var createdTransaction = JsonSerializer.Deserialize<Assetgaze.Features.Transactions.Transaction>(
            await postResponse.Content.ReadAsStringAsync(), 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    
        Assert.That(createdTransaction, Is.Not.Null);

        // 2. Make a GET request to retrieve the transaction we just created
        var getResponse = await _client.GetAsync($"/api/transactions/{createdTransaction.Id}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // 3. Deserialize the GET response and assert its values
        var retrievedTransaction = JsonSerializer.Deserialize<Assetgaze.Features.Transactions.Transaction>(
            await getResponse.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    
        Assert.That(retrievedTransaction, Is.Not.Null);
        Assert.That(retrievedTransaction.Id, Is.EqualTo(createdTransaction.Id));
        Assert.That(retrievedTransaction.ISIN, Is.EqualTo(createdTransaction.ISIN));
        Assert.That(retrievedTransaction.Consideration, Is.EqualTo(createdTransaction.Consideration));
        // Assertions for string properties now:
        Assert.That(retrievedTransaction.TransactionType, Is.EqualTo(newTransaction.TransactionType.ToString()));
        Assert.That(retrievedTransaction.TaxWrapper, Is.EqualTo(newTransaction.TaxWrapper.ToString()));

    }
    
    [Test]
    public async Task PutTransaction_WhenCalledWithValidData_UpdatesAndRetrievesTransaction()
    {
        // Arrange
        AuthenticateClient(_seededUserId, new List<Guid> { _seededAccountId });

        // 1. Create an initial transaction
        var initialTransactionRequest = new CreateTransactionRequest
        {
            TransactionType = TransactionType.Buy,
            BrokerId = _seededBrokerId,
            AccountId = _seededAccountId,
            TaxWrapper = TaxWrapper.DEALING,
            ISIN = "US0378331005",
            TransactionDate = DateTime.UtcNow.AddDays(-10),
            Quantity = 5,
            NativePrice = 100.00m,
            LocalPrice = 100.00m,
            Consideration = 500.00m
        };
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        var initialContent = new StringContent(JsonSerializer.Serialize(initialTransactionRequest, jsonSerializerOptions), Encoding.UTF8, "application/json");
        var postResponse = await _client.PostAsync("/api/transactions", initialContent);
        Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var createdTransaction = JsonSerializer.Deserialize<Assetgaze.Features.Transactions.Transaction>(
            await postResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.That(createdTransaction, Is.Not.Null);

        // 2. Prepare an update request
        var updateRequest = new UpdateTransactionRequest
        {
            TransactionType = TransactionType.Sell, // Change type
            BrokerDealReference = "UPDATEDREF",
            BrokerId = _seededBrokerId,
            AccountId = _seededAccountId,
            TaxWrapper = TaxWrapper.SIPP, // Change tax wrapper
            ISIN = "GB00B5B71H80", // Change ISIN
            TransactionDate = DateTime.UtcNow,
            Quantity = 10,
            NativePrice = 250.00m,
            LocalPrice = 250.00m,
            Consideration = 2500.00m,
            BrokerCharge = 15.00m,
            StampDuty = 2.00m
        };
        var updateContent = new StringContent(JsonSerializer.Serialize(updateRequest, jsonSerializerOptions), Encoding.UTF8, "application/json");

        // Act
        var putResponse = await _client.PutAsync($"/api/transactions/{createdTransaction.Id}", updateContent);

        // Assert
        Assert.That(putResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Retrieve the updated transaction and verify
        var getResponse = await _client.GetAsync($"/api/transactions/{createdTransaction.Id}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updatedTransaction = JsonSerializer.Deserialize<Assetgaze.Features.Transactions.Transaction>(
            await getResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.That(updatedTransaction, Is.Not.Null);
        Assert.That(updatedTransaction.Id, Is.EqualTo(createdTransaction.Id));
        Assert.That(updatedTransaction.TransactionType, Is.EqualTo(updateRequest.TransactionType.ToString()));
        Assert.That(updatedTransaction.BrokerDealReference, Is.EqualTo(updateRequest.BrokerDealReference));
        Assert.That(updatedTransaction.TaxWrapper, Is.EqualTo(updateRequest.TaxWrapper.ToString()));
        Assert.That(updatedTransaction.Quantity, Is.EqualTo(updateRequest.Quantity));
        Assert.That(updatedTransaction.NativePrice, Is.EqualTo(updateRequest.NativePrice));
        Assert.That(updatedTransaction.Consideration, Is.EqualTo(updateRequest.Consideration));
        Assert.That(updatedTransaction.BrokerCharge, Is.EqualTo(updateRequest.BrokerCharge));
        Assert.That(updatedTransaction.StampDuty, Is.EqualTo(updateRequest.StampDuty));
        // Note: FxCharge and AccruedInterest might be null if not provided in updateRequest
    }

    [Test]
    public async Task DeleteTransaction_WhenCalledWithValidData_DeletesTransaction()
    {
        // Arrange
        AuthenticateClient(_seededUserId, new List<Guid> { _seededAccountId });

        // 1. Create an initial transaction to delete
        var initialTransactionRequest = new CreateTransactionRequest
        {
            TransactionType = TransactionType.Buy,
            BrokerId = _seededBrokerId,
            AccountId = _seededAccountId,
            TaxWrapper = TaxWrapper.ISA,
            ISIN = "US0378331005",
            TransactionDate = DateTime.UtcNow.AddDays(-5),
            Quantity = 100,
            NativePrice = 10.00m,
            LocalPrice = 10.00m,
            Consideration = 1000.00m
        };
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        var initialContent = new StringContent(JsonSerializer.Serialize(initialTransactionRequest, jsonSerializerOptions), Encoding.UTF8, "application/json");
        var postResponse = await _client.PostAsync("/api/transactions", initialContent);
        Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var createdTransaction = JsonSerializer.Deserialize<Assetgaze.Features.Transactions.Transaction>(
            await postResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.That(createdTransaction, Is.Not.Null);

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/transactions/{createdTransaction.Id}");

        // Assert
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent)); // 204 No Content

        // Verify it's truly deleted by attempting to retrieve it
        var getResponseAfterDelete = await _client.GetAsync($"/api/transactions/{createdTransaction.Id}");
        Assert.That(getResponseAfterDelete.StatusCode, Is.EqualTo(HttpStatusCode.NotFound)); // 404 Not Found
    }
    
    [Test]
    public async Task DeleteTransaction_WhenCalledWithoutAuthorization_ReturnsForbidden()
    {
        // Arrange
        // Create a transaction belonging to the seeded user
        AuthenticateClient(_seededUserId, new List<Guid> { _seededAccountId });
        var initialTransactionRequest = new CreateTransactionRequest
        {
            TransactionType = TransactionType.Buy,
            BrokerId = _seededBrokerId,
            AccountId = _seededAccountId,
            TaxWrapper = TaxWrapper.ISA,
            ISIN = "UNAUTHORIZED_TEST_ISIN",
            TransactionDate = DateTime.UtcNow,
            Quantity = 1, NativePrice = 1, LocalPrice = 1, Consideration = 1
        };
        var jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        var initialContent = new StringContent(JsonSerializer.Serialize(initialTransactionRequest, jsonSerializerOptions), Encoding.UTF8, "application/json");
        var postResponse = await _client.PostAsync("/api/transactions", initialContent);
        Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var createdTransaction = JsonSerializer.Deserialize<Assetgaze.Features.Transactions.Transaction>(
            await postResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.That(createdTransaction, Is.Not.Null);

        // Authenticate with a different user (or no user) who does NOT have permission to _seededAccountId
        var unauthorizedUserId = Guid.NewGuid();
        // This user is not associated with _seededAccountId
        AuthenticateClient(unauthorizedUserId, new List<Guid>()); // No authorized accounts for this user

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/transactions/{createdTransaction.Id}");

        // Assert
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden)); // Expect 403 Forbidden
    }
    
    // Modified GenerateTestJwtToken to include account permissions
    private static string GenerateTestJwtToken(Guid userId, List<Guid> accountIds)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("ThisIsMySuperSecretKeyForAssetGazeWhichIsLongAndSecure");
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };

        // Add account_permission claims
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
}