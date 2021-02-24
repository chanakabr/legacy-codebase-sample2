
namespace ApiObjects.CanaryDeployment
{
    public class CanaryDeploymentDataOwnership
    {
        public CanaryDeploymentAuthenticationMsOwnership AuthenticationMsOwnership { get; set; }

        public CanaryDeploymentDataOwnership()
        {
            AuthenticationMsOwnership = new CanaryDeploymentAuthenticationMsOwnership();
        }
    }
}
