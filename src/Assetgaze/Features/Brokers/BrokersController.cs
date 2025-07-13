using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Assetgaze.Features.Brokers.DTOs;

namespace Assetgaze.Features.Brokers;


[ApiController]
[Route("api/[controller]")]
[Authorize] // This secures the entire controller
public class BrokersController : ControllerBase
{
    
    private readonly IBrokerRepository _brokerRepository;
    private readonly IBrokerSaveService _brokerSaveService;

    // Inject the new service and the repository
    public BrokersController(IBrokerSaveService brokerSaveService, IBrokerRepository brokerRepository)
    {
        _brokerSaveService = brokerSaveService;
        _brokerRepository = brokerRepository;
    }

    [HttpPost]
    public async Task<IActionResult> PostBroker([FromBody] CreateBrokerRequest request)
    {
        // Get the user's ID from the JWT claims.
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString is null)
        {
            // This should not happen if [Authorize] is working, but it's a good safeguard.
            return Unauthorized();
        }
        
        var userId = Guid.Parse(userIdString);
        var createBroker = await _brokerSaveService.SaveBrokerAsync(request, userId);

        return CreatedAtAction(nameof(GetBrokerById), new { id = createBroker.Id }, createBroker);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBrokerById(Guid id)
    {
        // This endpoint is now also secure. We need to add logic here to ensure
        var broker = await _brokerRepository.GetByIdAsync(id);

        if (broker is null)
        {
            return NotFound();
        }

        return Ok(broker);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllBrokers()
    {
        var brokers = await _brokerRepository.GetAllAsync();
        return Ok(brokers);
    }
    
    
}