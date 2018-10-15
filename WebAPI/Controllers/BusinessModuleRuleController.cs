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
    [Service("businessModuleRule")]
    public class BusinessModuleRuleController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        ///  Get the list of business module rules for the partner
        /// </summary>
        /// <param name="filter">filter by condition name</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaBusinessModuleRuleListResponse List(KalturaBusinessModuleRuleFilter filter)
        {
            KalturaBusinessModuleRuleListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.ApiClient().GetBusinessModuleRules(groupId, filter);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Add business module rule
        /// </summary>
        /// <param name="businessModuleRule">Business module rule</param>
        [Action("add")]
        [ApiAuthorize]
        static public KalturaBusinessModuleRule Add(KalturaBusinessModuleRule businessModuleRule)
        {
            KalturaBusinessModuleRule response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                businessModuleRule.Validate();
                response = ClientsManager.ApiClient().AddBusinessModuleRule(groupId, businessModuleRule);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update business module rule
        /// </summary>
        /// <param name="businessModuleRule">Business module rule</param>
        /// <param name="id">Business module rule ID to update</param>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.RuleNotExists)] 
        static public KalturaBusinessModuleRule Update(long id, KalturaBusinessModuleRule businessModuleRule)
        {
            KalturaBusinessModuleRule response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                KalturaBusinessModuleRule oldBusinessModuleRule = ClientsManager.ApiClient().GetBusinessModuleRule(groupId, id);
                // before updating businessModuleRule fill properties in case they are empty so it will be possible to validate the new properties
                businessModuleRule.FillEmpty(oldBusinessModuleRule);
                businessModuleRule.Validate();
                
                response = ClientsManager.ApiClient().UpdateBusinessModuleRule(groupId, id, businessModuleRule);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete business module rule
        /// </summary>
        /// <param name="id">Business module rule ID</param>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.RuleNotExists)]
        static public void Delete(long id)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                ClientsManager.ApiClient().DeleteBusinessModuleRule(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }
    }
}