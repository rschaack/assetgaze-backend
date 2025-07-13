using Assetgaze.Features.Brokers.DTOs;

namespace Assetgaze.Features.Brokers;

public interface IBrokerSaveService
{
    Task<Broker> SaveBrokerAsync(CreateBrokerRequest request, Guid loggedInUserId);
}