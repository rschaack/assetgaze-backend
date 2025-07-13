using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Assetgaze.Features.Users.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Assetgaze.Features.Users;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        // 1. Check if a user with this email already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            // User already exists, registration fails.
            return false; 
        }

        // 2. Hash the password
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 3. Create a new user entity
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = hashedPassword,
            CreatedDate = DateTime.UtcNow,
            FailedLoginAttempts = 0,
            LoginCount = 0
        };

        // 4. Save the new user to the database
        await _userRepository.AddAsync(newUser);

        return true;
    }

    public async Task<string?> LoginAsync(LoginRequest request)
    {
        // Find the user by email
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null) { return null; }

        if (user.LockoutEndDateUtc.HasValue && user.LockoutEndDateUtc.Value > DateTime.UtcNow)
        { return null; }
        
        // Verify the password against the stored hash
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            // --- HANDLE FAILED LOGIN ---
            user.FailedLoginAttempts++;
        
            // Lock account after 5 failed attempts
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEndDateUtc = DateTime.UtcNow.AddMinutes(15);
            }

            await _userRepository.UpdateAsync(user);
            return null;
        }

        // 3. Login is successful. For now, we return a placeholder token.
        user.FailedLoginAttempts = 0;
        user.LockoutEndDateUtc = null; // Clear any previous lock
        user.LoginCount++;
        user.LastLoginDate = DateTime.UtcNow;
        //    In the next step, we will generate a real JWT here.
        var token = GenerateJwtToken(user); 
        return token;
    }
    
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        
        // Get the secret key from appsettings.json
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

        // Define the token's claims (the data it will hold)
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Subject (standard claim for user ID)
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // JWT ID (standard claim for a unique token ID)
        };

        // Create the token descriptor
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1), // Token is valid for 1 hour
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        // Create and write the token
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}