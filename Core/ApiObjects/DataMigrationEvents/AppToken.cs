using System.Collections.Generic;

namespace ApiObjects.DataMigrationEvents
{
    public sealed class AppToken : BaseDataMigrationEvent
    {
        public long CreateDate { get; set; }
        public long Expiry { get; set; }
        public string HashType { get; set; }
        public string Id { get; set; }
        public long SessionDuration { get; set; }
        public IDictionary<string, string> SessionPrivileges { get; set; }
        public string SessionType { get; set; }
        public string SessionUserId { get; set; }
        public string Token { get; set; }
        public long UpdateDate { get; set; }

        public AppToken()
        {
            EventNameOverride = "OTT_MIGRATION_APP_TOKEN";
        }
    }
}