namespace WebAPI.Models.API
{
    public enum KalturaChannelEnrichment
    {
        ClientLocation = 1,
        UserId = 2,
        HouseholdId = 4,
        DeviceId = 8,
        DeviceType = 16,
        UTCOffset = 32,
        Language = 64,
        DTTRegion = 1024,
    }
}