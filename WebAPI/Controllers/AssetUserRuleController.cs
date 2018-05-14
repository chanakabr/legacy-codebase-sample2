using ApiObjects.Response;
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
    [RoutePrefix("_service/assetUserRule/action")]
    public class AssetUserRuleController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Get the list of asset user rules for the partner
        /// </summary>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaAssetUserRuleListResponse List()
        {
            KalturaAssetUserRuleListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.ApiClient().GetAssetUserRules(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Add asset user rule
        /// </summary>
        /// <param name="assetUserRule">Asset user rule</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaAssetUserRule Add(KalturaAssetUserRule assetUserRule)
        {
            KalturaAssetUserRule response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (string.IsNullOrEmpty(assetUserRule.Name))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
                }

                assetUserRule.ValidateActions();
                assetUserRule.ValidateConditions();
                
                response = ClientsManager.ApiClient().AddAssetUserRule(groupId, assetUserRule);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update asset user rule
        /// </summary>
        /// <param name="assetUserRule">Asset user rule</param>
        /// <param name="id">Asset user rule ID to update</param>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetUserRuleDoesNotExists)]
        [SchemeArgument("id", MinLong = 1)]
        public KalturaAssetUserRule Update(long id, KalturaAssetUserRule assetUserRule)
        {
            KalturaAssetUserRule response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                assetUserRule.ValidateConditions();

                response = ClientsManager.ApiClient().UpdateAssetUserRule(groupId, id, assetUserRule);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete asset user rule
        /// </summary>
        /// <param name="id">Asset user rule ID</param>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetUserRuleDoesNotExists)]
        [SchemeArgument("id", MinLong = 1)]
        public bool Delete(long id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.ApiClient().DeleteAssetUserRule(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}