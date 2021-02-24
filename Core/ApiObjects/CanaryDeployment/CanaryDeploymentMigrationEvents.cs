
namespace ApiObjects.CanaryDeployment
{
    public class CanaryDeploymentMigrationEvents
    {
        public bool AppToken { get; set; }
        public bool RefreshToken { get; set; }
        public bool UserPinCode { get; set; }
        public bool DevicePinCode { get; set; }
        public bool SessionRevocation { get; set; }
        public bool UserLoginHistory { get; set; }
        public bool DeviceLoginHistory { get; set; }

        public CanaryDeploymentMigrationEvents()
        {
            AppToken = false;
            RefreshToken = false;
            UserPinCode = false;
            DevicePinCode = false;
            SessionRevocation = false;
            UserLoginHistory = false;
            DeviceLoginHistory = false;
        }

        public CanaryDeploymentMigrationEvents(bool status)
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
