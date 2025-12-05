namespace Taskify.Services.Utilities
{
    public class JwtConfig
    {
        public string SigningKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audiences { get; set; } = string.Empty;
        public int ExpirationTime { get; set; }
    }
}
