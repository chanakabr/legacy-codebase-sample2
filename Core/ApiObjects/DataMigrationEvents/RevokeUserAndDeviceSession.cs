namespace ApiObjects.DataMigrationEvents
{
    public sealed class RevokeUserAndDeviceSession : BaseDataMigrationEvent
    {
        public long KsExpiry { get; set; }
        public string Udid { get; set; }
        public long UserDeviceSessionCreationDate { get; set; }
        public new long UserId { get; set; }
        
        public RevokeUserAndDeviceSession()
        {
            EventNameOverride = "OTT_MIGRATION_REVOKE_USER_DEVICE_SESSION";
        }
    }
}