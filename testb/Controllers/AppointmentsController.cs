using Microsoft.AspNetCore.Mvc;
using Template.Exceptions;
using Template.Models;
using Template.Services;

namespace Template.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VisitsController : ControllerBase


{
    private readonly IDbService _dbService;

    public VisitsController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVisit(int id)
    {
        try
        {
            var visit = await _dbService.GetVisitByIdAsync(id);
            return Ok(visit);
        }
        catch (Exception e)
        {
            return NotFound(e.Message);
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> AddVisit([FromBody] CreateVisitRequestDto request)
    {
        if (!request.Services.Any())
        {
            return BadRequest("At least one service is required.");
        }

        try
        {
            await _dbService.AddVisitAsync(request);
            return CreatedAtAction(nameof(GetVisit), new { id = request.VisitId }, request);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}
