
namespace ApiObjects.CanaryDeployment
{
    public class CanaryDeploymentAuthenticationMsOwnership
    {
        public bool UserLoginHistory { get; set; }
        public bool DeviceLoginHistory { get; set; }
        public bool SSOAdapterProfiles { get; set; }
        public bool RefreshToken { get; set; }
        public bool DeviceLoginPin { get; set; }
        public bool SessionRevocation { get; set; }

        public CanaryDeploymentAuthenticationMsOwnership()
        {
            UserLoginHistory = false;
            DeviceLoginHistory = false;
            SSOAdapterProfiles = false;
            RefreshToken = false;
            DeviceLoginPin = false;
            SessionRevocation = false;
        }
    }
}
