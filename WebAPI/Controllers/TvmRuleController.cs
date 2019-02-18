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
    [Service("tvmRule")]
    public class TvmRuleController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        ///  Get the list of tvm rules for the partner
        /// </summary>
        /// <param name="filter">TvmRuleFilter Filter</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        public static KalturaTvmRuleListResponse List(KalturaTvmRuleFilter filter = null)
        {
            KalturaTvmRuleListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            if (filter == null)
            {
                filter = new KalturaTvmRuleFilter();
            }

            try
            {
                response = ClientsManager.ApiClient().GetTvmRules(groupId, filter.RuleTypeEqual, filter.NameEqual);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}