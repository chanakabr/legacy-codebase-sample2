using System;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Utils;
using KLogMonitor;
using WebAPI.Models.CanaryDeployment;
using WebAPI.Models.CanaryDeployment.Elasticsearch;
using WebAPI.Models.CanaryDeployment.Microservices;

namespace WebAPI.Controllers
{
    /// <summary>
    /// elasticsearchCanaryDeploymentConfiguration
    /// </summary>
    [Service("elasticsearchCanaryDeploymentConfiguration", isInternal: true)]
    public class ElasticsearchCanaryDeploymentConfiguration : IKalturaController
    {
        private static readonly KLogger _log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// get elasticsearch canary deployment configuration
        /// </summary>
        /// <param name="groupId">groupId</param>
        /// <returns>elasticsearch canary Deployment configuration</returns>
        [Action("get", isInternal: true)]
        [ApiAuthorize]
        public static KalturaElasticsearchCanaryDeploymentConfiguration Get(int groupId)
        {
            KalturaElasticsearchCanaryDeploymentConfiguration res = null;
            try
            {
                res = ClientsManager.CanaryDeploymentClient().GetElasticsearchCanaryDeploymentConfiguration(groupId);
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
        /// <param name="status">migration event status - true/false</param>
        /// <returns>true if set operation is success, otherwise false</returns>
        [Action("setMigrationEventsStatus", isInternal: true)]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static bool SetMigrationEventsStatus(int groupId, bool status)
        {
            var res = false;
            try
            {
                res = ClientsManager.CanaryDeploymentClient().SetElasticsearchMigrationEventsStatus(groupId, status);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return res;
        }

        /// <summary>
        /// set the current active version of elasticsearch api the system will use
        /// </summary>
        /// <param name="groupId">groupId</param>
        /// <param name="activeVersion">the active version of elasticsearch to set</param>
        /// <returns></returns>
        [ApiAuthorize]
        [Action("setActiveVersion", isInternal: true)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static bool SetActiveVersion(int groupId, KalturaElasticsearchVersion activeVersion)
        {
            var res = false;
            try
            {
                res = ClientsManager.CanaryDeploymentClient().SetElasticsearchActiveVersion(groupId, activeVersion);
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
            var res = false;
            try
            {
                res = ClientsManager.CanaryDeploymentClient().DeleteElasticsearchCanaryDeploymentConfiguration(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return res;
        }

    }
}