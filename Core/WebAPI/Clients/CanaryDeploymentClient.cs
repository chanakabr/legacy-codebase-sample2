using ApiLogic.CanaryDeployment;
using ApiObjects.CanaryDeployment;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Models.CanaryDeployment;
using WebAPI.ObjectsConvertor.Mapping;

namespace WebAPI.Clients
{
    public class CanaryDeploymentClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public CanaryDeploymentClient()
        {
        }

        public KalturaCanaryDeploymentConfiguration GetCanaryDeploymentConfiguration(int groupId)
        {
            Func<GenericResponse<CanaryDeploymentConfiguration>> getGroupConfiguration = () => CanaryDeploymentFactory.Instance.GetCanaryDeploymentManager().GetGroupConfiguration(groupId);
            return ClientUtils.GetResponseFromWS<KalturaCanaryDeploymentConfiguration, CanaryDeploymentConfiguration>(getGroupConfiguration);
        }

        internal bool SetMigrationEventStatus(int groupId, KalturaCanaryDeploymentMigrationEvent migrationEvent, bool status)
        {
            Func<Status> setMigrationEvent = null;
            CanaryDeploymentMigrationEvent canaryDeploymentMigrationEvent = CanaryDeploymentMapping.ConvertMigrationEvent(migrationEvent);
            if (status)
            {
                setMigrationEvent = () => CanaryDeploymentFactory.Instance.GetCanaryDeploymentManager().EnableMigrationEvent(groupId, canaryDeploymentMigrationEvent);
            }
            else
            {
                setMigrationEvent = () => CanaryDeploymentFactory.Instance.GetCanaryDeploymentManager().DisableMigrationEvent(groupId, canaryDeploymentMigrationEvent);
            }

            return ClientUtils.GetResponseStatusFromWS(setMigrationEvent);
        }

        internal bool SetAllMigrationEventsStatus(int groupId, bool status)
        {
            Func<Status> setAllMigrationEvents = () => CanaryDeploymentFactory.Instance.GetCanaryDeploymentManager().SetAllMigrationEventsStatus(groupId, status);
            return ClientUtils.GetResponseStatusFromWS(setAllMigrationEvents);
        }

        internal bool SetRoutingAction(int groupId, KalturaCanaryDeploymentRoutingAction routingAction, KalturaCanaryDeploymentRoutingService routingService)
        {
            CanaryDeploymentRoutingAction canaryDeploymentRoutingAction = CanaryDeploymentMapping.ConvertRoutingAction(routingAction);
            CanaryDeploymentRoutingService canaryDeploymentRoutingService = CanaryDeploymentMapping.ConvertRoutingService(routingService);
            Func<Status> setRoutingAction = () => CanaryDeploymentFactory.Instance.GetCanaryDeploymentManager().SetRoutingAction(groupId, canaryDeploymentRoutingAction, canaryDeploymentRoutingService);         

            return ClientUtils.GetResponseStatusFromWS(setRoutingAction);
        }

        internal bool SetAllRoutingActionsToMs(int groupId, bool enableMs)
        {
            Func<Status> setAllRoutingActionsToMs = () => CanaryDeploymentFactory.Instance.GetCanaryDeploymentManager().SetAllRoutingActionsToMs(groupId, enableMs);

            return ClientUtils.GetResponseStatusFromWS(setAllRoutingActionsToMs);
        }

        internal bool DeleteCanaryDeploymentConfiguration(int groupId)
        {
            Func<Status> deleteGroupConfiguration = () => CanaryDeploymentFactory.Instance.GetCanaryDeploymentManager().DeleteGroupConfiguration(groupId);
            return ClientUtils.GetResponseStatusFromWS(deleteGroupConfiguration);
        }
    }
}
