namespace NotificationService.Options
{
    public class FrontendUrlOptions
    {
        public const string SectionName = "FrontendUrls";

        public string WelcomeLoginUrl { get; set; } = string.Empty;
        public string PasswordResetUrlTemplate { get; set; } = string.Empty;
    }
}
