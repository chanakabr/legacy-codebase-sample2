namespace ApiObjects.DataMigrationEvents
{
    public sealed class RevokeKs : BaseDataMigrationEvent
    {
        public string Ks { get; set; }
        public long KsExpiry { get; set; }
        
        public RevokeKs()
        {
            EventNameOverride = "OTT_MIGRATION_REVOKE_KS";
        }
    }
}