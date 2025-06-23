// In: tests/Assetgaze.Transactions.API.Tests/AssetGazeApiFactory.cs

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Assetgaze.Transactions.API.Tests;

/// <summary>
/// A custom WebApplicationFactory for integration tests. Its responsibilities are:
/// 1. To spin up a dedicated, isolated PostgreSQL database in a Docker container for tests.
/// 2. To configure the in-memory application to use this test database instead of a real one.
/// 3. To run database migrations against the test database before any tests are executed.
/// </summary>
public class AssetGazeApiFactory : WebApplicationFactory<Program>
{
    // Defines the PostgreSQL container configuration using Testcontainers.
    private readonly IContainer _dbContainer = new ContainerBuilder()
        .WithImage("postgres:16-alpine")
        .WithEnvironment("POSTGRES_USER", "testuser")
        .WithEnvironment("POSTGRES_PASSWORD", "testpassword")
        .WithEnvironment("POSTGRES_DB", "testdb")
        .WithPortBinding(5433, 5432) // Map to 5433 on the host to avoid conflicts with a local dev DB
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready", "-U", "testuser", "-d", "testdb"))
        .Build();

    /// <summary>
    /// A publicly accessible connection string that points to the running test container.
    /// </summary>
    public string ConnectionString => $"Host={_dbContainer.Hostname};Port={_dbContainer.GetMappedPublicPort(5432)};Database=testdb;Username=testuser;Password=testpassword;";

    /// <summary>
    /// This method overrides the application's configuration as it's being built by the test host.
    /// This is the core of our testing setup.
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Trace); // Log everything
        });
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Remove any other configuration sources to ensure a clean slate.
            config.Sources.Clear();
            
            // Step 1: Run our database migrations using the dynamic connection string for the test container.
            MigrationManager.ApplyMigrations(this.ConnectionString);

            // Step 2: Add our test connection string to the application's configuration.
            // This ensures that when the real Program.cs code runs, it finds this connection string.
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = this.ConnectionString
            });
        });
    }

    /// <summary>
    /// Starts the database container. Called once by OneTimeSetUp in the test fixture.
    /// </summary>
    public async Task InitializeContainerAsync()
    {
        await _dbContainer.StartAsync();
    }
    
    /// <summary>
    /// Stops and disposes of the database container. Called once by OneTimeTearDown.
    /// </summary>
    public async Task DisposeContainerAsync()
    {
        await _dbContainer.StopAsync();
    }
}