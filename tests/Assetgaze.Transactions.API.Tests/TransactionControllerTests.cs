using System.Net;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Assetgaze.Transactions.API.Tests;

[TestFixture]
public class TransactionControllerTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // This code runs when the test server is being configured
                builder.ConfigureServices(services =>
                {
                    // Find and remove the HttpsRedirection service
                    var httpsRedirectionOptions = services.SingleOrDefault(
                        d => d.ServiceType == typeof(HttpsRedirectionOptions));
                
                    if (httpsRedirectionOptions != null)
                    {
                        services.Remove(httpsRedirectionOptions);
                    }
                });
            });
    }

    [SetUp]
    public void SetUp()
    {
        _client = _factory.CreateClient();
    }

    [Test]
    public async Task PostTransaction_WhenCalledWithValidData_ReturnsCreatedStatus()
    {
        // Arrange
        var newTransaction = new
        {
            Ticker = "AAPL",
            Quantity = 10,
            Price = 175.50,
            TransactionType = "Buy",
            TransactionDate = "2025-06-09T10:00:00Z"
        };
        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(newTransaction), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/transactions", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _factory.Dispose();
    }
}