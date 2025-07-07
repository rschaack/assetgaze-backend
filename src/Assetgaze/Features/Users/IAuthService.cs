using Assetgaze.Features.Users.DTOs;

namespace Assetgaze.Features.Users;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterRequest request);
    Task<string?> LoginAsync(LoginRequest request);
}