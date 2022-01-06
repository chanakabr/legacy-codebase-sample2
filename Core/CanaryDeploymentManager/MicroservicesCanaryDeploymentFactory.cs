using System;
using System.Threading;
using ApiObjects.CanaryDeployment.Microservices;
using ApiObjects.Response;


namespace CanaryDeploymentManager
{
    public class CanaryDeploymentFactory
    {

        private bool isCanaryDeploymentConfigurationEnabled;

        private static readonly Lazy<CanaryDeploymentFactory> lazy = new Lazy<CanaryDeploymentFactory>(() => new CanaryDeploymentFactory(), LazyThreadSafetyMode.PublicationOnly);

        public static CanaryDeploymentFactory Instance { get { return lazy.Value; } }

        private CanaryDeploymentFactory()
        {
            isCanaryDeploymentConfigurationEnabled = Phx.Lib.Appconfig.ApplicationConfiguration.Current.MicroservicesClientConfiguration.ShouldAllowCanaryDeploymentConfiguration.Value;
        }

        public IMicroservicesCanaryDeploymentManager GetMicroservicesCanaryDeploymentManager()
        {
            IMicroservicesCanaryDeploymentManager manager = null;
            if (isCanaryDeploymentConfigurationEnabled)
            {
                manager = MicroservicesMicroservicesCanaryDeploymentManager.Instance;
            }
            else
            {
                manager = BaseMicroservicesCanaryDeploymentManager.Instance;
            }

            return manager;
        }

        public IElasticsearchCanaryDeploymentManager GetElasticsearchCanaryDeploymentManager()
        {
            return ElasticsearchCanaryDeploymentManager.Instance;
        }

    }


    // a class to return "default" responses when canary deployment configuration isn't enabled
    public class BaseMicroservicesCanaryDeploymentManager : IMicroservicesCanaryDeploymentManager
    {
        private const string DEFAULT_ERROR_MSG = "Canary Deployment Configuration is disabled on the environment, to enable add should_allow_canary_deployment_configuration: true under microservices_client_configuration TCM configuration";
        private static readonly Lazy<BaseMicroservicesCanaryDeploymentManager> lazy = new Lazy<BaseMicroservicesCanaryDeploymentManager>(() => new BaseMicroservicesCanaryDeploymentManager(), LazyThreadSafetyMode.PublicationOnly);

        private readonly Status defaultResponseStatus;

        internal static BaseMicroservicesCanaryDeploymentManager Instance { get { return lazy.Value; } }

        private BaseMicroservicesCanaryDeploymentManager()
        {
            defaultResponseStatus = new Status(eResponseStatus.CanaryDeploymentConfigurationIsDisabledOnTheEnvironment, DEFAULT_ERROR_MSG);
        }

        public Status DeleteGroupConfiguration(int groupId)
        {
            return defaultResponseStatus;
        }

        public Status DisableMigrationEvent(int groupId, CanaryDeploymentMigrationEvent migrationEvent)
        {
            return defaultResponseStatus;
        }

        public Status EnableMigrationEvent(int groupId, CanaryDeploymentMigrationEvent migrationEvent)
        {
            return defaultResponseStatus;
        }

        public GenericResponse<MicroservicesCanaryDeploymentConfiguration> GetGroupConfiguration(int groupId)
        {
            return new GenericResponse<MicroservicesCanaryDeploymentConfiguration>(defaultResponseStatus, null);
        }

        public bool IsDataOwnershipFlagEnabled(int groupId, CanaryDeploymentDataOwnershipEnum ownershipFlag)
        {
            return false;
        }

        public bool IsEnabledMigrationEvent(int groupId, CanaryDeploymentMigrationEvent migrationEvent)
        {
            return false;
        }

        public Status SetAllMigrationEventsStatus(int groupId, bool status)
        {
            return defaultResponseStatus;
        }

        public Status SetAllRoutingActionsToMs(int groupId, bool enableMs)
        {
            return defaultResponseStatus;
        }

        public Status SetRoutingAction(int groupId, CanaryDeploymentRoutingAction routingAction, MicroservicesCanaryDeploymentRoutingService routingService)
        {
            return defaultResponseStatus;
        }
    }
}
