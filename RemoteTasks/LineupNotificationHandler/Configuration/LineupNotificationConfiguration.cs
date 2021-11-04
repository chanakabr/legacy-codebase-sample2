namespace LineupNotificationHandler.Configuration
{
    public class LineupNotificationConfiguration : ILineupNotificationConfiguration
    {
        public int CloudFrontInvalidationTtlInMs { get; set; } = 3 * 60 * 1000; // 3 minutes
    }
}