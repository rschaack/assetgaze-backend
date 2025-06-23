using Assetgaze.Transactions.API;
using LinqToDB;
using LinqToDB.Data;

namespace Assetgaze.Transactions.API;
    
public class AppDataConnection : DataConnection
{
    // This constructor will be used by our repository
    public AppDataConnection(string connectionString)
        : base(new DataOptions().UsePostgreSQL(connectionString))
    {
    }

    public ITable<Transaction> Transactions => this.GetTable<Transaction>();
}