// In: tests/Assetgaze.Tests/AssetGazeApiFactory.cs

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;

namespace Assetgaze.Tests;

public class AssetGazeApiFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpassword")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready", "-U", "testuser", "-d", "testdb"))
        .Build();

    public string ConnectionString => _dbContainer.GetConnectionString();

    public async Task InitializeContainerAsync()
    {
        await _dbContainer.StartAsync();
    }
    
    public async Task DisposeContainerAsync()
    {
        await _dbContainer.StopAsync();
    }
}
