using aletrail_api.Models;

namespace aletrail_api.Services.PubCrawl;

public class RouteCalculationService : IRouteCalculationService
{
    private const double EarthRadiusKm = 6371.0;

    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    public List<Bar> FindNearbyBars(List<Bar> allBars, double centerLat, double centerLon, double radiusKm, int limit)
    {
        return allBars
            .Select(bar => new
            {
                Bar = bar,
                Distance = CalculateDistance(centerLat, centerLon, bar.Latitude, bar.Longitude)
            })
            .Where(x => x.Distance <= radiusKm)
            .OrderBy(x => x.Distance)
            .Take(limit)
            .Select(x => x.Bar)
            .ToList();
    }

    public List<Bar> OptimizeRoute(List<Bar> bars, double startLat, double startLon, double endLat, double endLon)
    {
        if (bars.Count == 0) return bars;
        if (bars.Count == 1) return bars;

        var unvisited = new HashSet<Bar>(bars);
        var route = new List<Bar>();
        
        var currentLat = startLat;
        var currentLon = startLon;

        while (unvisited.Count > 0)
        {
            Bar? nearest = null;
            double minDistance = double.MaxValue;

            foreach (var bar in unvisited)
            {
                double distanceToBar = CalculateDistance(currentLat, currentLon, bar.Latitude, bar.Longitude);
                double distanceToEnd = CalculateDistance(bar.Latitude, bar.Longitude, endLat, endLon);
                
                double heuristic = distanceToBar + distanceToEnd * 0.5;

                if (heuristic < minDistance)
                {
                    minDistance = heuristic;
                    nearest = bar;
                }
            }

            if (nearest != null)
            {
                route.Add(nearest);
                unvisited.Remove(nearest);
                currentLat = nearest.Latitude;
                currentLon = nearest.Longitude;
            }
        }

        route = Apply2OptImprovement(route, startLat, startLon, endLat, endLon);

        return route;
    }

    public double CalculateTotalDistance(List<Bar> orderedBars, double? startLat = null, double? startLon = null, double? endLat = null, double? endLon = null)
    {
        if (orderedBars.Count == 0) return 0;

        double totalDistance = 0;

        if (startLat.HasValue && startLon.HasValue)
        {
            totalDistance += CalculateDistance(startLat.Value, startLon.Value, 
                orderedBars[0].Latitude, orderedBars[0].Longitude);
        }

        for (int i = 0; i < orderedBars.Count - 1; i++)
        {
            totalDistance += CalculateDistance(
                orderedBars[i].Latitude, orderedBars[i].Longitude,
                orderedBars[i + 1].Latitude, orderedBars[i + 1].Longitude
            );
        }

        if (endLat.HasValue && endLon.HasValue)
        {
            var lastBar = orderedBars[^1];
            totalDistance += CalculateDistance(lastBar.Latitude, lastBar.Longitude, 
                endLat.Value, endLon.Value);
        }

        return totalDistance;
    }

    private List<Bar> Apply2OptImprovement(List<Bar> route, double startLat, double startLon, double endLat, double endLon)
    {
        if (route.Count < 3) return route;

        var improved = true;
        var bestRoute = new List<Bar>(route);

        while (improved)
        {
            improved = false;
            var bestDistance = CalculateTotalDistance(bestRoute, startLat, startLon, endLat, endLon);

            for (int i = 1; i < bestRoute.Count - 1; i++)
            {
                for (int j = i + 1; j < bestRoute.Count; j++)
                {
                    var newRoute = TwoOptSwap(bestRoute, i, j);
                    var newDistance = CalculateTotalDistance(newRoute, startLat, startLon, endLat, endLon);

                    if (newDistance < bestDistance)
                    {
                        bestRoute = newRoute;
                        bestDistance = newDistance;
                        improved = true;
                    }
                }
            }
        }

        return bestRoute;
    }

    private List<Bar> TwoOptSwap(List<Bar> route, int i, int j)
    {
        var newRoute = new List<Bar>();
        
        for (int k = 0; k < i; k++)
            newRoute.Add(route[k]);
        
        for (int k = j; k >= i; k--)
            newRoute.Add(route[k]);
        
        for (int k = j + 1; k < route.Count; k++)
            newRoute.Add(route[k]);

        return newRoute;
    }

    private double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}
