using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using TVPPro.SiteManager.Helper;
using System.Web.Services;
using TVPApiModule.Services;
using TVPPro.SiteManager.Context;
using TVPApiModule.Objects;
using TVPPro.SiteManager.TvinciPlatform.Domains;
using TVPPro.SiteManager.TvinciPlatform.Billing;
using System.Web;
using TVPApiModule.Manager;
using TVPApiModule.Objects.Authorization;
using KLogMonitor;
using System.Reflection;

namespace TVPApiServices
{
    /// <summary>
    /// Summary description for Service
    /// </summary>
    [WebService(Namespace = "http://platform-us.tvinci.com/tvpapi/ws")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class BillingService : System.Web.Services.WebService, IBillingService
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region public methods

        [WebMethod(EnableSession = true, Description = "Get last billing Info")]
        [PrivateMethod]
        public AdyenBillingDetail GetLastBillingUserInfo(InitializationObject initObj, int billingMethod)
        {
            AdyenBillingDetail response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetLastBillingUserInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiBillingService(groupID, initObj.Platform).GetLastBillingUserInfo(initObj.SiteGuid, billingMethod);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "")]
        public string GetClientMerchantSig(InitializationObject initObj, string sParamaters)
        {
            string response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetClientMerchantSig", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiBillingService(groupID, initObj.Platform).GetClientMerchantSig(sParamaters);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "")]
        [PrivateMethod]
        public AdyenBillingDetail GetLastBillingTypeUserInfo(InitializationObject initObj, string sSiteGuid)
        {
            TVPPro.SiteManager.TvinciPlatform.Billing.AdyenBillingDetail response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetClientMerchantSig", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: validate siteGuid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, sSiteGuid, 0, null, groupID, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    response = new TVPApiModule.Services.ApiBillingService(groupID, initObj.Platform).GetLastBillingTypeUserInfo(sSiteGuid);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        #endregion
    }
}
