namespace LineupNotificationHandler.Configuration
{
    public interface ILineupNotificationConfiguration
    {
        int CloudFrontInvalidationTtlInMs { get; set; }
    }
}