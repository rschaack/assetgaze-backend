using Assetgaze.Features.Users.DTOs;

namespace Assetgaze.Features.Users;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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
            PasswordHash = hashedPassword
        };

        // 4. Save the new user to the database
        await _userRepository.AddAsync(newUser);

        return true;
    }

    public async Task<string?> LoginAsync(LoginRequest request)
    {
        // 1. Find the user by email
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            // User not found
            return null;
        }

        // 2. Verify the password against the stored hash
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            // Invalid password
            return null;
        }

        // 3. Login is successful. For now, we return a placeholder token.
        //    In the next step, we will generate a real JWT here.
        var token = "success-placeholder-token"; 

        return token;
    }
}