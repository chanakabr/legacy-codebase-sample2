
namespace ApiObjects.CanaryDeployment.Microservices
{
    public class MicroservicesCanaryDeploymentMigrationEvents
    {
        public bool AppToken { get; set; }
        public bool RefreshToken { get; set; }
        public bool UserPinCode { get; set; }
        public bool DevicePinCode { get; set; }
        public bool SessionRevocation { get; set; }
        public bool UserLoginHistory { get; set; }
        public bool DeviceLoginHistory { get; set; }

        public MicroservicesCanaryDeploymentMigrationEvents()
        {
            AppToken = false;
            RefreshToken = false;
            UserPinCode = false;
            DevicePinCode = false;
            SessionRevocation = false;
            UserLoginHistory = false;
            DeviceLoginHistory = false;
        }

        public MicroservicesCanaryDeploymentMigrationEvents(bool status)
        {
            AppToken = status;
            RefreshToken = status;
            UserPinCode = status;
            DevicePinCode = status;
            SessionRevocation = status;
            UserLoginHistory = status;
            DeviceLoginHistory = status;
        }
    }
}
