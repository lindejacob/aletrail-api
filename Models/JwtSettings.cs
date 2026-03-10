namespace aletrail_api.Models
{
    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty;
        
        public int ExpirationMinutes { get; set; } = 60;
    }
}

