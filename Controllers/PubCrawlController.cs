using System.Security.Claims;
using aletrail_api.Dtos.PubCrawl;
using aletrail_api.Services.PubCrawl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace aletrail_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PubCrawlController : ControllerBase
{
    private readonly IPubCrawlService _pubCrawlService;
    private readonly ILogger<PubCrawlController> _logger;

    public PubCrawlController(IPubCrawlService pubCrawlService, ILogger<PubCrawlController> logger)
    {
        _pubCrawlService = pubCrawlService;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    [HttpPost("manual")]
    public async Task<IActionResult> CreateManualRoute([FromBody] CreateManualRouteDto dto)
    {
        try
        {
            var userId = GetUserId();
            var route = await _pubCrawlService.CreateManualRouteAsync(dto, userId);
            return Ok(route);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating manual route");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateRoute([FromBody] GenerateRouteDto dto)
    {
        try
        {
            var userId = GetUserId();
            var route = await _pubCrawlService.GenerateRouteAsync(dto, userId);
            return Ok(route);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating route");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetUserRoutes(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            int userId = GetUserId();
            var routes = await _pubCrawlService.GetUserRoutesAsync(userId, page, pageSize);
            return Ok(routes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting routes");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRouteById(int id)
    {
        try
        {
            int userId = GetUserId();
            var route = await _pubCrawlService.GetRouteByIdAsync(id, userId);
            
            if (route == null)
                return NotFound(new { message = "Route not found or access denied" });

            return Ok(route);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting route {RouteId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("join")]
    public async Task<IActionResult> JoinRoute([FromBody] JoinRouteDto dto)
    {
        try
        {
            int userId = GetUserId();
            var route = await _pubCrawlService.JoinRouteAsync(dto.InviteCode, userId);
            return Ok(route);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining route with code {InviteCode}", dto.InviteCode);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/regenerate-code")]
    public async Task<IActionResult> RegenerateInviteCode(int id)
    {
        try
        {
            int userId = GetUserId();
            var inviteCode = await _pubCrawlService.RegenerateInviteCodeAsync(id, userId);
            return Ok(new { inviteCode });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating invite code for route {RouteId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}/participants/{participantUserId}")]
    public async Task<IActionResult> RemoveParticipant(int id, int participantUserId)
    {
        try
        {
            int userId = GetUserId();
            var removed = await _pubCrawlService.RemoveParticipantAsync(id, participantUserId, userId);
            
            if (!removed)
                return NotFound(new { message = "Route or participant not found, or access denied" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing participant {ParticipantUserId} from route {RouteId}", participantUserId, id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoute(int id, [FromBody] UpdateRouteDto dto)
    {
        try
        {
            var userId = GetUserId();
            var route = await _pubCrawlService.UpdateRouteAsync(id, dto, userId);
            
            if (route == null)
                return NotFound(new { message = "Route not found or access denied" });

            return Ok(route);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating route {RouteId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoute(int id)
    {
        try
        {
            var userId = GetUserId();
            var deleted = await _pubCrawlService.DeleteRouteAsync(id, userId);
            
            if (!deleted)
                return NotFound(new { message = "Route not found or access denied" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting route {RouteId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("{id}/stops")]
    public async Task<IActionResult> AddStop(int id, [FromBody] AddStopDto dto)
    {
        try
        {
            var userId = GetUserId();
            var route = await _pubCrawlService.AddStopAsync(id, dto, userId);
            
            if (route == null)
                return NotFound(new { message = "Route not found or access denied" });

            return Ok(route);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding stop to route {RouteId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}/stops/{stopId}")]
    public async Task<IActionResult> DeleteStop(int id, int stopId)
    {
        try
        {
            var userId = GetUserId();
            var deleted = await _pubCrawlService.DeleteStopAsync(id, stopId, userId);
            
            if (!deleted)
                return NotFound(new { message = "Route or stop not found, or access denied" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting stop {StopId} from route {RouteId}", stopId, id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
