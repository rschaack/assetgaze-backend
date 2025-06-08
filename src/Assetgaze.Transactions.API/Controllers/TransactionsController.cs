using System.Transactions;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;

namespace Assetgaze.Transactions.API.Controllers;

[ApiController]
[Route("api/[controller]")] // This sets the URL to /api/transactions
public class TransactionsController : ControllerBase
{
    private readonly AppDataConnection _db;
    
    public TransactionsController(AppDataConnection db)
    {
        _db = db;
    }
    
    [HttpPost]
    public async Task<IActionResult> PostTransaction([FromBody] Transaction transaction)
    {
        transaction.Id = Guid.NewGuid();

        // Use the powerful LINQ-based API to insert the record
        await _db.InsertAsync(transaction);

        return CreatedAtAction(nameof(GetTransactionById), new { id = transaction.Id }, transaction);
    }
    
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransactionById(Guid id)
    {
        // Use the queryable Transactions table and LINQ to find the record
        var transaction = await _db.Transactions
            .Where(t => t.Id == id)
            .SingleOrDefaultAsync();

        if (transaction is null)
        {
            return NotFound();
        }

        return Ok(transaction);
    }
}