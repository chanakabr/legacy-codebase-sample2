using System;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Utils;
using Phx.Lib.Log;
using WebAPI.Models.CanaryDeployment;
using WebAPI.Models.CanaryDeployment.Microservices;

namespace WebAPI.Controllers
{
    /// <summary>
    /// canaryDeploymentConfiguration
    /// </summary>
    [Service("microservicesCanaryDeploymentConfiguration", isInternal: true)]
    public class MicroservicesCanaryDeploymentConfigurationController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// get canary deployment configuration
        /// </summary>
        /// <param name="groupId">groupId</param>
        /// <returns>canary Deployment configuration</returns>
        [Action("get", isInternal: true)]
        [ApiAuthorize]
        public static KalturaMicroservicesCanaryDeploymentConfiguration Get(int groupId)
        {
            KalturaMicroservicesCanaryDeploymentConfiguration res = null;
            try
            {
                res = ClientsManager.CanaryDeploymentClient().GetMicroservicesCanaryDeploymentConfiguration(groupId);
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
        /// <param name="microservicesMigrationEvent">migration event type</param>
        /// <param name="status">migration event status - true/false</param>
        /// <returns></returns>
        [Action("setMigrationEventStatus", isInternal: true)]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public bool SetMigrationEventStatus(int groupId, KalturaCanaryDeploymentMicroservicesMigrationEvent microservicesMigrationEvent, bool status)
        {
            bool res = false;
            try
            {
                res = ClientsManager.CanaryDeploymentClient().SetMicroservicesMigrationEventStatus(groupId, microservicesMigrationEvent, status);
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
        [ValidationException(SchemeValidationType.ACTION_NAME)]/// 
        static public bool SetAllMigrationEventsStatus(int groupId, bool status)
        {
            bool res = false;
            try
            {
                res = ClientsManager.CanaryDeploymentClient().SetAllMicroservicesMigrationEventsStatus(groupId, status);
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
        /// <param name="microservicesRoutingAction">routingAction</param>
        /// <param name="microservicesRoutingService">routingService</param>
        /// <returns></returns>
        [Action("setRoutingAction", isInternal: true)]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public bool SetRoutingAction(int groupId, KalturaCanaryDeploymentMicroservicesRoutingAction microservicesRoutingAction, KalturaCanaryDeploymentMicroservicesRoutingService microservicesRoutingService)
        {
            bool res = false;
            try
            {
                res = ClientsManager.CanaryDeploymentClient().SetMicroservicesRoutingAction(groupId, microservicesRoutingAction, microservicesRoutingService);
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
        public static bool SetAllRoutingActionsToMs(int groupId)
        {
            bool res = false;
            try
            {
                res = ClientsManager.CanaryDeploymentClient().SetAllMicroservicesRoutingActionsToMs(groupId, true);
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
        public static bool SetAllRoutingActionsToPhoenix(int groupId)
        {
            bool res = false;
            try
            {
                res = ClientsManager.CanaryDeploymentClient().SetAllMicroservicesRoutingActionsToMs(groupId, false);
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
        public static bool Delete(int groupId)
        {
            bool res = false;
            try
            {
                res = ClientsManager.CanaryDeploymentClient().DeleteMicroservicesCanaryDeploymentConfiguration(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return res;
        }

    }
}