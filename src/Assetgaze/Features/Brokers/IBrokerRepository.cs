namespace Assetgaze.Features.Brokers;

public interface IBrokerRepository
{
    Task<List<Broker>> GetAllAsync(); // Corrected: Returns a list of non-nullable Brokers
    Task<Broker?> GetByIdAsync(Guid id);
    Task AddAsync(Broker broker); // Corrected: Returns Task, not Task<Broker>
}