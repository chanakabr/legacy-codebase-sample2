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
                if (string.IsNullOrEmpty(assetRule.Name))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
                }

                if (assetRule.Actions == null)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "actions");
                }

                if (assetRule.Conditions == null)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "conditions");
                }

                ValidateAssetRuleActions(assetRule);

                ValidateAssetRuleConditions(assetRule.Conditions);


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
                ValidateAssetRuleConditions(assetRule.Conditions);

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

        private void ValidateAssetRuleActions(KalturaAssetRule assetRule)
        {
            if (assetRule != null && assetRule.Actions != null)
            {
                var duplicates = assetRule.Actions.GroupBy(x => x.Type).Where(t => t.Count() >= 2);
                if (duplicates != null && duplicates.ToList().Count > 1)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "actions");
                }

                var ruleActionBlock = assetRule.Actions.Where(x => x.Type == KalturaRuleActionType.BLOCK);
                if (ruleActionBlock != null && assetRule.Actions.Count > 1)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "actions=" + KalturaRuleActionType.BLOCK.ToString(),
                        "actions= " + KalturaRuleActionType.END_DATE_OFFSET.ToString() + "/" + KalturaRuleActionType.START_DATE_OFFSET.ToString());
                }
            }
        }

        private void ValidateAssetRuleConditions(List<KalturaCondition> conditions)
        {
            bool countryConditionExist = false;

            if (conditions != null)
            {
                foreach (var condition in conditions)
                {
                    if (condition is KalturaCountryCondition)
                    {
                        countryConditionExist = true;
                        KalturaCountryCondition kAssetCondition = condition as KalturaCountryCondition;
                        if (string.IsNullOrEmpty(kAssetCondition.Countries))
                        {
                            throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "countries");
                        }
                    }
                    else if (condition is KalturaAssetCondition)
                    {
                        KalturaAssetCondition kAssetCondition = condition as KalturaAssetCondition;
                        if (string.IsNullOrEmpty(kAssetCondition.Ksql))
                        {
                            throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "ksql");
                        }
                    }
                }

                if (!countryConditionExist)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "countries");
                }
            }
        }
    }
}