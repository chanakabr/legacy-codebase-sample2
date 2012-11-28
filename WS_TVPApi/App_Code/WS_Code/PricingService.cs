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
    public class PricingService : System.Web.Services.WebService, IPricingService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(PricingService));

        #region public methods

        [WebMethod(EnableSession = true, Description = "Get PPV Module data")]
        public TVPPro.SiteManager.TvinciPlatform.Pricing.PPVModule GetPPVModuleData(InitializationObject initObj, int ppvCode)
        {
            TVPPro.SiteManager.TvinciPlatform.Pricing.PPVModule response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPPVModuleData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetPPVModuleData-> [{0}, {1}]", groupID, initObj.Platform);

            if (groupID > 0)
            {
                try
                {
                    response = new ApiPricingService(groupID, initObj.Platform).GetPPVModuleData(ppvCode, string.Empty, string.Empty, initObj.UDID);
                }
                catch (Exception ex)
                {
                    logger.Error("GetPPVModuleData->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetPPVModuleData-> 'Unknown group' Username: {0}, Password: {1}, PPVCode: {2}", initObj.ApiUser, initObj.ApiPass, ppvCode);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Get all subscriptions contains media file")]
        public TVPPro.SiteManager.TvinciPlatform.Pricing.Subscription[] GetSubscriptionsContainingMediaFile(InitializationObject initObj, int iMediaID, int iFileID)
        {
            TVPPro.SiteManager.TvinciPlatform.Pricing.Subscription[] subs = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionsContainingMedia", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetSubscriptionsContainingMedia-> [{0}, {1}], Params:[mediaId: {2}, fileId: {3}]", groupId, initObj.Platform, iFileID);

            if (groupId > 0)
            {
                try
                {
                    subs = new ApiPricingService(groupId, initObj.Platform).GetSubscriptionsContainingMediaFile(iMediaID, iFileID);
                }
                catch (Exception ex)
                {
                    logger.Error("GetSubscriptionsContainingMedia->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetSubscriptionsContainingMedia-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return subs;
        }

        #endregion
    }
}
