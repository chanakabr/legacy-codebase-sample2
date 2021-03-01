using Newtonsoft.Json;

namespace ApiObjects.DataMigrationEvents
{
    public class RefreshToken : BaseDataMigrationEvent
    {
        public long ExpirationDate { get; set; }
        
        // cannot name property with same name of the class :\
        [JsonProperty("refreshToken")]
        public string Token { get; set; }
        
        public string SessionHash { get; set; }
        public long Ttl { get; set; }
        public string Udid { get; set; }
        public new long UserId { get; set; }

        public RefreshToken()
        {
            EventNameOverride = "OTT_MIGRATION_REFRESH_TOKEN";
        }
    }
}