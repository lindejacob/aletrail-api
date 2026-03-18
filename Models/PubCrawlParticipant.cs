using System.ComponentModel.DataAnnotations;

namespace aletrail_api.Models;

public class PubCrawlParticipant
{
    [Key]
    public int Id { get; set; }
    
    public int PubCrawlRouteId { get; set; }
    public PubCrawlRoute PubCrawlRoute { get; set; } = null!;
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public DateTime JoinedAt { get; set; }
}
