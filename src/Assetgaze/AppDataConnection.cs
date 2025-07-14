// In: src/Assetgaze/AppDataConnection.cs
using Assetgaze.Features.Accounts;
using Assetgaze.Features.Brokers;
using Assetgaze.Features.Transactions;
using Assetgaze.Features.Users;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Data; // Keep this using for other potential uses, but not directly for trace logging here

namespace Assetgaze;
    
public class AppDataConnection : DataConnection
{
    public AppDataConnection(string connectionString)
        : base(new DataOptions().UsePostgreSQL(connectionString))
    {
        // NEW: Simplified LinqToDB tracing to log SQL queries
        OnTraceConnection += info =>
        {
            if (info.TraceInfoStep == TraceInfoStep.BeforeExecute)
            {
                Console.WriteLine($"--- LinqToDB SQL Query ---");
                Console.WriteLine(info.SqlText); // This should definitely work
                Console.WriteLine($"Trace Step: {info.TraceInfoStep}"); // Log the trace step for context
                // Removed all code attempting to access info.Parameters or info.Data
                Console.WriteLine("--------------------------");
            }
        };
    }

    public ITable<Transaction> Transactions => this.GetTable<Transaction>();
    public ITable<User> Users => this.GetTable<User>();
    public ITable<Broker> Brokers => this.GetTable<Broker>();
    public ITable<Account> Accounts => this.GetTable<Account>();
    public ITable<UserAccountPermission> UserAccountPermissions => this.GetTable<UserAccountPermission>();
}