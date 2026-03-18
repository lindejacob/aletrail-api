namespace aletrail_api.Dtos.PubCrawl;

public class PubCrawlRouteDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int CreatedByUserId { get; set; }
    public string CreatedByUsername { get; set; } = null!;
    public string? InviteCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<RouteStopDto> Stops { get; set; } = new();
    public List<ParticipantDto> Participants { get; set; } = new();
    public double? TotalDistanceKm { get; set; }
    public bool IsOwner { get; set; }
}

public class ParticipantDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public DateTime JoinedAt { get; set; }
}

public class JoinRouteDto
{
    public string InviteCode { get; set; } = null!;
}

public class CreateManualRouteDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public List<CreateRouteStopDto> Stops { get; set; } = new();
}

public class GenerateRouteDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public double StartLatitude { get; set; }
    public double StartLongitude { get; set; }
    public double EndLatitude { get; set; }
    public double EndLongitude { get; set; }
    public int NumberOfBars { get; set; }
}

public class AddStopDto
{
    public long BarOsmId { get; set; }
    public int? OrderIndex { get; set; }
    public string? Notes { get; set; }
    public int? ChallengeId { get; set; }
}

public class UpdateRouteDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}
