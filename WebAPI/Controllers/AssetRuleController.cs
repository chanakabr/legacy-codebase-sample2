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
    [RoutePrefix("_service/assetRule/action")]
    public class AssetRuleController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        ///  Get the list of asset rules for the partner
        /// </summary>
        /// <param name="filter">filter by condition name</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaAssetRuleListResponse List(KalturaAssetRuleFilter filter = null)
        {
            KalturaAssetRuleListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
            {
                filter = new KalturaAssetRuleFilter();
            }

            try
            {
                filter.Validate();

                response = ClientsManager.ApiClient().GetAssetRules(groupId, filter);
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
                if (string.IsNullOrEmpty(assetRule.Name) || string.IsNullOrWhiteSpace(assetRule.Name))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
                }

                assetRule.Validate();
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
        /// <param name="id">Asset rule ID to update</param>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetRuleNotExists)]
        public KalturaAssetRule Update(long id, KalturaAssetRule assetRule)
        {
            KalturaAssetRule response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                var oldAssetRule = ClientsManager.ApiClient().GetAssetRule(groupId, id);
                // before updating AssetRule fill properties in case they are empty so it will be possible to validate the new properties
                assetRule.FillEmpty(oldAssetRule);
                assetRule.Validate();
                
                response = ClientsManager.ApiClient().UpdateAssetRule(groupId, id, assetRule);
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
        [Throws(eResponseStatus.AssetRuleNotExists)]
        public bool Delete(long id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.ApiClient().DeleteAssetRule(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}