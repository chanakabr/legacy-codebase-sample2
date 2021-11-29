namespace EpgNotificationHandler.Configuration
{
    public interface IEpgNotificationConfiguration
    {
        int CloudFrontInvalidationTtlInMs { get; set; }
    }
}