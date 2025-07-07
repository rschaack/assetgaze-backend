// In: src/Assetgaze/Program.cs

using Assetgaze;
using Assetgaze.Features.Transactions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// This is the only DI registration needed for our data layer.
// The repository itself will get IConfiguration to find the connection string.
builder.Services.AddScoped<ITransactionRepository, Linq2DbTransactionRepository>();
builder.Services.AddScoped<ITransactionSaveService, TransactionSaveService>();

var app = builder.Build();

// Run migrations on startup for local development.
// This is safe because it's no longer part of the test startup.
var connectionString = app.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    MigrationManager.ApplyMigrations(connectionString);
}

app.MapControllers();
app.Run();

namespace Assetgaze
{
    public partial class Program { }
}