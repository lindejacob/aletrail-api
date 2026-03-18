using System.ComponentModel.DataAnnotations;

namespace aletrail_api.Models.BuisnessObjects
{
    public class LocationBO
    {
        [Required]
        public double Latitude { get; set; }
        
        [Required]
        public double Longitude { get; set; }

        [Required]
        public int Radius { get; set; }
    }
}

