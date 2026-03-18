using aletrail_api.Dtos.PointOfInterest;
using aletrail_api.Models.BuisnessObjects;

namespace aletrail_api.Services.PointOfInterest;

public interface IPOIService
{
    Task<List<BarDto>> GetBarsNearby(LocationBO location);
}