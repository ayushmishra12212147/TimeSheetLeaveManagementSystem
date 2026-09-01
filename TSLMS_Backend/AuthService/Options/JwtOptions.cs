namespace AuthService.Options
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int AccessTokenExpiryHours { get; set; } = 8;
        public int RefreshTokenExpiryDays { get; set; } = 7;
        public string RefreshCookieName { get; set; } = "ltms_refresh_token";
    }
}
