using Assetgaze.Features.Accounts;
using Assetgaze.Features.Brokers;
using Assetgaze.Features.Transactions;
using Assetgaze.Features.Users;
using LinqToDB;
using LinqToDB.Data;

namespace Assetgaze;
    
public class AppDataConnection : DataConnection
{
    // This constructor will be used by our repository
    public AppDataConnection(string connectionString)
        : base(new DataOptions().UsePostgreSQL(connectionString))
    {
    }

    public ITable<Transaction> Transactions => this.GetTable<Transaction>();
    public ITable<User> Users => this.GetTable<User>();
    public ITable<Broker> Brokers => this.GetTable<Broker>();
    public ITable<Account> Accounts => this.GetTable<Account>();
}