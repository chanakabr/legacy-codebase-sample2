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
    [Service("assetUserRule")]
    public class AssetUserRuleController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        ///  Get the list of asset user rules for the partner
        /// </summary>
        /// <param name="filter">AssetUserRule Filter</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetUserRulesOperationsDisable)]
        static public KalturaAssetUserRuleListResponse List(KalturaAssetUserRuleFilter filter = null)
        {
            KalturaAssetUserRuleListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (filter != null && filter.AttachedUserIdEqualCurrent.HasValue && filter.AttachedUserIdEqualCurrent.Value)
                {
                    long userId = long.Parse(KS.GetFromRequest().UserId);
                    response = response = ClientsManager.ApiClient().GetAssetUserRules(groupId, userId);
                }
                else
                {
                    response = ClientsManager.ApiClient().GetAssetUserRules(groupId);
                }
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
        [Action("add")]
        [ApiAuthorize]
        static public KalturaAssetUserRule Add(KalturaAssetUserRule assetUserRule)
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
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetUserRuleDoesNotExists)]
        [SchemeArgument("id", MinLong = 1)]
        static public KalturaAssetUserRule Update(long id, KalturaAssetUserRule assetUserRule)
        {
            KalturaAssetUserRule response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (assetUserRule.Conditions != null && assetUserRule.Conditions.Count > 0)
                {
                    assetUserRule.ValidateConditions();
                }

                if (assetUserRule.Actions != null && assetUserRule.Actions.Count > 0)
                {
                    assetUserRule.ValidateActions();
                }

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
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetUserRuleDoesNotExists)]
        [SchemeArgument("id", MinLong = 1)]
        static public void Delete(long id)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                ClientsManager.ApiClient().DeleteAssetUserRule(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }

        /// <summary>
        /// Attach AssetUserRule To User
        /// </summary>
        /// <param name="ruleId">AssetUserRule id to add</param>
        [Action("attachUser")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.AssetUserRuleDoesNotExists)]
        [Throws(eResponseStatus.UserAlreadyAttachedToAssetUserRule)]
        [Throws(eResponseStatus.AssetUserRulesOperationsDisable)]
        [SchemeArgument("ruleId", MinLong = 1)]
        static public void AttachUser(long ruleId)
        {
            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            try
            {
                ClientsManager.ApiClient().AddAssetUserRuleToUser(long.Parse(userId), ruleId, groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }

        /// <summary>
        /// Detach AssetUserRule from user
        /// </summary>
        /// <param name="ruleId">AssetUserRule id to remove</param>
        [Action("detachUser")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.AssetUserRuleDoesNotExists)]
        [Throws(eResponseStatus.AssetUserRulesOperationsDisable)]
        [SchemeArgument("ruleId", MinLong = 1)]
        static public void DetachUser(long ruleId)
        {
            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            try
            {
                ClientsManager.ApiClient().DeleteAssetUserRuleFromUser(long.Parse(userId), ruleId, groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }
    }
}