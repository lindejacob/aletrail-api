using aletrail_api.DAL;
using aletrail_api.Dtos.PointOfInterest;
using aletrail_api.Models.BuisnessObjects;
using Microsoft.EntityFrameworkCore;

namespace aletrail_api.Services.PointOfInterest;

public class POIService : IPOIService
{
    private readonly ApplicationDbContext _dbContext;

    public POIService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<BarDto>> GetBarsNearby(LocationBO location)
    {
        const double metersPerDegreeLat = 111000.0;
        double latRad = Math.PI / 180.0 * location.Latitude;
        double deltaLat = location.Radius / metersPerDegreeLat;
        double deltaLon = location.Radius / (metersPerDegreeLat * Math.Cos(latRad));

        var candidates = await _dbContext.Bars
            .AsNoTracking()
            .Where(b =>
                b.Latitude >= location.Latitude - deltaLat &&
                b.Latitude <= location.Latitude + deltaLat &&
                b.Longitude >= location.Longitude - deltaLon &&
                b.Longitude <= location.Longitude + deltaLon)
            .ToListAsync();

        return candidates
            .Where(b => Haversine(location.Latitude, location.Longitude, b.Latitude, b.Longitude) <= location.Radius)
            .Select(b => new BarDto
            {
                Id = b.OsmId,
                Name = b.Name,
                Latitude = b.Latitude,
                Longitude = b.Longitude
            })
            .ToList();
    }

    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000;
        double dLat = (lat2 - lat1) * Math.PI / 180.0;
        double dLon = (lon2 - lon1) * Math.PI / 180.0;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                 + Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0)
                 * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
