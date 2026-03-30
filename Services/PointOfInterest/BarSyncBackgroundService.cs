using System.Text;
using System.Text.Json;
using aletrail_api.DAL;
using aletrail_api.Models;
using aletrail_api.Models.BuisnessObjects;
using Microsoft.EntityFrameworkCore;

namespace aletrail_api.Services.PointOfInterest;

public class BarSyncBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BarSyncBackgroundService> _logger;
    private static readonly TimeSpan SyncInterval = TimeSpan.FromHours(24);

    private static readonly string[] OverpassUrls =
    [
        "https://overpass-api.de/api/interpreter",
        "https://overpass.kumi.systems/api/interpreter",
        "https://maps.mail.ru/osm/tools/overpass/api/interpreter"
    ];

    public BarSyncBackgroundService(IServiceProvider serviceProvider, ILogger<BarSyncBackgroundService> logger, IHostEnvironment env)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await SyncAsync(stoppingToken);
            await Task.Delay(SyncInterval, stoppingToken);
        }
    }

    private async Task SyncAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Denmark bars sync...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (_env.IsDevelopment())
            {
                var firstBar = await dbContext.Bars
                    .OrderBy(b => b.OsmId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (firstBar != null)
                {
                    var timeSinceSync = DateTime.UtcNow - firstBar.SyncedAt;
                    if (timeSinceSync < SyncInterval)
                    {
                        _logger.LogInformation(
                            "Skipping sync in development. Last sync was {Hours:F1} hours ago (interval: {IntervalHours} hours)",
                            timeSinceSync.TotalHours,
                            SyncInterval.TotalHours);
                        return;
                    }
                }
            }

            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("overpass");

            var bars = await FetchAllDenmarkBarsAsync(httpClient);
            var syncedAt = DateTime.UtcNow;

            _logger.LogInformation("Saving {Count} bars to database...", bars.Count);

            await dbContext.Bars.ExecuteDeleteAsync(cancellationToken);
            await dbContext.Bars.AddRangeAsync(bars.Select(b => new Bar
            {
                OsmId = b.OsmId,
                Name = b.Name,
                Latitude = b.Latitude,
                Longitude = b.Longitude,
                SyncedAt = syncedAt
            }), cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Denmark bars sync complete. Next sync in 24 hours.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Denmark bars sync failed.");
        }
    }

    private async Task<List<(long OsmId, string? Name, double Latitude, double Longitude)>> FetchAllDenmarkBarsAsync(HttpClient httpClient)
    {
        _logger.LogInformation("Downloading bars from Overpass API...");
        var query = """
            [out:json][timeout:90];
            area["ISO3166-1"="DK"][admin_level=2]->.denmark;
            (
              node["amenity"~"bar|pub"]["access"!="private"]["name"](area.denmark);
              way["amenity"~"bar|pub"]["access"!="private"]["name"](area.denmark);
            );
            out center;
            """;

        HttpResponseMessage? response = null;
        foreach (var url in OverpassUrls)
        {
            _logger.LogInformation("Trying {Url}...", url);
            var content = new StringContent($"data={Uri.EscapeDataString(query)}", Encoding.UTF8, "application/x-www-form-urlencoded");
            response = await httpClient.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Success from {Url}", url);
                break;
            }
            _logger.LogWarning("Failed ({StatusCode}) from {Url}, trying next...", (int)response.StatusCode, url);
        }

        response!.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var overpassResponse = JsonSerializer.Deserialize<OverpassResponseBO>(json);

        if (overpassResponse?.Elements is null)
            return [];

        return overpassResponse.Elements
            .Select(e => (
                OsmId: e.Id,
                Name: e.Tags?.GetValueOrDefault("name"),
                Latitude: e.Type == "way" ? e.Center?.Lat ?? 0 : e.Lat ?? 0,
                Longitude: e.Type == "way" ? e.Center?.Lon ?? 0 : e.Lon ?? 0
            ))
            .ToList();
    }
}
