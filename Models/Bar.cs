namespace aletrail_api.Models;

public class Bar
{
    public long OsmId { get; set; }
    public string? Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime SyncedAt { get; set; }
}
