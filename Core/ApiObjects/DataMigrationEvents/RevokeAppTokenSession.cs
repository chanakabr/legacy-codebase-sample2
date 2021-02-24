namespace ApiObjects.DataMigrationEvents
{
    public sealed class RevokeAppTokenSession : BaseDataMigrationEvent
    {
        public string AppTokenId { get; set; }
        public long KsExpiry { get; set; }
        public long SessionRevocationTime { get; set; }
        public new long UserId { get; set; }

        public RevokeAppTokenSession()
        {
            EventNameOverride = "OTT_MIGRATION_REVOKE_APP_TOKEN";
        }
    }
}