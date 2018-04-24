using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/assetRule/action")]
    public class AssetRuleController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Get the list of asset rules for the partner
        /// </summary>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaAssetRuleListResponse List()
        {
            KalturaAssetRuleListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.ApiClient().GetAssetRules(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Add asset rule
        /// </summary>
        /// <param name="assetRule">Asset rule</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaAssetRule Add(KalturaAssetRule assetRule)
        {
            KalturaAssetRule response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.ApiClient().AddAssetRule(groupId, assetRule);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update asset rule
        /// </summary>
        /// <param name="assetRule">Asset rule</param>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public KalturaAssetRule Update(KalturaAssetRule assetRule)
        {
            KalturaAssetRule response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.ApiClient().UpdateAssetRule(groupId, assetRule);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete asset rule
        /// </summary>
        /// <param name="id">Asset rule ID</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public void Delete(long id)
        {
            KalturaAssetRule response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.ApiClient().DeleteAssetRule(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }
    }
}