using ApiObjects.Response;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.ModelsValidators;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("assetRule")]
    public class AssetRuleController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        ///  Get the list of asset rules for the partner
        /// </summary>
        /// <param name="filter">filter by condition name</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetRuleNotExists)]
        static public KalturaAssetRuleListResponse List(KalturaAssetRuleFilter filter = null)
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

                if (filter.AssetRuleIdEqual.HasValue)
                {
                    var assetRule = ClientsManager.ApiClient().GetAssetRule(groupId, filter.AssetRuleIdEqual.Value);
                    if (assetRule != null)
                    {
                        response = new KalturaAssetRuleListResponse();
                        response.Objects = new List<KalturaAssetRule>();
                        response.Objects.Add(assetRule);
                        response.TotalCount = 1;
                    }
                }
                else
                {
                    response = ClientsManager.ApiClient().GetAssetRules(groupId, filter);
                }
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
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.VideoCodecsDoesNotExist)]
        [Throws(eResponseStatus.AudioCodecsDoesNotExist)]
        [Throws(eResponseStatus.LabelDoesNotExist)]
        [Throws(eResponseStatus.DynamicDataKeyDoesNotExist)]
        static public KalturaAssetRule Add(KalturaAssetRule assetRule)
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
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetRuleNotExists)]
        [Throws(eResponseStatus.AssetRuleStatusNotWritable)]
        [Throws(eResponseStatus.VideoCodecsDoesNotExist)]
        [Throws(eResponseStatus.AudioCodecsDoesNotExist)]
        [Throws(eResponseStatus.LabelDoesNotExist)]
        [Throws(eResponseStatus.DynamicDataKeyDoesNotExist)]
        static public KalturaAssetRule Update(long id, KalturaAssetRule assetRule)
        {
            KalturaAssetRule response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                var oldAssetRule = ClientsManager.ApiClient().GetAssetRule(groupId, id);
                if (oldAssetRule.Status == KalturaAssetRuleStatus.IN_PROGRESS)
                {
                    throw new ClientException((int)eResponseStatus.AssetRuleStatusNotWritable, "Cannot update or delete asset rule when in progress");
                }
                // before updating AssetRule fill properties in case they are empty so it will be possible to validate the new properties
                FillEmpty(oldAssetRule, assetRule);
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
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetRuleNotExists)]
        [Throws(eResponseStatus.AssetRuleStatusNotWritable)]
        static public bool Delete(long id)
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

        /// <summary>
        /// Fill current AssetRule data members with givven assetRule only if they are empty\null
        /// </summary>
        /// <param name="oldAssetRule">givven assetRule to fill with</param>
        private static void FillEmpty(KalturaAssetRule oldAssetRule, KalturaAssetRule newAssetRule)
        {
            if (oldAssetRule != null)
            {
                if (string.IsNullOrEmpty(newAssetRule.Name) || string.IsNullOrWhiteSpace(newAssetRule.Name))
                {
                    newAssetRule.Name = oldAssetRule.Name;
                }

                if (newAssetRule.NullableProperties != null && newAssetRule.NullableProperties.Contains("description"))
                {
                    newAssetRule.Description = string.Empty;
                }
                else if (string.IsNullOrEmpty(newAssetRule.Description) || string.IsNullOrWhiteSpace(newAssetRule.Description))
                {
                    newAssetRule.Description = oldAssetRule.Description;
                }

                if (newAssetRule.Actions == null || newAssetRule.Actions.Count == 0)
                {
                    newAssetRule.Actions = oldAssetRule.Actions;
                }

                if (newAssetRule.Conditions == null || newAssetRule.Conditions.Count == 0)
                {
                    newAssetRule.Conditions = oldAssetRule.Conditions;
                }
            }
        }
    }
}