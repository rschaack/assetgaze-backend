using System.Net;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Assetgaze.Features.Brokers.DTOs;
using LinqToDB;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using Assetgaze.Features.Brokers;
using Assetgaze.Features.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection; // For AddControllers().AddJsonOptions
using Microsoft.AspNetCore.Hosting; // For UseEnvironment


namespace Assetgaze.Tests.Features.Brokers;

[TestFixture]
public class BrokersControllerTests
{
    private static AssetGazeApiFactory _factory = null!;
    private HttpClient _client = null!;
    private static Guid _seededUserId;
    private static Guid _seededBrokerId; // For a test-specific broker

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new AssetGazeApiFactory();
        await _factory.InitializeContainerAsync();

        MigrationManager.ApplyMigrations(_factory.ConnectionString);

        await using var db = new AppDataConnection(_factory.ConnectionString);

        var user = new User { Id = Guid.NewGuid(), Email = "testbroker@user.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("testpassword") };
        await db.InsertAsync(user);
        _seededUserId = user.Id;

        // Seed a broker for testing retrieval
        var broker = new Broker { Id = Guid.NewGuid(), Name = "Seeded Test Broker" };
        await db.InsertAsync(broker);
        _seededBrokerId = broker.Id;


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
    public async Task PostBroker_WhenCalledWithValidData_CreatesBroker()
    {
        // Arrange
        // Brokers are globally accessible, so any authenticated user can create one
        AuthenticateClient(_seededUserId, new List<Guid>()); // Authenticate a user

        var createBrokerRequest = new CreateBrokerRequest { Name = "New Broker For Test" };
        var content = new StringContent(JsonSerializer.Serialize(createBrokerRequest), Encoding.UTF8, "application/json");

        // Act
        var postResponse = await _client.PostAsync("/api/brokers", content);

        // Assert
        Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var createdBroker = JsonSerializer.Deserialize<Broker>(await postResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.That(createdBroker, Is.Not.Null);
        Assert.That(createdBroker.Name, Is.EqualTo(createBrokerRequest.Name));
    }

    [Test]
    public async Task GetBrokerById_WhenCalled_ReturnsBroker()
    {
        // Arrange
        // _seededBrokerId is available from OneTimeSetUp
        AuthenticateClient(_seededUserId, new List<Guid>()); // Authenticate a user

        // Act
        var getResponse = await _client.GetAsync($"/api/brokers/{_seededBrokerId}");

        // Assert
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var retrievedBroker = JsonSerializer.Deserialize<Broker>(await getResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.That(retrievedBroker, Is.Not.Null);
        Assert.That(retrievedBroker.Id, Is.EqualTo(_seededBrokerId));
        Assert.That(retrievedBroker.Name, Is.EqualTo("Seeded Test Broker"));
    }

    [Test]
    public async Task GetAllBrokers_WhenCalled_ReturnsAllBrokers()
    {
        // Arrange
        // Create another broker to ensure GetAll returns multiple
        AuthenticateClient(_seededUserId, new List<Guid>()); // Authenticate a user

        var createBrokerRequest = new CreateBrokerRequest { Name = "Another Test Broker" };
        var content = new StringContent(JsonSerializer.Serialize(createBrokerRequest), Encoding.UTF8, "application/json");
        var postResponse = await _client.PostAsync("/api/brokers", content);
        Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created)); // Ensure it's created

        // Act
        var getResponse = await _client.GetAsync("/api/brokers");

        // Assert
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var retrievedBrokers = JsonSerializer.Deserialize<List<Broker>>(await getResponse.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.That(retrievedBrokers, Is.Not.Null);
        Assert.That(retrievedBrokers.Count, Is.GreaterThanOrEqualTo(2)); // Should have at least seeded + 1 new
        Assert.That(retrievedBrokers.Any(b => b.Id == _seededBrokerId && b.Name == "Seeded Test Broker"), Is.True);
        Assert.That(retrievedBrokers.Any(b => b.Name == "Another Test Broker"), Is.True);
    }
}