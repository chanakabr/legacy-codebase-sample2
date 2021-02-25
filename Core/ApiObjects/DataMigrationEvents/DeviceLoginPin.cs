using System.ServiceModel.Security.Tokens;
using ApiObjects.User;
using EventBus.Abstraction;

namespace ApiObjects.DataMigrationEvents
{
    public sealed class DeviceLoginPin : BaseDataMigrationEvent
    {
        public string Udid { get; set; }
        public string Pin { get; set; }

        public DeviceLoginPin()
        {
            EventNameOverride = "OTT_MIGRATION_DEVICE_LOGIN_PIN";
        }
    }
}