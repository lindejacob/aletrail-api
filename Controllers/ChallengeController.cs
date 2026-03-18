using aletrail_api.DAL;
using aletrail_api.Dtos.PubCrawl;
using aletrail_api.Mappers.PubCrawl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aletrail_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChallengeController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ChallengeController> _logger;

    public ChallengeController(ApplicationDbContext context, ILogger<ChallengeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetChallenges([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var challenges = await _context.Challenges
                .OrderBy(c => c.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(challenges.Select(ChallengeMapper.ToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting challenges");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetChallengeById(int id)
    {
        try
        {
            var challenge = await _context.Challenges.FindAsync(id);
            
            if (challenge == null)
                return NotFound(new { message = "Challenge not found" });

            return Ok(ChallengeMapper.ToDto(challenge));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting challenge {ChallengeId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateChallenge([FromBody] CreateChallengeDto dto)
    {
        try
        {
            var challenge = ChallengeMapper.ToEntity(dto);
            _context.Challenges.Add(challenge);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetChallengeById), new { id = challenge.Id }, ChallengeMapper.ToDto(challenge));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating challenge");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateChallenge(int id, [FromBody] UpdateChallengeDto dto)
    {
        try
        {
            var challenge = await _context.Challenges.FindAsync(id);
            
            if (challenge == null)
                return NotFound(new { message = "Challenge not found" });

            ChallengeMapper.UpdateEntity(challenge, dto);
            await _context.SaveChangesAsync();

            return Ok(ChallengeMapper.ToDto(challenge));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating challenge {ChallengeId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteChallenge(int id)
    {
        try
        {
            var challenge = await _context.Challenges.FindAsync(id);
            
            if (challenge == null)
                return NotFound(new { message = "Challenge not found" });

            _context.Challenges.Remove(challenge);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting challenge {ChallengeId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
