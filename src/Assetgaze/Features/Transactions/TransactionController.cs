// In: src/Assetgaze/Features/Transactions/TransactionController.cs
using Assetgaze.Features.Transactions.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Collections.Generic; 
using System.Linq; 

namespace Assetgaze.Features.Transactions;

[ApiController]
[Route("api/[controller]")]
[Authorize] // This secures the entire controller
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ITransactionRepository _transactionRepository; // Still needed for GetTransactionById directly

    public TransactionsController(ITransactionService transactionService, ITransactionRepository transactionRepository)
    {
        _transactionService = transactionService;
        _transactionRepository = transactionRepository;
    }

    [HttpPost]
    public async Task<IActionResult> PostTransaction([FromBody] CreateTransactionRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString is null)
        {
            return Unauthorized();
        }
        var userId = Guid.Parse(userIdString);

        // Get account IDs from the JWT claims (using the "account_permission" claim type)
        var authorizedAccountIds = User.FindAll("account_permission").Select(c => Guid.Parse(c.Value)).ToList();

        try
        {
            var createdTransaction = await _transactionService.SaveTransactionAsync(request, userId, authorizedAccountIds);
            return CreatedAtAction(nameof(GetTransactionById), new { id = createdTransaction.Id }, createdTransaction);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message }); 
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while creating the transaction.");
        }
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransactionById(Guid id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString is null)
        {
            return Unauthorized();
        }
        var loggedInUserId = Guid.Parse(userIdString);

        // Get account IDs from the JWT claims (using the "account_permission" claim type)
        var authorizedAccountIds = User.FindAll("account_permission").Select(c => Guid.Parse(c.Value)).ToList();

        var transaction = await _transactionRepository.GetByIdAsync(id);

        if (transaction is null)
        {
            return NotFound();
        }

        // Authorization check: Verify if the transaction's AccountId is in the authorized list
        if (!authorizedAccountIds.Contains(transaction.AccountId))
        {
            return Forbid(); 
        }

        return Ok(transaction);
    }

    [HttpPut("{id}")] 
    public async Task<IActionResult> PutTransaction(Guid id, [FromBody] UpdateTransactionRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString is null)
        {
            return Unauthorized();
        }
        var userId = Guid.Parse(userIdString);

        // Get account IDs from the JWT claims (using the "account_permission" claim type)
        var authorizedAccountIds = User.FindAll("account_permission").Select(c => Guid.Parse(c.Value)).ToList();

        try
        {
            var updatedTransaction = await _transactionService.UpdateTransactionAsync(id, request, userId, authorizedAccountIds);

            if (updatedTransaction is null)
            {
                return NotFound(); 
            }
            return Ok(updatedTransaction);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message }); 
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while updating the transaction.");
        }
    }
    
    [HttpDelete("{id}")] 
    public async Task<IActionResult> DeleteTransaction(Guid id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString is null)
        {
            return Unauthorized();
        }

        var userId = Guid.Parse(userIdString);

        // Get account IDs from the JWT claims (using the "account_permission" claim type)
        var authorizedAccountIds = User.FindAll("account_permission").Select(c => Guid.Parse(c.Value)).ToList();

        try
        {
            var wasDeleted = await _transactionService.DeleteTransactionAsync(id, userId, authorizedAccountIds);

            if (!wasDeleted)
            {
                return NotFound(); 
            }
            return NoContent(); 
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message }); 
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while deleting the transaction.");
        }
    }
}