using Serilog;
using System.Text;
using Assetgaze;
using Assetgaze.Features.Transactions;
using Assetgaze.Features.Transactions.Services;
using Assetgaze.Features.Users;
using Assetgaze.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

// --- ADD AUTHENTICATION & AUTHORIZATION SERVICES ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    // Configure the token validation parameters
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false, // For development, we can ignore token expiration
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddAuthorization();
// ------------------------------------------------

// Add your other services
builder.Services.AddControllers();
builder.Services.AddScoped<ITransactionRepository, Linq2DbTransactionRepository>();
builder.Services.AddScoped<IUserRepository, Linq2DbUserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITransactionSaveService, TransactionSaveService>();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseSerilogRequestLogging();

// Run migrations on startup for local development.
var connectionString = app.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    MigrationManager.ApplyMigrations(connectionString);
}

// --- ADD THE AUTH MIDDLEWARE TO THE REQUEST PIPELINE ---
// IMPORTANT: This must be after app.Build() and before app.MapControllers()
app.UseAuthentication();
app.UseAuthorization();
// ---------------------------------------------------

app.MapControllers();

app.Run();

public partial class Program { }