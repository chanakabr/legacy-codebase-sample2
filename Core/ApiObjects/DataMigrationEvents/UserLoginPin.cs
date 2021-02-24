namespace ApiObjects.DataMigrationEvents
{
    public sealed class UserLoginPin : BaseDataMigrationEvent
    {
        public long? ExpirationTime { get; set; }
        public string Pin { get; set; }
        public int? RemainingUsages { get; set; }
        public string Secret { get; set; }
        public new long UserId { get; set; }

        public UserLoginPin()
        {
            EventNameOverride = "OTT_MIGRATION_USER_LOGIN_PIN";
        }
    }
}