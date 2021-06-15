using ApiObjects.CanaryDeployment;
using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ApiLogic.CanaryDeployment
{
    public class CanaryDeploymentFactory
    {

        private bool isCanaryDeploymentConfigurationEnabled;

        private static readonly Lazy<CanaryDeploymentFactory> lazy = new Lazy<CanaryDeploymentFactory>(() => new CanaryDeploymentFactory(), LazyThreadSafetyMode.PublicationOnly);

        public static CanaryDeploymentFactory Instance { get { return lazy.Value; } }

        private CanaryDeploymentFactory()
        {
            isCanaryDeploymentConfigurationEnabled = ConfigurationManager.ApplicationConfiguration.Current.MicroservicesClientConfiguration.ShouldAllowCanaryDeploymentConfiguration.Value;
        }

        public ICanaryDeploymentManager GetCanaryDeploymentManager()
        {
            ICanaryDeploymentManager manager = null;
            if (isCanaryDeploymentConfigurationEnabled)
            {
                manager = CanaryDeploymentManager.Instance;
            }
            else
            {
                manager = BaseCanaryDeploymentManager.Instance;
            }

            return manager;
        }

    }


    // a class to return "default" responses when canary deployment configuration isn't enabled
    public class BaseCanaryDeploymentManager : ICanaryDeploymentManager
    {
        private const string DEFAULT_ERROR_MSG = "Canary Deployment Configuration is disabled on the environment, to enable add should_allow_canary_deployment_configuration: true under microservices_client_configuration TCM configuration";
        private static readonly Lazy<BaseCanaryDeploymentManager> lazy = new Lazy<BaseCanaryDeploymentManager>(() => new BaseCanaryDeploymentManager(), LazyThreadSafetyMode.PublicationOnly);

        private readonly Status defaultResponseStatus;

        internal static BaseCanaryDeploymentManager Instance { get { return lazy.Value; } }

        private BaseCanaryDeploymentManager()
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

        public GenericResponse<CanaryDeploymentConfiguration> GetGroupConfiguration(int groupId)
        {
            return new GenericResponse<CanaryDeploymentConfiguration>(defaultResponseStatus, null);
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

        public Status SetRoutingAction(int groupId, CanaryDeploymentRoutingAction routingAction, CanaryDeploymentRoutingService routingService)
        {
            return defaultResponseStatus;
        }
    }
}
