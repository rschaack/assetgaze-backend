// In: tests/Assetgaze.Tests/Features/Transactions/TransactionSaveServiceTests.cs

using Assetgaze.Domain;
using Assetgaze.Features.Transactions;
using Assetgaze.Features.Transactions.DTOs;
using Assetgaze.Features.Transactions.Services;

namespace Assetgaze.Tests.Features.Transactions;



[TestFixture]
public class TransactionServiceTests
{
    private FakeTransactionRepository _fakeRepo = null!;
    private ITransactionService _service = null!;

    [SetUp]
    public void SetUp()
    {
        // For each test, create a new instance of our Fake Repository
        _fakeRepo = new FakeTransactionRepository();
        
        // Create the service we are testing, injecting the fake implementation
        _service = new TransactionService(_fakeRepo);
    }

    [Test]
    public async Task SaveTransactionAsync_WithValidRequest_SavesTransactionToRepository()
    {
        // Arrange
        var loggedInUserId = Guid.NewGuid(); // A dummy user ID for the test
        var request = new CreateTransactionRequest
        {
            TransactionType = TransactionType.Buy,
            BrokerId = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            TaxWrapper = TaxWrapper.ISA,
            ISIN = "US0378331005",
            TransactionDate = DateTime.UtcNow,
            Quantity = 10,
            NativePrice = 200.00m,
            LocalPrice = 200.00m,
            Consideration = 2000.00m
        };

        // Act
        // Call the service method we want to test
        var result = await _service.SaveTransactionAsync(request, loggedInUserId);

        // Assert
        // 1. Verify that a transaction was actually "saved" to our fake repository
        Assert.That(_fakeRepo.Transactions.Count, Is.EqualTo(1));
        
        var savedTransaction = _fakeRepo.Transactions.First();

        // 2. Verify that the mapping from the DTO to the entity was correct
        Assert.That(savedTransaction, Is.Not.Null);
        Assert.That(savedTransaction.Id, Is.EqualTo(result.Id));
        Assert.That(savedTransaction.ISIN, Is.EqualTo(request.ISIN));
        Assert.That(savedTransaction.TransactionType, Is.EqualTo(request.TransactionType));
    }
}
