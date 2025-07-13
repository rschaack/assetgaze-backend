// In: src/Assetgaze/Features/Transactions/TransactionsController.cs
using Assetgaze.Features.Transactions.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Assetgaze.Features.Transactions;

[ApiController]
[Route("api/[controller]")]
[Authorize] // This secures the entire controller
public class TransactionsController : ControllerBase
{
    private readonly ITransactionSaveService _transactionSaveService;
    private readonly ITransactionRepository _transactionRepository;

    // Inject the new service and the repository
    public TransactionsController(ITransactionSaveService transactionSaveService, ITransactionRepository transactionRepository)
    {
        _transactionSaveService = transactionSaveService;
        _transactionRepository = transactionRepository;
    }

    [HttpPost]
    public async Task<IActionResult> PostTransaction([FromBody] CreateTransactionRequest request)
    {
        // Get the user's ID from the JWT claims.
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString is null)
        {
            // This should not happen if [Authorize] is working, but it's a good safeguard.
            return Unauthorized();
        }
        
        var userId = Guid.Parse(userIdString);
        var createdTransaction = await _transactionSaveService.SaveTransactionAsync(request, userId);

        return CreatedAtAction(nameof(GetTransactionById), new { id = createdTransaction.Id }, createdTransaction);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransactionById(Guid id)
    {
        // This endpoint is now also secure. We need to add logic here to ensure
        // the logged-in user is the one who owns this transaction.
        var transaction = await _transactionRepository.GetByIdAsync(id);

        if (transaction is null)
        {
            return NotFound();
        }

        // TODO: Add check: if (transaction.OwnerId != loggedInUserId) return Forbid();

        return Ok(transaction);
    }
}