// In: src/Assetgaze.Transactions.API/Controllers/TransactionsController.cs

using Assetgaze.Transactions.API;
using Assetgaze.Transactions.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionRepository _transactionRepository;

    // Inject the INTERFACE, not the concrete class
    public TransactionsController(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    [HttpPost]
    public async Task<IActionResult> PostTransaction([FromBody] Transaction transaction)
    {
        transaction.Id = Guid.NewGuid();
        
        // The controller's job is simple: delegate to the repository.
        await _transactionRepository.AddAsync(transaction);

        return CreatedAtAction(nameof(GetTransactionById), new { id = transaction.Id }, transaction);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransactionById(Guid id)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);

        if (transaction is null)
        {
            return NotFound();
        }

        return Ok(transaction);
    }
}