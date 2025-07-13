using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Assetgaze.Features.Accounts.DTOs;

namespace Assetgaze.Features.Accounts;


[ApiController]
[Route("api/[controller]")]
[Authorize] // This secures the entire controller
public class AccountsController : ControllerBase
{
    
    private readonly IAccountRepository _AccountRepository;
    private readonly IAccountSaveService _AccountSaveService;

    // Inject the new service and the repository
    public AccountsController(IAccountSaveService AccountSaveService, IAccountRepository AccountRepository)
    {
        _AccountSaveService = AccountSaveService;
        _AccountRepository = AccountRepository;
    }

    [HttpPost]
    public async Task<IActionResult> PostAccount([FromBody] CreateAccountRequest request)
    {
        // Get the user's ID from the JWT claims.
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString is null)
        {
            // This should not happen if [Authorize] is working, but it's a good safeguard.
            return Unauthorized();
        }
        
        var userId = Guid.Parse(userIdString);
        var createAccount = await _AccountSaveService.SaveAccountAsync(request, userId);

        return CreatedAtAction(nameof(GetAccountById), new { id = createAccount.Id }, createAccount);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccountById(Guid id)
    {
        // This endpoint is now also secure. We need to add logic here to ensure
        var Account = await _AccountRepository.GetByIdAsync(id);

        if (Account is null)
        {
            return NotFound();
        }

        return Ok(Account);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllAccounts()
    {
        var Accounts = await _AccountRepository.GetAllAsync();
        return Ok(Accounts);
    }
    
    
}