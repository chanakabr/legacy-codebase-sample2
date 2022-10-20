
namespace ApiObjects.CanaryDeployment.Microservices
{
    public class MicroservicesCanaryDeploymentDataOwnership
    {
        public CanaryDeploymentAuthenticationMsOwnership AuthenticationMsOwnership { get; set; }
        public CanaryDeploymentSegmentationMsOwnership SegmentationMsOwnership { get; set; }

        public MicroservicesCanaryDeploymentDataOwnership()
        {
            AuthenticationMsOwnership = new CanaryDeploymentAuthenticationMsOwnership();
            SegmentationMsOwnership = new CanaryDeploymentSegmentationMsOwnership();
        }
    }
}
