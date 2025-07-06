// In: tests/Assetgaze.Transactions.API.Tests/TransactionSaveServiceTests.cs

using Assetgaze.Transactions.API.Interfaces;
using Assetgaze.Transactions.API.Repositories; // Or Services

namespace Assetgaze.Transactions.API.Tests;

[TestFixture]
public class TransactionSaveServiceTests
{
    private FakeTransactionRepository _fakeRepo; // Changed from Mock<T> to our concrete fake
    private ITransactionSaveService _service;

    [SetUp]
    public void SetUp()
    {
        // For each test, create a new instance of our Fake Repository
        _fakeRepo = new FakeTransactionRepository();

        // Create the service we are testing, injecting the fake implementation
        _service = new TransactionSaveService(_fakeRepo);
    }

    [Test]
    public async Task SaveTransactionAsync_WithValidRequest_SavesTransactionToRepository()
    {
        // Arrange
        var request = new CreateTransactionRequest
        {
            Ticker = "MSFT",
            Quantity = 100,
            Price = 300.50m,
            TransactionType = "Buy",
            Currency = "USD",
            TransactionDate = DateTime.UtcNow
        };

        // Act
        var result = await _service.SaveTransactionAsync(request);

        // Assert
        // Instead of verifying a mock, we now assert against the state of our Fake Repository.
        Assert.That(_fakeRepo.Transactions.Count, Is.EqualTo(1));

        var savedTransaction = _fakeRepo.Transactions.First();

        Assert.That(savedTransaction, Is.Not.Null);
        Assert.That(savedTransaction.Id, Is.EqualTo(result.Id)); // Check if the returned ID matches the saved one
        Assert.That(savedTransaction.Ticker, Is.EqualTo("MSFT"));
    }
}