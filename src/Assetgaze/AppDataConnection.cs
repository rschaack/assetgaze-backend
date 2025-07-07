using Assetgaze.Features.Transactions;
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
}