using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using TVPPro.SiteManager.Helper;
using System.Web.Services;
using log4net;
using TVPApiModule.Services;
using TVPPro.SiteManager.Context;
using TVPApiModule.Objects;
using System.Web;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Helper;

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
    public class BillingService : System.Web.Services.WebService//, IBillingService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(BillingService));

        #region public methods

        [WebMethod(EnableSession = true, Description = "Get last billing Info")]
        public TVPApiModule.Objects.Responses.AdyenBillingDetail GetLastBillingUserInfo(InitializationObject initObj, string siteGuid, int billingMethod)
        {
            TVPApiModule.Objects.Responses.AdyenBillingDetail response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetLastBillingUserInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiBillingService(groupID, initObj.Platform).GetLastBillingUserInfo(siteGuid, billingMethod);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);                    
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");                    
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        #endregion
    }
}
