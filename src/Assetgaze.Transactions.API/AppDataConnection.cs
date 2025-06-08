namespace Assetgaze.Transactions.API;

using LinqToDB;
using LinqToDB.Data;

public class AppDataConnection : DataConnection
{
    public AppDataConnection(DataOptions<AppDataConnection> options)
        : base(options.Options)
    {
    }

    // This creates a queryable table accessor for your Transactions
    public ITable<Transaction> Transactions => this.GetTable<Transaction>();
}