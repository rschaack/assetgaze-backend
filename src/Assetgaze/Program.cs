// In: src/Assetgaze/Program.cs
// (Relevant additions, full content from before for context)
using Serilog;
using System.Text;
using Assetgaze;
using Assetgaze.Features.Accounts;
using Assetgaze.Features.Accounts.Services;
using Assetgaze.Features.Brokers;
using Assetgaze.Features.Brokers.Services;
using Assetgaze.Features.Transactions;
using Assetgaze.Features.Users;
using Assetgaze.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting; // Added for IHostEnvironment

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddScoped<ITransactionRepository, Linq2DbTransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

builder.Services.AddScoped<IUserRepository, Linq2DbUserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IBrokerRepository, Linq2DbBrokerRepository>();
builder.Services.AddScoped<IBrokerSaveService, BrokerSaveService>();

builder.Services.AddScoped<IAccountRepository, Linq2DbAccountRepository>();
builder.Services.AddScoped<IAccountSaveService, AccountSaveService>();

var app = builder.Build();

// No change needed here, as middleware automatically resolves injected services
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseSerilogRequestLogging();

var connectionString = app.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    MigrationManager.ApplyMigrations(connectionString);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }