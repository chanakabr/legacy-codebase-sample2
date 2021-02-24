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
            Func<GenericResponse<CanaryDeploymentConfiguration>> getGroupConfiguration = () => CanaryDeploymentManager.Instance.GetGroupConfiguration(groupId);
            return ClientUtils.GetResponseFromWS<KalturaCanaryDeploymentConfiguration, CanaryDeploymentConfiguration>(getGroupConfiguration);
        }

        internal bool SetMigrationEventStatus(int groupId, KalturaCanaryDeploymentMigrationEvent migrationEvent, bool status)
        {
            Func<Status> setMigrationEvent = null;
            CanaryDeploymentMigrationEvent canaryDeploymentMigrationEvent = CanaryDeploymentMapping.ConvertMigrationEvent(migrationEvent);
            if (status)
            {
                setMigrationEvent = () => CanaryDeploymentManager.Instance.EnableMigrationEvent(groupId, canaryDeploymentMigrationEvent);
            }
            else
            {
                setMigrationEvent = () => CanaryDeploymentManager.Instance.DisableMigrationEvent(groupId, canaryDeploymentMigrationEvent);
            }

            return ClientUtils.GetResponseStatusFromWS(setMigrationEvent);
        }

        internal bool SetAllMigrationEventsStatus(int groupId, bool status)
        {
            Func<Status> setAllMigrationEvents = () => CanaryDeploymentManager.Instance.SetAllMigrationEventsStatus(groupId, status);
            return ClientUtils.GetResponseStatusFromWS(setAllMigrationEvents);
        }

        internal bool SetRoutingAction(int groupId, KalturaCanaryDeploymentRoutingAction routingAction, KalturaCanaryDeploymentRoutingService routingService)
        {
            CanaryDeploymentRoutingAction canaryDeploymentRoutingAction = CanaryDeploymentMapping.ConvertRoutingAction(routingAction);
            CanaryDeploymentRoutingService canaryDeploymentRoutingService = CanaryDeploymentMapping.ConvertRoutingService(routingService);
            Func<Status> setRoutingAction = () => CanaryDeploymentManager.Instance.SetRoutingAction(groupId, canaryDeploymentRoutingAction, canaryDeploymentRoutingService);         

            return ClientUtils.GetResponseStatusFromWS(setRoutingAction);
        }

        internal bool SetAllRoutingActions(int groupId, KalturaCanaryDeploymentRoutingService routingService)
        {
            CanaryDeploymentRoutingService canaryDeploymentRoutingService = CanaryDeploymentMapping.ConvertRoutingService(routingService);
            Func<Status> setAllRoutingActions = () => CanaryDeploymentManager.Instance.SetAllRoutingActions(groupId, canaryDeploymentRoutingService);

            return ClientUtils.GetResponseStatusFromWS(setAllRoutingActions);
        }

        internal bool DeleteCanaryDeploymentConfiguration(int groupId)
        {
            Func<Status> deleteGroupConfiguration = () => CanaryDeploymentManager.Instance.DeleteGroupConfiguration(groupId);
            return ClientUtils.GetResponseStatusFromWS(deleteGroupConfiguration);
        }
    }
}
