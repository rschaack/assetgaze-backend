// In: tests/Assetgaze.Tests/Features/Transactions/TransactionControllerTests.cs
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Assetgaze.Tests.Features.Transactions;

[TestFixture]
public class TransactionControllerTests
{
    private static AssetGazeApiFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new AssetGazeApiFactory();
        await _factory.InitializeContainerAsync();
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _factory.DisposeContainerAsync();
        _factory.Dispose();
        _client.Dispose();
    }

    [Test]
    public async Task PostTransaction_WhenCalledWithValidData_ReturnsCreatedStatus()
    {
        // Your test logic remains the same.
        var newTransaction = new
        {
            Ticker = "AAPL",
            Quantity = 10,
            Price = 175.50,
            TransactionType = "Buy",
            Currency = "USD",
            TransactionDate = "2025-06-23T10:00:00Z"
        };
        var content = new StringContent(JsonSerializer.Serialize(newTransaction), System.Text.Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync("/api/transactions", content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }
}