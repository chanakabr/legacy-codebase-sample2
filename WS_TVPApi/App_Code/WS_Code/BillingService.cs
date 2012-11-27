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
using TVPPro.SiteManager.TvinciPlatform.Domains;
using TVPPro.SiteManager.TvinciPlatform.Billing;

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
        private readonly ILog logger = LogManager.GetLogger(typeof(BillingService));

        #region public methods

        [WebMethod(EnableSession = true, Description = "Get last billing Info")]
        public AdyenBillingDetail GetLastBillingUserInfo(InitializationObject initObj, int billingMethod)
        {
            AdyenBillingDetail response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetLastBillingUserInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetLastBillingUserInfo-> [{0}, {1}]", groupID, initObj.Platform);

            if (groupID > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiBillingService(groupID, initObj.Platform).GetLastBillingUserInfo(initObj.SiteGuid, billingMethod);
                }
                catch (Exception ex)
                {
                    logger.Error("GetLastBillingUserInfo->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetLastBillingUserInfo-> 'Unknown group' Username: {0}, Password: {1}, siteGuid: {2}", initObj.ApiUser, initObj.ApiPass, initObj.SiteGuid);
            }

            return response;
        }

        #endregion
    }
}
