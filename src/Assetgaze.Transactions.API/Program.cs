using Assetgaze.Transactions.API;
using LinqToDB;
using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLinqToDBContext<AppDataConnection>((provider, options) =>
{
    options
        // Use the "DefaultConnection" string from your configuration (docker-compose.yml)
        .UsePostgreSQL(builder.Configuration.GetConnectionString("DefaultConnection"))
        // This enables logging of generated SQL to the console, great for debugging
        .UseDefaultLogging(provider);
    return options;
});

builder.Services.AddControllers();
var app = builder.Build();
app.MigrateDatabase();
app.MapControllers();
app.Run();


