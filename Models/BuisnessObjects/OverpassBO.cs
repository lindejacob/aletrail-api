using System.Text.Json.Serialization;

namespace aletrail_api.Models.BuisnessObjects;

public class OverpassResponseBO
{
    [JsonPropertyName("elements")]
    public List<OverpassElementBO>? Elements { get; set; }
}

public class OverpassElementBO
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("lat")]
    public double? Lat { get; set; }

    [JsonPropertyName("lon")]
    public double? Lon { get; set; }

    [JsonPropertyName("center")]
    public OverpassCenterBO? Center { get; set; }

    [JsonPropertyName("tags")]
    public Dictionary<string, string>? Tags { get; set; }
}

public class OverpassCenterBO
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lon")]
    public double Lon { get; set; }
}
