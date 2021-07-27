
namespace ApiObjects.CanaryDeployment.Microservices
{
    public class MicroservicesCanaryDeploymentDataOwnership
    {
        public CanaryDeploymentAuthenticationMsOwnership AuthenticationMsOwnership { get; set; }

        public MicroservicesCanaryDeploymentDataOwnership()
        {
            AuthenticationMsOwnership = new CanaryDeploymentAuthenticationMsOwnership();
        }
    }
}
