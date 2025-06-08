using System.Reflection;

namespace Assetgaze.Transactions.API;

using DbUp;
using DbUp.Postgresql;

public static class MigrationManager
{
    public static IHost MigrateDatabase(this IHost host)
    {
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var configuration = services.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Ensure the database exists before running migrations
            EnsureDatabase.For.PostgresqlDatabase(connectionString);

            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var scriptsPath = Path.Combine(assemblyPath!, "Migrations");

            var upgrader = DeployChanges.To
                .PostgresqlDatabase(connectionString)
                // Change this line to point to the file system path
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
                // You might want to throw an exception here to stop the app from starting
                throw new Exception("Database migration failed.", result.Error);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Database migration successful!");
            Console.ResetColor();
        }

        return host;
    }
}