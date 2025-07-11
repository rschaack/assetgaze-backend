// In: src/Assetgaze/Program.cs

using Assetgaze;
using Assetgaze.Features.Transactions;
using Assetgaze.Features.Users;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();


//builder.Services.AddScoped<ITransactionRepository, Linq2DbTransactionRepository>(); 
//builder.Services.AddScoped<ITransactionSaveService, TransactionSaveService>();
//builder.Services.AddScoped<IUserRepository, Linq2DbUserRepository>();
//builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<ITransactionRepository, Linq2DbTransactionRepository>();
builder.Services.AddScoped<IUserRepository, Linq2DbUserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();


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