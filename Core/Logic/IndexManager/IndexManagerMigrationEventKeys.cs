namespace ApiLogic.Catalog.IndexManager
{
    public class IndexManagerMigrationEventKeys
    {
        public const string MEDIA = "media";
        public const string EPG = "epg";
        public const string RECORDING = "recording";
        public const string CHANNEL = "channel";
        public const string CHANNEL_METADATA = "channelMetedata";
        public const string TAG = "tag";
        public const string STATS = "stats";
        public const string IP_TO_COUNTRY = "ipToCountry";
    }

    public enum IndexManagerVersion
    {
        EsV2=2,
        EsV7=7
    }
    
}