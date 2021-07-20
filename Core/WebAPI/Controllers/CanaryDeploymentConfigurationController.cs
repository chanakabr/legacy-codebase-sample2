using System;
using System.Reflection;
using ApiObjects.Response;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Utils;
using KLogMonitor;
using WebAPI.Models.CanaryDeployment;

namespace WebAPI.Controllers
{
    /// <summary>
    /// canaryDeploymentConfiguration
    /// </summary>
    [Service("canaryDeploymentConfiguration", isInternal: true)]
    public class CanaryDeploymentConfigurationController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// get canary deployment configuration
        /// </summary>
        /// <param name="groupId">groupId</param>
        /// <returns>canary Deployment configuration</returns>
        [Action("get", isInternal: true)]
        [ApiAuthorize]
        [Throws(eResponseStatus.GroupCanaryDeploymentConfigurationNotSetYet)]
        [Throws(eResponseStatus.CanaryDeploymentConfigurationIsDisabledOnTheEnvironment)]
        public static KalturaCanaryDeploymentConfiguration Get(int groupId)
        {
            KalturaCanaryDeploymentConfiguration res = null;
            try
            {
                res = ClientsManager.CanaryDeploymentClient().GetCanaryDeploymentConfiguration(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return res;
        }

        /// <summary>
        /// set migration event status for group
        /// </summary>
        /// <param name="groupId">groupId</param>
        /// <param name="migrationEvent">migration event type</param>
        /// <param name="status">migration event status - true/false</param>
        /// <returns></returns>
        [Action("setMigrationEventStatus", isInternal: true)]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.FailedToEnableCanaryDeploymentMigrationEvent)]
        [Throws(eResponseStatus.FailedToDisableCanaryDeploymentMigrationEvent)]
        [Throws(eResponseStatus.CanaryDeploymentConfigurationIsDisabledOnTheEnvironment)]
        static public bool SetMigrationEventStatus(int groupId, KalturaCanaryDeploymentMigrationEvent migrationEvent, bool status)
        {
            bool res = false;
            try
            {
                res = ClientsManager.CanaryDeploymentClient().SetMigrationEventStatus(groupId, migrationEvent, status);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return res;
        }

        /// <summary>
        /// set all migration events status by groupId
        /// </summary>
        /// <param name="groupId">groupId</param>
        /// <param name="status">status</param>
        /// <returns></returns>
        [ApiAuthorize]
        [Action("setAllMigrationEventsStatus", isInternal: true)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.FailedToSetAllGroupCanaryDeploymentMigrationEventsStatus)]
        [Throws(eResponseStatus.CanaryDeploymentConfigurationIsDisabledOnTheEnvironment)]
        static public bool SetAllMigrationEventsStatus(int groupId, bool status)
        {
            bool res = false;
            try
            {
                res = ClientsManager.CanaryDeploymentClient().SetAllMigrationEventsStatus(groupId, status);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return res;
        }

        /// <summary>
        /// set routing action for group
        /// </summary>
        /// <param name="groupId">groupId></param>
        /// <param name="routingAction">routingAction</param>
        /// <param name="routingService">routingService</param>
        /// <returns></returns>
        [Action("setRoutingAction", isInternal: true)]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.FailedToSetAllRoutingActions)]
        [Throws(eResponseStatus.FailedToSetRouteAppTokenController)]
        [Throws(eResponseStatus.FailedToSetRouteUserLoginPinController)]
        [Throws(eResponseStatus.FailedToSetRouteSessionController)]
        [Throws(eResponseStatus.FailedToSetRouteHouseHoldDevicePinActions)]
        [Throws(eResponseStatus.FailedToSetRouteRefreshToken)]
        [Throws(eResponseStatus.CanaryDeploymentConfigurationIsDisabledOnTheEnvironment)]
        static public bool SetRoutingAction(int groupId, KalturaCanaryDeploymentRoutingAction routingAction, KalturaCanaryDeploymentRoutingService routingService)
        {
            bool res = false;
            try
            {
                res = ClientsManager.CanaryDeploymentClient().SetRoutingAction(groupId, routingAction, routingService);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return res;
        }

        /// <summary>
        /// set all routing actions for groupId to MS
        /// </summary>
        /// <param name="groupId">groupId</param>
        /// <returns></returns>
        [ApiAuthorize]
        [Action("setAllRoutingActionsToMs", isInternal: true)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.FailedToSetAllRoutingActions)]
        [Throws(eResponseStatus.FailedToSetRouteAppTokenController)]
        [Throws(eResponseStatus.FailedToSetRouteUserLoginPinController)]
        [Throws(eResponseStatus.FailedToSetRouteSessionController)]
        [Throws(eResponseStatus.FailedToSetRouteHouseHoldDevicePinActions)]
        [Throws(eResponseStatus.FailedToSetRouteRefreshToken)]
        [Throws(eResponseStatus.CanaryDeploymentConfigurationIsDisabledOnTheEnvironment)]
        public static bool SetAllRoutingActionsToMs(int groupId)
        {
            bool res = false;
            try
            {
                res = ClientsManager.CanaryDeploymentClient().SetAllRoutingActionsToMs(groupId, true);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return res;
        }
        
        /// <summary>
        /// set all routing actions for groupId to phoenix
        /// </summary>
        /// <param name="groupId">groupId</param>
        /// <returns></returns>
        [ApiAuthorize]
        [Action("setAllRoutingActionsToPhoenix", isInternal: true)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.FailedToSetAllRoutingActions)]
        [Throws(eResponseStatus.FailedToSetRouteAppTokenController)]
        [Throws(eResponseStatus.FailedToSetRouteUserLoginPinController)]
        [Throws(eResponseStatus.FailedToSetRouteSessionController)]
        [Throws(eResponseStatus.FailedToSetRouteHouseHoldDevicePinActions)]
        [Throws(eResponseStatus.FailedToSetRouteRefreshToken)]
        [Throws(eResponseStatus.CanaryDeploymentConfigurationIsDisabledOnTheEnvironment)]
        public static bool SetAllRoutingActionsToPhoenix(int groupId)
        {
            bool res = false;
            try
            {
                res = ClientsManager.CanaryDeploymentClient().SetAllRoutingActionsToMs(groupId, false);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return res;
        }        

        /// <summary>
        /// delete canary deployment configuration
        /// </summary>
        /// <param name="groupId">groupId</param>
        /// <returns>canary Deployment configuration</returns>
        [Action("delete", isInternal: true)]
        [ApiAuthorize]
        [Throws(eResponseStatus.FailedToDeleteGroupCanaryDeploymentConfiguration)]
        [Throws(eResponseStatus.CanaryDeploymentConfigurationIsDisabledOnTheEnvironment)]
        public static bool Delete(int groupId)
        {
            bool res = false;
            try
            {
                res = ClientsManager.CanaryDeploymentClient().DeleteCanaryDeploymentConfiguration(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return res;
        }

    }
}