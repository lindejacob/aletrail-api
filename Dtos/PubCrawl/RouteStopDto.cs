namespace aletrail_api.Dtos.PubCrawl;

public class RouteStopDto
{
    public int Id { get; set; }
    public long BarOsmId { get; set; }
    public string? BarName { get; set; }
    public double BarLatitude { get; set; }
    public double BarLongitude { get; set; }
    public int OrderIndex { get; set; }
    public string? Notes { get; set; }
    public ChallengeDto? Challenge { get; set; }
}

public class CreateRouteStopDto
{
    public long BarOsmId { get; set; }
    public int OrderIndex { get; set; }
    public string? Notes { get; set; }
    public int? ChallengeId { get; set; }
}

public class UpdateRouteStopDto
{
    public int OrderIndex { get; set; }
    public string? Notes { get; set; }
    public int? ChallengeId { get; set; }
}
