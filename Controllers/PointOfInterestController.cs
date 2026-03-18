using Microsoft.AspNetCore.Mvc;
using aletrail_api.Models.BuisnessObjects;
using aletrail_api.Services.PointOfInterest;

namespace aletrail_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PointOfInterestController : ControllerBase
{
    private readonly IPOIService _poiService;

    public PointOfInterestController(IPOIService poiService)
    {
        _poiService = poiService;
    }

    [HttpPost("GetBarsNearby")]
    public async Task<IActionResult> GetBarsNearby([FromBody] LocationBO location)
    {
        var bars = await _poiService.GetBarsNearby(location);
        return Ok(bars);
    }
}