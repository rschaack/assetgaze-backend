using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Assetgaze.Features.Accounts.DTOs;
using Assetgaze.Features.Users;

namespace Assetgaze.Features.Accounts;


[ApiController]
[Route("api/[controller]")]
[Authorize] // This secures the entire controller
public class AccountsController : ControllerBase
{
    
    private readonly IAccountRepository _AccountRepository;
    private readonly IAccountSaveService _AccountSaveService;
    private readonly IUserRepository _userRepository;

    // Inject the new service and the repository
    public AccountsController(IAccountSaveService AccountSaveService, IAccountRepository AccountRepository, IUserRepository userRepository)
    {
        _AccountSaveService = AccountSaveService;
        _AccountRepository = AccountRepository;
        _userRepository = userRepository;
    }

    [HttpPost]
    public async Task<IActionResult> PostAccount([FromBody] CreateAccountRequest request)
    {
        // Get the user's ID from the JWT claims.
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString is null) return Unauthorized();
        
        var userId = Guid.Parse(userIdString);
        var createAccount = await _AccountSaveService.SaveAccountAsync(request, userId);

        return CreatedAtAction(nameof(GetAccountById), new { id = createAccount.Id }, createAccount);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccountById(Guid id)
    {
        // This endpoint is now also secure. We need to add logic here to ensure
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString is null) return Unauthorized();
        
        var authorizedAccountIds = User.FindAll("account_permission").Select(c => Guid.Parse(c.Value)).ToList();

        var Account = await _AccountRepository.GetByIdAsync(id);
        
        if (!authorizedAccountIds.Contains(id)) return Forbid(); // HTTP 403 Forbidden if user doesn't have permission to this account
        if (Account is null) return NotFound();
        
        return Ok(Account);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllAccounts()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString is null) return Unauthorized();
        
        var authorizedAccountIds = User.FindAll("account_permission").Select(c => Guid.Parse(c.Value)).ToList();
        
        var allAccounts = await _AccountRepository.GetAllAsync();
        var userAccounts = allAccounts.Where(a => authorizedAccountIds.Contains(a.Id)).ToList();

        return Ok(userAccounts);
    }
    
    
}