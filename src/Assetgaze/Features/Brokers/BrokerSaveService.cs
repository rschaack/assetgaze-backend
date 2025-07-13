// In: src/Assetgaze/Features/Transactions/Services/TransactionSaveService.cs
using Assetgaze.Features.Brokers.DTOs;

namespace Assetgaze.Features.Brokers.Services;

public class BrokerSaveService : IBrokerSaveService
{
    private readonly IBrokerRepository _brokerRepository;

    public BrokerSaveService(IBrokerRepository brokerRepository)
    {
        _brokerRepository = brokerRepository;
    }

    // The method signature doesn't need to change, but the mapping logic inside MUST be updated.
    public async Task<Broker> SaveBrokerAsync(CreateBrokerRequest request, Guid loggedInUserId)
    {
        var newBroker = new Broker
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
        };

        await _brokerRepository.AddAsync(newBroker);
        
        return newBroker;
    }
}