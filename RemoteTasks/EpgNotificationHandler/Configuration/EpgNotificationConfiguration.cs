namespace EpgNotificationHandler.Configuration
{
    public class EpgNotificationConfiguration : IEpgNotificationConfiguration
    {
        public int CloudFrontInvalidationTtlInMs { get; set; } = 3 * 60 * 1000; // 3 minutes
    }
}