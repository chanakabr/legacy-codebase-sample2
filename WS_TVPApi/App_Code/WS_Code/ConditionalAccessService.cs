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
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;

namespace TVPApiServices
{
    [WebService(Namespace = "http://platform-us.tvinci.com/tvpapi/ws")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class ConditionalAccessService : System.Web.Services.WebService, IConditionalAccessService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(ConditionalAccessService));

        [WebMethod(EnableSession = true, Description = "Activate Campaign with information")]
        public TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.CampaignActionInfo ActivateCampaignWithInfo(InitializationObject initObj, long campID, string hashCode, int mediaID, string mediaLink,
                                                                                                                string senderEmail, string senderName, CampaignActionResult status, VoucherReceipentInfo[] voucherReceipents)
        {
            CampaignActionInfo campaignActionInfo = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ActivateCampaignWithInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("ActivateCampaignWithInfo-> [{0}, {1}], Params:[CampID: {2} , hashCode: {3} , mediaID: {4} , mediaLink: {5} , senderEmail: {6} , senderName: {7} , status: {8}]", groupId, initObj.Platform, campID, hashCode, mediaID, mediaLink, senderEmail, senderName, status);
            if (groupId > 0)
            {
                try
                {
                    campaignActionInfo = new ApiConditionalAccessService(groupId, initObj.Platform).ActivateCampaignWithInfo(initObj.SiteGuid, campID, hashCode, mediaID, mediaLink, senderEmail, senderName, status, voucherReceipents);
                }
                catch (Exception ex)
                {
                    logger.Error("ActivateCampaignWithInfo->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("ActivateCampaignWithInfo-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }
            return campaignActionInfo;
        }
    }
}
