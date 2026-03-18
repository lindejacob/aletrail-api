using System.ComponentModel.DataAnnotations;

namespace aletrail_api.Models;

public class RouteStop
{
    [Key]
    public int Id { get; set; }
    
    public int PubCrawlRouteId { get; set; }
    public PubCrawlRoute PubCrawlRoute { get; set; } = null!;
    
    public long BarOsmId { get; set; }
    public Bar Bar { get; set; } = null!;
    
    public int OrderIndex { get; set; }
    
    public string? Notes { get; set; }
    
    public int? ChallengeId { get; set; }
    public Challenge? Challenge { get; set; }
}
