namespace ApiObjects.DataMigrationEvents
{
    public sealed class DeviceLoginHistory : BaseDataMigrationEvent
    {
        public string Udid { get; set; }
        public long LastLoginDate { get; set; }

        public DeviceLoginHistory()
        {
            EventNameOverride = "OTT_MIGRATION_DEVICE_LOGIN_HISTORY";
        }
    }
}