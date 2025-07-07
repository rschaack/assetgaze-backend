// In: src/Assetgaze/MigrationManager.cs

using System.Reflection;
using DbUp;

namespace Assetgaze;

public static class MigrationManager
{
    // This is now a simple utility method that we can call from anywhere.
    // It has one dependency: the database connection string.
    public static void ApplyMigrations(string connectionString)
    {
        // This ensures the database exists before DbUp tries to create its version table.
        EnsureDatabase.For.PostgresqlDatabase(connectionString);
        
        // This robustly finds the 'Migrations' folder relative to your running application
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var scriptsPath = Path.Combine(assemblyPath!, "Migrations");

        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsFromFileSystem(scriptsPath)
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Database migration failed:");
            Console.WriteLine(result.Error);
            Console.ResetColor();
            throw new Exception("Database migration failed.", result.Error);
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Database migration successful!");
        Console.ResetColor();
    }
}